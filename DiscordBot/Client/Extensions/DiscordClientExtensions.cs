using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot
{
    public static class DiscordClientExtensions
    {
        public static async Task<IUser> GetUserAsync(this DiscordSocketClient client, ulong id, CacheMode mode = CacheMode.AllowDownload)
        {
            IUser user = client.GetUser(id);
            if (user == null && mode == CacheMode.AllowDownload)
                return await client.Rest.GetUserAsync(id);
            return user;
        }
    }
}
