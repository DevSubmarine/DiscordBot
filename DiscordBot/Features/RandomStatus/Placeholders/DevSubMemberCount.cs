using Discord;

namespace DevSubmarine.DiscordBot.RandomStatus.Placeholders
{
    [StatusPlaceholder("{{DevSubMemberCount}}")]
    internal class DevSubMemberCount : IStatusPlaceholder
    {
        private readonly IDiscordClient _client;
        private readonly DevSubOptions _options;

        private int _count;

        public DevSubMemberCount(IDiscordClient client, IOptions<DevSubOptions> options)
        {
            this._client = client;
            this._options = options.Value;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (this._count > 0)
                return this._count.ToString();

            IGuild guild = await this._client.GetGuildAsync(this._options.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IEnumerable<IGuildUser> users = await guild.GetUsersAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            this._count = users.Count();
            return this._count.ToString();
        }
    }
}
