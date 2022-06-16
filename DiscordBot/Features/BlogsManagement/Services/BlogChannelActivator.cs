using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    internal class BlogChannelActivator : IBlogChannelActivator
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogChannelActivator(DiscordSocketClient client, ILogger<BlogChannelActivator> log, IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._client = client;
            this._log = log;
            this._options = options;
        }

        public Task ActivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default)
            => this.MoveChannelAsync(channelID, this.Options.InactiveBlogsCategoryID, this.Options.ActiveBlogsCategoryID, cancellationToken);

        public Task DeactivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default)
            => this.MoveChannelAsync(channelID, this.Options.ActiveBlogsCategoryID, this.Options.InactiveBlogsCategoryID, cancellationToken);

#pragma warning disable CA2017 // Parameter count mismatch
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
                { "GuildID", guild.Id },
                { "GuildName", guild.Name },
                { "ChannelID", channel.Id },
                { "ChannelName", channel.Name },
                { "SourceCategoryID", sourceCategory.Id },
                { "SourceCategoryName", sourceCategory.Name },
                { "TargetCategoryID", targetCategory.Id },
                { "TargetCategoryName", targetCategory.Name }
            });

            if (channel.CategoryId == targetCategoryID)
            {
                this._log.LogDebug("Channel {ChannelName} is already in category {TargetCategoryName} ({TargetCategoryID})");
                return;
            }
            if (channel.CategoryId != sourceCategoryID)
                this._log.LogWarning("Channel {ChannelName} is not in category {SourceCategoryName} ({SourceCategoryID})");

            this._log.LogInformation("Moving channel {ChannelName} to category {TargetCategoryName} ({TargetCategoryID})");
            await channel.ModifyAsync(options => options.CategoryId = targetCategory.Id, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            this._log.LogDebug("Channel {ChannelName} moved to category {TargetCategoryName} ({TargetCategoryID})");
        }
#pragma warning restore CA2017 // Parameter count mismatch
    }
}
