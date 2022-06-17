using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    /// <summary>Service responsible for sorting channels in a category.</summary>
    public interface IBlogChannelSorter
    {
        /// <summary>Sorts channels.</summary>
        /// <param name="category">Category to sort channels in.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task SortChannelsAsync(SocketCategoryChannel category, CancellationToken cancellationToken = default);
    }
}
