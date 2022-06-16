using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    internal class BlogChannelSorter : IBlogChannelSorter
    {
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;

        public BlogChannelSorter(ILogger<BlogActivityScanner> log, IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._log = log;
            this._options = options;
        }

#pragma warning disable CA2017 // Parameter count mismatch
        public Task SortChannelsAsync(SocketCategoryChannel category, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
            {
                { "GuildID", category.Guild.Id },
                { "GuildName", category.Guild.Name },
                { "CategoryID", category.Id },
                { "CategoryName", category.Name }
            });

            this._log.LogInformation("Sorting channels in category {CategoryName} ({CategoryID})");
            IOrderedEnumerable<SocketGuildChannel> channels = category.Channels
                .OrderBy(c => !this._options.CurrentValue.IgnoredChannelsIDs.Contains(c.Id))
                .ThenBy(c => c.Name);

            IDictionary<SocketGuildChannel, int> channelPositions = new Dictionary<SocketGuildChannel, int>(channels.Count());
            int nextPosition = category.Channels.Min(c => c.Position);

            foreach (SocketGuildChannel channel in channels)
            {
                channelPositions[channel] = nextPosition;
                nextPosition++;
            }

            return category.Guild.ReorderChannelsAsync(
                channelPositions.Select(pair => new ReorderChannelProperties(pair.Key.Id, pair.Value)),
                new RequestOptions() { CancelToken = cancellationToken });
        }
#pragma warning restore CA2017 // Parameter count mismatch
    }
}
