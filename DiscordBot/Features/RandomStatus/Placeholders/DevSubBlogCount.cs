using DevSubmarine.DiscordBot.BlogsManagement;
using Discord;

namespace DevSubmarine.DiscordBot.RandomStatus.Placeholders
{
    [StatusPlaceholder($"{{{{DevSubBlogCount({_activeTag}|{_inactiveTag})?}}}}")]
    internal class DevSubBlogCount : IStatusPlaceholder
    {
        private const string _activeTag = ":active";
        private const string _inactiveTag = ":inactive";

        private readonly IDiscordClient _client;
        private readonly DevSubOptions _options;
        private readonly BlogsManagementOptions _blogOptions;

        private int _activeCount;
        private int _inactiveCount;

        public DevSubBlogCount(IDiscordClient client, IOptionsSnapshot<DevSubOptions> options, IOptionsSnapshot<BlogsManagementOptions> blogOptions)
        {
            this._client = client;
            this._options = options.Value;
            this._blogOptions = blogOptions.Value;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (this._activeCount == 0 && this._inactiveCount == 0)
            {
                IGuild guild = await this._client.GetGuildAsync(this._options.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                IEnumerable<ITextChannel> channels = await guild.GetTextChannelsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                channels = channels.Where(ch => !this._blogOptions.IgnoredChannelsIDs.Contains(ch.Id));

                this._activeCount = channels.Where(ch => ch.CategoryId == this._blogOptions.ActiveBlogsCategoryID).Count();
                this._inactiveCount = channels.Where(ch => ch.CategoryId == this._blogOptions.InactiveBlogsCategoryID).Count();
            }

            int result = this._activeCount + this._inactiveCount;

            if (this.HasTag(placeholder, _activeTag))
                result = this._activeCount;
            else if (this.HasTag(placeholder, _inactiveTag))
                result = this._inactiveCount;

            return result.ToString();
        }

        private bool HasTag(Match placeholder, string tag)
            => placeholder.Groups.Count > 0 && placeholder.Groups[1].Value == tag;
    }
}
