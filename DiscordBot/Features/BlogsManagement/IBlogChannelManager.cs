using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogChannelManager
    {
        Task<IEnumerable<IGuildChannel>> GetBlogChannelsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<IGuildChannel>> FindUserBlogChannelsAsync(ulong userID, CancellationToken cancellationToken = default);
        Task<IGuildChannel> CreateBlogChannel(string name, IEnumerable<ulong> userIDs, CancellationToken cancellationToken = default);
    }
}
