using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot
{
    public static class DiscordClientExtensions
    {
        public static Task<IUser> GetUserAsync(this DiscordSocketClient client, ulong id, CancellationToken cancellationToken = default)
            => client.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken);

        public static async Task<IUser> GetUserAsync(this DiscordSocketClient client, ulong id, CacheMode mode, CancellationToken cancellationToken = default)
        {
            IUser user = await client.GetUserAsync(id, cancellationToken.ToRequestOptions());
            if (user == null && mode == CacheMode.AllowDownload)
                return await client.Rest.GetUserAsync(id, cancellationToken.ToRequestOptions());
            return user;
        }
    }
}
