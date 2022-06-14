using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    internal class BlogChannelsActivator : IBlogChannelsActivator
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogChannelsActivator(DiscordSocketClient client, ILogger<BlogChannelsActivator> log, IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._client = client;
            this._log = log;
            this._options = options;
        }

        public Task ActivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default)
            => this.MoveChannelAsync(channelID, this.Options.InactiveBlogsCategoryID, this.Options.ActiveBlogsCategoryID, cancellationToken);

        public Task DeactivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default)
            => this.MoveChannelAsync(channelID, this.Options.ActiveBlogsCategoryID, this.Options.InactiveBlogsCategoryID, cancellationToken);

        private async Task MoveChannelAsync(ulong channelID, ulong fromCategoryID, ulong toCategoryID, CancellationToken cancellationToken = default)
        {
            if (channelID == default)
                throw new ArgumentException($"{channelID} is not a valid channel ID", nameof(channelID));


            if (this.IsChannelIgnored(channelID))
            {
                this._log.LogDebug("Channel {ChannelID} is ignored", channelID);
                return;
            }


            SocketGuild guild = this._client.GetGuild(this.Options.GuildID);
            SocketTextChannel channel = guild.GetTextChannel(channelID);

            if (channel.CategoryId == toCategoryID)
            {
                this._log.LogDebug("Channel {ChannelID} is already in category {CategoryID}", channelID, toCategoryID);
                return;
            }
            if (channel.CategoryId != fromCategoryID)
                this._log.LogWarning("Channel {ChannelID} is not in category {CategoryID}", channelID, fromCategoryID);


            this._log.LogInformation("Moving channel {ChannelID} to category {CategoryID}", channel, toCategoryID);
            await channel.ModifyAsync(options => options.CategoryId = toCategoryID,
                new RequestOptions() { CancelToken = cancellationToken });
            this._log.LogDebug("Channel {ChannelID} moved to category {CategoryID}", channel, toCategoryID);
        }

        private bool IsChannelIgnored(ulong channelID)
            => this.Options.IgnoredChannelsIDs.Contains(channelID);
    }
}
