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

        private async Task MoveChannelAsync(ulong channelID, ulong sourceCategoryID, ulong targetCategoryID, CancellationToken cancellationToken = default)
        {
            if (channelID == default)
                throw new ArgumentException($"{channelID} is not a valid channel ID", nameof(channelID));

            SocketGuild guild = this._client.GetGuild(this.Options.GuildID);
            SocketTextChannel channel = guild.GetTextChannel(channelID);
            SocketCategoryChannel sourceCategory = guild.GetCategoryChannel(targetCategoryID);
            SocketCategoryChannel targetCategory = guild.GetCategoryChannel(targetCategoryID);
            using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
            {
                { "GuildID", channel.Guild.Id },
                { "GuildName", channel.Guild.Name },
                { "ChannelID", channel.Id },
                { "ChannelName", channel.Name }
            });

            if (channel.CategoryId == targetCategoryID)
            {
                this._log.LogDebug("Channel {ChannelID} is already in category {CategoryName} ({CategoryID})", channel, targetCategory.Name, targetCategory.Id);
                return;
            }
            if (channel.CategoryId != sourceCategoryID)
                this._log.LogWarning("Channel {ChannelID} is not in category {CategoryName} ({CategoryID})", channelID, sourceCategory.Name, sourceCategory.Id);

            this._log.LogInformation("Moving channel {ChannelID} to category {CategoryName} ({CategoryID})", channel, targetCategory.Name, targetCategory.Id);
            await channel.ModifyAsync(options => options.CategoryId = targetCategory.Id,
                new RequestOptions() { CancelToken = cancellationToken });
            this._log.LogDebug("Channel {ChannelID} moved to category {CategoryName} ({CategoryID})", channel, targetCategory.Name, targetCategory.Id);
        }

        private bool IsChannelIgnored(ulong channelID)
            => this.Options.IgnoredChannelsIDs.Contains(channelID);
    }
}
