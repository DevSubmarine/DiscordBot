using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogChannelSorter
    {
        Task SortChannelsAsync(SocketCategoryChannel category, CancellationToken cancellationToken = default);
    }
}
