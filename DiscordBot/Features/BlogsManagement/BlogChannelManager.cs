using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    internal class BlogChannelManager : IBlogChannelManager
    {
        private readonly IBlogChannelSorter _sorter;
        private readonly DiscordSocketClient _client;
        private readonly ILogger _log;
        private readonly BlogsManagementOptions _options;

        public BlogChannelManager(IBlogChannelSorter sorter, DiscordSocketClient client,
            ILogger<BlogChannelManager> log, IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._sorter = sorter;
            this._client = client;
            this._log = log;
            this._options = options.CurrentValue;
        }

        public Task<IEnumerable<IGuildChannel>> GetBlogChannelsAsync(CancellationToken cancellationToken = default)
        {
            SocketGuild guild = this._client.GetGuild(this._options.GuildID);
            IEnumerable<IGuildChannel> channels = guild.Channels
                .Where(channel => channel is SocketTextChannel)
                .Cast<SocketTextChannel>()
                .Where(channel => channel.CategoryId != null 
                    && (channel.CategoryId == this._options.ActiveBlogsCategoryID 
                        || channel.CategoryId == this._options.InactiveBlogsCategoryID));
            return Task.FromResult(channels);
        }

        public async Task<IEnumerable<IGuildChannel>> FindUserBlogChannelsAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            IEnumerable<IGuildChannel> channels = await this.GetBlogChannelsAsync(cancellationToken).ConfigureAwait(false);
            channels = channels.Where(channel => !this._options.IgnoredChannelsIDs.Contains(channel.Id));

            // yes, this can be done with LINQ, but using loop makes it easier to trace what's going on
            List<IGuildChannel> results = new List<IGuildChannel>(channels.Count());
            foreach (IGuildChannel channel in channels)
            {
                IEnumerable<IGuildUser> channelUsers = await channel.GetUsersAsync(CacheMode.AllowDownload, new RequestOptions() { CancelToken = cancellationToken }).FlattenAsync().ConfigureAwait(false);
                IGuildUser user = channelUsers.FirstOrDefault(u => u.Id == userID);
                if (user == null)
                    continue;

                // NOTE: this permissions check will fail for users with admin permissions
                // this is currently okay, only one person should have admin perms
                // but if this ever changes, well, different mechanism will be needed
                ChannelPermissions perms = user.GetPermissions(channel);
                if (perms.SendMessages)
                    results.Add(channel);
            }
            return results;
        }

        public async Task<IGuildChannel> CreateBlogChannel(string name, IEnumerable<ulong> userIDs, CancellationToken cancellationToken = default)
        {
            this._log.LogInformation("Creating blog channel {ChannelName} for user(s) {UserIDs}", name, string.Join(", ", userIDs));

            SocketGuild guild = this._client.GetGuild(this._options.GuildID);
            SocketCategoryChannel category = guild.GetCategoryChannel(this._options.ActiveBlogsCategoryID);

            IGuildChannel result = await guild.CreateTextChannelAsync(name, props =>
            {
                props.CategoryId = category.Id;

                this._log.LogTrace("Calculating permissions");
                // discord perm overwrites don't merge with each other
                // so each overwrite is to be created by modifying @everyone perms from the category
                // this is so we can both respect category permissions, as well as assign our own overrides
                OverwritePermissions categoryPerms = category.PermissionOverwrites.First(p
                    => p.TargetId == guild.EveryoneRole.Id && p.TargetType == PermissionTarget.Role)
                    .Permissions;

                List<Overwrite> perms = new List<Overwrite>(category.PermissionOverwrites);
                props.PermissionOverwrites = perms;
                perms.Add(new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                            categoryPerms.Modify(sendMessages: PermValue.Deny)));
                foreach (ulong uid in userIDs)
                {
                    perms.Add(new Overwrite(uid, PermissionTarget.User, categoryPerms.Modify(
                        sendMessages: PermValue.Allow,
                        manageMessages: PermValue.Allow,
                        manageWebhooks: PermValue.Allow
                    )));
                }
            },
                new RequestOptions() { CancelToken = cancellationToken });

            this._log.LogTrace("Sorting channels");
            await this._sorter.SortChannelsAsync(category, cancellationToken).ConfigureAwait(false);

            this._log.LogDebug("Channel {ChannelName} ({ChannelID}) created", result.Name, result.Id);
            return result;
        }
    }
}
