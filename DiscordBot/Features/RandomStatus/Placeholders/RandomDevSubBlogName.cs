using DevSubmarine.DiscordBot.BlogsManagement;
using Discord;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.RandomStatus.Placeholders
{
    [StatusPlaceholder($"{{{{RandomDevSubBlogName({_activeTag}|{_inactiveTag})?}}}}")]
    internal class RandomDevSubBlogName : IStatusPlaceholder
    {
        private const string _activeTag = ":active";
        private const string _inactiveTag = ":inactive";

        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;
        private readonly DevSubOptions _options;
        private readonly BlogsManagementOptions _blogOptions;

        public RandomDevSubBlogName(IDiscordClient client, IRandomizer randomizer,
            IOptionsSnapshot<DevSubOptions> options, IOptionsSnapshot<BlogsManagementOptions> blogOptions)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._options = options.Value;
            this._blogOptions = blogOptions.Value;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            IGuild guild = await this._client.GetGuildAsync(this._options.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IEnumerable<ITextChannel> channels = await guild.GetTextChannelsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            channels = channels.Where(ch => !this._blogOptions.IgnoredChannelsIDs.Contains(ch.Id));

            if (this.HasTag(placeholder, _activeTag))
                channels = channels.Where(ch => ch.CategoryId == this._blogOptions.ActiveBlogsCategoryID);
            else if (this.HasTag(placeholder, _inactiveTag))
                channels = channels.Where(ch => ch.CategoryId == this._blogOptions.InactiveBlogsCategoryID);

            if (!channels.Any())
                throw new InvalidOperationException($"No applicable channel found; Tag = {(placeholder.Groups[1].Success ? placeholder.Groups[1].Value : null)}");

            ITextChannel randomChannel = this._randomizer.GetRandomValue(channels);
            return randomChannel.Name;
        }

        private bool HasTag(Match placeholder, string tag)
            => placeholder.Groups[1].Success && placeholder.Groups[1].Value == tag;
    }
}
