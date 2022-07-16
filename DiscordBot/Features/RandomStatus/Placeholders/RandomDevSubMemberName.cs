using Discord;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.RandomStatus.Placeholders
{
    [StatusPlaceholder("{{RandomDevSubMemberName}}")]
    internal class RandomDevSubMemberName : IStatusPlaceholder
    {
        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;
        private readonly DevSubOptions _options;

        public RandomDevSubMemberName(IDiscordClient client, IRandomizer randomizer, IOptionsSnapshot<DevSubOptions> options)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._options = options.Value;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            IGuild guild = await this._client.GetGuildAsync(this._options.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IEnumerable<IGuildUser> members = await guild.GetUsersAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);

            if (!members.Any())
                throw new InvalidOperationException($"Guild {this._options.GuildID} has no members to pick from");

            IGuildUser randomMember = this._randomizer.GetRandomValue(members);
            return randomMember.Nickname ?? randomMember.Username;
        }
    }
}
