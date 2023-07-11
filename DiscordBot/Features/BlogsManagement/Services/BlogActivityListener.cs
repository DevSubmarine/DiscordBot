using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <summary>Background service that will activate inactive blog channels whenever a new message has been posted.</summary>
    internal class BlogActivityListener : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IBlogChannelActivator _activator;
        private readonly IBlogChannelSorter _sorter;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;
        private readonly IOptionsMonitor<DevSubOptions> _devsubOptions;
        private CancellationTokenSource _cts;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogActivityListener(IBlogChannelActivator activator, IBlogChannelSorter sorter, DiscordSocketClient client,
            ILogger<BlogActivityListener> log, IOptionsMonitor<BlogsManagementOptions> options, IOptionsMonitor<DevSubOptions> devsubOptions)
        {
            this._activator = activator;
            this._sorter = sorter;
            this._client = client;
            this._log = log;
            this._options = options;
            this._devsubOptions = devsubOptions;

            this._client.MessageReceived += this.OnClientMessageReceived;
        }

        private async Task OnClientMessageReceived(SocketMessage message)
        {
            if (message.Channel is not SocketTextChannel channel)
                return;
            if (channel.Guild.Id != this._devsubOptions.CurrentValue.GuildID)
                return;
            if (channel.CategoryId != this.Options.InactiveBlogsCategoryID)
                return;
            if (this.Options.IgnoredChannelsIDs.Contains(channel.Id))
                return;

            using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
            {
                { "GuildID", channel.Guild.Id },
                { "GuildName", channel.Guild.Name },
                { "ChannelID", channel.Id },
                { "ChannelName", channel.Name }
            });

            this._log.LogInformation("Message received from inactive blog channel {ChannelName} ({ChannelID})", channel.Name, channel.Id);
            try
            {
                CancellationToken cancellationToken = this._cts.Token;
                await this._activator.ActivateBlogChannel(channel, cancellationToken).ConfigureAwait(false);

                // offload sorting the category to a background task as it seems it might hang the connection task?
                SocketCategoryChannel categoryToSort = channel.Guild.GetCategoryChannel(this.Options.ActiveBlogsCategoryID);
                _ = this.SortChannelsAsync(categoryToSort, cancellationToken);
            }
            catch (HttpException ex) when (ex.IsMissingPermissions() && ex.LogAsError(this._log, "Failed moving {ChannelName} ({ChannelID}) due to missing permissions", channel.Name, channel.Id)) { }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed moving channel {ChannelName} ({ChannelID})", channel.Name, channel.Id)) { }
        }

        private async Task SortChannelsAsync(SocketCategoryChannel category, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5));
                await this._sorter.SortChannelsAsync(category, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpException ex) when (ex.IsMissingPermissions() && ex.LogAsError(this._log, "Failed sorting category {CategoryName} ({CategoryID}) due to missing permissions", category.Name, category.Id)) { }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed sorting category {CategoryName} ({CategoryID})", category.Name, category.Id)) { }
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            try { this._cts?.Cancel(); } catch { }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { this._client.MessageReceived -= this.OnClientMessageReceived; } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
