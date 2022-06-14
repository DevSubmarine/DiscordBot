using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogChannelsSorter
    {
        Task SortChannelsAsync(SocketCategoryChannel category, CancellationToken cancellationToken = default);
    }
}
