using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <summary>Background service that periodically scans blog channels for last activity and activates or deactivates them.</summary>
    internal class BlogActivityScanner : IHostedService, IDisposable
    {
        private readonly IHostedDiscordClient _client;
        private readonly IBlogChannelActivator _activator;
        private readonly IBlogChannelSorter _sorter;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;
        private readonly IOptionsMonitor<DevSubOptions> _devsubOptions;
        private CancellationTokenSource _cts;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogActivityScanner(IBlogChannelActivator activator, IBlogChannelSorter sorter, IHostedDiscordClient client,
            ILogger<BlogActivityScanner> log, IOptionsMonitor<BlogsManagementOptions> options, IOptionsMonitor<DevSubOptions> devsubOptions)
        {
            this._activator = activator;
            this._sorter = sorter;
            this._client = client;
            this._log = log;
            this._options = options;
            this._devsubOptions = devsubOptions;
        }

#pragma warning disable CA2017 // Parameter count mismatch
        private async Task ScannerLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                DiscordSocketClient client = (DiscordSocketClient)this._client.Client;

                while (!cancellationToken.IsCancellationRequested)
                {
                    await this._client.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                    SocketGuild guild = client.GetGuild(this._devsubOptions.CurrentValue.GuildID);
                    using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
                    {
                        { "GuildID", guild.Id },
                        { "GuildName", guild.Name }
                    });

                    this._log.LogInformation("Scanning guild {GuildName} ({GuildID}) blog channels");
                    SocketCategoryChannel activeCategory = guild.GetCategoryChannel(this.Options.ActiveBlogsCategoryID);
                    SocketCategoryChannel inactiveCategory = guild.GetCategoryChannel(this.Options.InactiveBlogsCategoryID);
                    bool anyActiveMoved = await this.ScanCategoryAsync(activeCategory, cancellationToken).ConfigureAwait(false);
                    bool anyInactiveMoved = await this.ScanCategoryAsync(inactiveCategory, cancellationToken).ConfigureAwait(false);

                    if (anyActiveMoved)
                        await this.SortCategoryAsync(inactiveCategory, cancellationToken).ConfigureAwait(false);
                    if (anyInactiveMoved)
                        await this.SortCategoryAsync(activeCategory, cancellationToken).ConfigureAwait(false);

                    await Task.Delay(this.Options.ActivityScanningRate, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex.LogAsError(this._log, "An exception occured in blog channel scanner loop")) { }
        }

        /// <summary>Scans category, automatically moving channels as needed.</summary>
        /// <param name="category">Category to scan.</param>
        /// <param name="cancellationToken">Token to cancel operation.</param>
        /// <returns>Whether any channel has been moved.</returns>
        private async Task<bool> ScanCategoryAsync(SocketCategoryChannel category, CancellationToken cancellationToken)
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

            bool anyMoved = false;
            foreach (SocketTextChannel channel in channels)
                anyMoved |= await this.ScanChannelAsync(channel, cancellationToken).ConfigureAwait(false);

            return anyMoved;
        }

        private async Task SortCategoryAsync(SocketCategoryChannel category, CancellationToken cancellationToken)
        {
            try
            {
                await this._sorter.SortChannelsAsync(category, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpException ex) when (ex.IsMissingPermissions() && ex.LogAsError(this._log, "Failed reordering channels in category {CategoryName} ({CategoryID}) due to missing permissions")) { }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed reordering channels in category {CategoryName} ({CategoryID})")) { }
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
            catch (HttpException ex) when (ex.IsMissingPermissions() &&
                    ex.LogAsError(this._log, "Failed reading messages from channel {ChannelName} ({ChannelID}) due to missing permissions")) { return false; }
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
            catch (HttpException ex) when (ex.IsMissingPermissions() &&
                    ex.LogAsError(this._log, "Failed moving {ChannelName} ({ChannelID}) due to missing permissions")) { }
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
