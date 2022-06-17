using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public static class BlogChannelManagerExtensions
    {
        /// <summary>Gets all blog channels with given name. This INCLUDES ignored blog channels.</summary>
        /// <param name="manager">The service instance.</param>
        /// <param name="name">Name of the channel.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of all blog channels found, including ignored blog channels.</returns>
        public static async Task<IEnumerable<IGuildChannel>> GetBlogChannelsAsync(this IBlogChannelManager manager, string name, CancellationToken cancellationToken = default)
        {
            name = name.Trim();
            IEnumerable<IGuildChannel> channels = await manager.GetBlogChannelsAsync(cancellationToken).ConfigureAwait(false);
            return channels.Where(channel => channel.Name == name);
        }

        /// <summary>Creates a blog channel, and allows specified user to post in it.</summary>
        /// <remarks>This method creates channels with data as-provided. No validation is performed.</remarks>
        /// <param name="manager">The service instance.</param>
        /// <param name="name">Name of the channel to create.</param>
        /// <param name="userIDs">ID of user that should have permissions to post in the channel.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The created channel.</returns>
        public static Task<IGuildChannel> CreateBlogChannel(this IBlogChannelManager manager, string name, ulong userID, CancellationToken cancellationToken = default)
            => manager.CreateBlogChannel(name, new ulong[] { userID }, cancellationToken);
    }
}
