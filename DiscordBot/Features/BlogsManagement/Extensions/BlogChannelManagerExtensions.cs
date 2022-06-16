using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public static class BlogChannelManagerExtensions
    {
        public static async Task<IEnumerable<IGuildChannel>> GetBlogChannelsAsync(this IBlogChannelManager manager, string name, CancellationToken cancellationToken = default)
        {
            name = name.Trim();
            IEnumerable<IGuildChannel> channels = await manager.GetBlogChannelsAsync(cancellationToken).ConfigureAwait(false);
            return channels.Where(channel => channel.Name == name);
        }

        public static Task<IGuildChannel> CreateBlogChannel(this IBlogChannelManager manager, string name, ulong userID, CancellationToken cancellationToken = default)
            => manager.CreateBlogChannel(name, new ulong[] { userID }, cancellationToken);
    }
}
