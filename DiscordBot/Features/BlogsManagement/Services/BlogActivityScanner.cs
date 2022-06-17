using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <summary>Background service that periodically scans blog channels for last activity and activates or deactivates them.</summary>
    internal class BlogActivityScanner : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IBlogChannelActivator _activator;
        private readonly IBlogChannelSorter _sorter;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;
        private CancellationTokenSource _cts;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogActivityScanner(IBlogChannelActivator activator, IBlogChannelSorter sorter, DiscordSocketClient client,
            ILogger<BlogActivityScanner> log, IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._activator = activator;
            this._sorter = sorter;
            this._client = client;
            this._log = log;
            this._options = options;
        }

#pragma warning disable CA2017 // Parameter count mismatch
        private async Task ScannerLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (this._client.ConnectionState != ConnectionState.Connected)
                {
                    this._log.LogTrace("Client not connected, waiting");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
                }

                SocketGuild guild = this._client.GetGuild(this.Options.GuildID);
                using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>() 
                { 
                    { "GuildID", guild.Id },
                    { "GuildName", guild.Name }
                });

                this._log.LogInformation("Scanning guild {GuildName} ({GuildID}) blog channels");
                await this.ScanCategoryAsync(guild.GetCategoryChannel(this.Options.ActiveBlogsCategoryID), cancellationToken).ConfigureAwait(false);
                await this.ScanCategoryAsync(guild.GetCategoryChannel(this.Options.InactiveBlogsCategoryID), cancellationToken).ConfigureAwait(false);

                await Task.Delay(this.Options.ActivityScanningRate, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ScanCategoryAsync(SocketCategoryChannel category, CancellationToken cancellationToken)
        {
            using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>() 
            { 
                { "CategoryID", category.Id },
                { "CategoryName", category.Name }
            });
            this._log.LogDebug("Scanning category {CategoryName} ({CategoryID})");
            IEnumerable<SocketTextChannel> channels = category.Channels
                .Where(c => c is SocketTextChannel)
                .Cast<SocketTextChannel>();

            bool needsSorting = false;
            foreach (SocketTextChannel channel in channels)
                needsSorting |= await this.ScanChannelAsync(channel, cancellationToken).ConfigureAwait(false);

            if (needsSorting)
            {
                try
                {
                    await this._sorter.SortChannelsAsync(category, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpException ex)
                    when (ex.DiscordCode == DiscordErrorCode.MissingPermissions
                        && ex.LogAsError(this._log, "Failed reordering channels in category {CategoryName} ({CategoryID}) due to missing permissions")) { }
                catch (Exception ex)
                    when (ex.LogAsError(this._log, "Failed reordering channels in category {CategoryName} ({CategoryID})")) { }
            }
        }

        private async Task<bool> ScanChannelAsync(SocketTextChannel channel, CancellationToken cancellationToken)
        {
            using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>() 
            {
                { "ChannelID", channel.Id },
                { "ChannelName", channel.Name }
            });


            if (this.Options.IgnoredChannelsIDs.Contains(channel.Id))
                return false;

            this._log.LogDebug("Scanning channel {ChannelName} ({ChannelID})");
            IMessage lastMessage;
            try
            {
                lastMessage = (await channel
                    .GetMessagesAsync(limit: 1, cancellationToken.ToRequestOptions())
                    .FlattenAsync()
                    .ConfigureAwait(false))
                    .FirstOrDefault();
            }
            catch (HttpException ex) 
                when (ex.DiscordCode == DiscordErrorCode.MissingPermissions
                    && ex.LogAsError(this._log, "Failed reading messages from channel {ChannelName} ({ChannelID}) due to missing permissions")) { return false; }
            catch (Exception ex)
                when (ex.LogAsError(this._log, "Failed reading messages from channel {ChannelName} ({ChannelID})")) { return false; }


            try
            {
                TimeSpan inactivityLength = lastMessage != null
                    ? DateTimeOffset.UtcNow - lastMessage.Timestamp
                    : DateTimeOffset.UtcNow - channel.CreatedAt;
                bool isInactive = inactivityLength > this.Options.MaxBlogInactivityTime;

                if (isInactive && channel.CategoryId == this.Options.ActiveBlogsCategoryID)
                {
                    await this._activator.DeactivateBlogChannel(channel, cancellationToken).ConfigureAwait(false);
                    return true;
                }
                else if (!isInactive && channel.CategoryId == this.Options.InactiveBlogsCategoryID)
                {
                    await this._activator.ActivateBlogChannel(channel, cancellationToken).ConfigureAwait(false);
                    return true;
                }
                else
                    this._log.LogTrace("Channel {ChannelName} ({ChannelID}) requires no changes");
            }
            catch (HttpException ex)
                when (ex.DiscordCode == DiscordErrorCode.MissingPermissions
                    && ex.LogAsError(this._log, "Failed moving {ChannelName} ({ChannelID}) due to missing permissions")) { }
            catch (Exception ex)
                when (ex.LogAsError(this._log, "Failed moving channel {ChannelName} ({ChannelID})")) { }

            return false;
        }
#pragma warning restore CA2017 // Parameter count mismatch

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = this.ScannerLoopAsync(this._cts.Token);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            try { this._cts?.Cancel(); } catch { }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
