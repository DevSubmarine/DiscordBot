using Discord;

namespace DevSubmarine.DiscordBot
{
    public static class DiscordGuildExtensions
    {
        public static async Task<IGuildUser> GetGuildUserAsync(this IGuild guild, ulong id, CancellationToken cancellationToken = default)
        {
            IGuildUser user = await guild.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (user == null)
            {
                await guild.DownloadUsersAsync();
                user = await guild.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            }
            return user;
        }
        public static Task<IGuildUser> GetGuildUserAsync(this IGuildChannel channel, ulong id)
            => GetGuildUserAsync(channel.Guild, id);
        public static Task<IGuildUser> GetGuildUserAsync(this IGuildChannel channel, IUser user)
            => GetGuildUserAsync(channel, user.Id);
        public static Task<IGuildUser> GetGuildUserAsync(this IGuild guild, IUser user)
            => GetGuildUserAsync(guild, user.Id);
    }
}
