using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <inheritdoc/>
    public class BlogChannelActivator : IBlogChannelActivator
    {
        private readonly IDiscordClient _client;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;
        private readonly IOptionsMonitor<DevSubOptions> _devsubOptions;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogChannelActivator(IDiscordClient client, ILogger<BlogChannelActivator> log, 
            IOptionsMonitor<BlogsManagementOptions> options, IOptionsMonitor<DevSubOptions> devsubOptions)
        {
            this._client = client;
            this._log = log;
            this._options = options;
            this._devsubOptions = devsubOptions;
        }

        /// <inheritdoc/>
        public Task ActivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default)
            => this.MoveChannelAsync(channelID, this.Options.InactiveBlogsCategoryID, this.Options.ActiveBlogsCategoryID, cancellationToken);

        /// <inheritdoc/>
        public Task DeactivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default)
            => this.MoveChannelAsync(channelID, this.Options.ActiveBlogsCategoryID, this.Options.InactiveBlogsCategoryID, cancellationToken);

#pragma warning disable CA2017 // Parameter count mismatch
        private async Task MoveChannelAsync(ulong channelID, ulong sourceCategoryID, ulong targetCategoryID, CancellationToken cancellationToken = default)
        {
            if (channelID == default)
                throw new ArgumentException($"{channelID} is not a valid channel ID", nameof(channelID));

            IGuild guild = await this._client.GetGuildAsync(this._devsubOptions.CurrentValue.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
            ITextChannel channel = await guild.GetTextChannelAsync(channelID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IEnumerable<ICategoryChannel> categories = await guild.GetCategoriesAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            ICategoryChannel sourceCategory = categories.First(cat => cat.Id == sourceCategoryID);
            ICategoryChannel targetCategory = categories.First(cat => cat.Id == targetCategoryID);
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
