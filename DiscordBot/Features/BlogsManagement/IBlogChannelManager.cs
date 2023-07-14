using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    /// <summary>Service responsible for retrieving and creating blog channels.</summary>
    public interface IBlogChannelManager
    {
        /// <summary>Gets all blog channels. This INCLUDES ignored blog channels.</summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of all blog channels found, including ignored blog channels.</returns>
        Task<IEnumerable<IGuildChannel>> GetBlogChannelsAsync(CancellationToken cancellationToken = default);
        /// <summary>Finds blog channels where the user can post in. This EXCLUDES ignored blog channels.</summary>
        /// <param name="userID">ID of the user to find channels for.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of found blog channels, excluding ignored blog channels.</returns>
        Task<IEnumerable<IGuildChannel>> FindUserBlogChannelsAsync(ulong userID, CancellationToken cancellationToken = default);
        /// <summary>Creates a blog channel, and allows specified users to post in it.</summary>
        /// <remarks>This method creates channels with data as-provided. No validation is performed.</remarks>
        /// <param name="name">Name of the channel to create.</param>
        /// <param name="userIDs">IDs of users that should have permissions to post in the channel.</param>
        /// <param name="properties">Optional properties for the new channel.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The created channel.</returns>
        Task<IGuildChannel> CreateBlogChannel(string name, IEnumerable<ulong> userIDs, BlogChannelProperties properties, CancellationToken cancellationToken = default);
        /// <summary>Eits a blog channel.</summary>
        /// <remarks>This method edits channels with data as-provided. No validation is performed.</remarks>
        /// <param name="channel">Channel to edit.</param>
        /// <param name="properties">Optional properties for the channel.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The created channel.</returns>
        Task EditBlogChannel(IGuildChannel channel, BlogChannelProperties properties, CancellationToken cancellationToken = default);
    }
}
