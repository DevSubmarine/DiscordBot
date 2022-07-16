using Discord;

namespace DevSubmarine.DiscordBot.RandomStatus.Placeholders
{
    [StatusPlaceholder($"{{{{UserNickname:(\\d{{1,20}})}}}}")]
    internal class UserNickname : IStatusPlaceholder
    {
        private readonly IDiscordClient _client;
        private readonly DevSubOptions _options;

        private string _nickname;

        public UserNickname(IDiscordClient client, IOptionsSnapshot<DevSubOptions> options)
        {
            this._client = client;
            this._options = options.Value;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(this._nickname))
                return this._nickname;

            if (!placeholder.Groups[1].Success)
                throw new ArgumentException($"Placeholder requires a valid user ID to be provided");
            if (!ulong.TryParse(placeholder.Groups[1].Value, out ulong id))
                throw new ArgumentException($"Placeholder: {placeholder.Groups[1].Value} is not a valid user ID");

            // attempt to get nickname from guild - but user might not have one set or not even be in the guild
            // so fallback to normal username and/or non-member retrieval
            IGuild guild = await this._client.GetGuildAsync(this._options.GuildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guild != null)
            {
                IGuildUser guildUser = await guild.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                if (guildUser != null)
                {
                    this._nickname = this.GetName(guildUser);
                    return this._nickname;
                }
            }

            IUser user = await this._client.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (user == null)
                throw new InvalidOperationException($"Discord user with ID {id} not found");
            this._nickname = this.GetName(user);
            return this._nickname;
        }

        private string GetName(IUser user)
        {
            if (user is IGuildUser guildUser && guildUser?.Nickname != null)
                return guildUser.Nickname;
            return user.Username;
        }
    }
}
