using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    internal class BlogActivityListener : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IBlogChannelsActivator _activator;
        private readonly IBlogChannelsSorter _sorter;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<BlogsManagementOptions> _options;
        private CancellationTokenSource _cts;

        private BlogsManagementOptions Options => this._options.CurrentValue;

        public BlogActivityListener(IBlogChannelsActivator activator, IBlogChannelsSorter sorter, DiscordSocketClient client,
            ILogger<BlogActivityListener> log, IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._activator = activator;
            this._sorter = sorter;
            this._client = client;
            this._log = log;
            this._options = options;

            this._client.MessageReceived += OnClientMessageReceived;
        }

        private async Task OnClientMessageReceived(SocketMessage message)
        {
            if (message.Channel is not SocketTextChannel channel)
                return;
            if (channel.Guild.Id != this.Options.GuildID)
                return;
            if (channel.CategoryId != this.Options.InactiveBlogsCategoryID)
                return;

            this._log.LogInformation("Message received from inactive blog channel {ChannelName} ({ChannelID}, guild {GuildID})", channel.Name, channel.Id, channel.Guild.Id);
            CancellationToken cancellationToken = this._cts.Token;
            SocketCategoryChannel category = channel.Guild.GetCategoryChannel(channel.CategoryId.Value);
            try
            {
                await this._activator.ActivateBlogChannel(channel, cancellationToken).ConfigureAwait(false);
                await this._sorter.SortChannelsAsync(category, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpException ex)
                when (ex.DiscordCode == DiscordErrorCode.MissingPermissions
                    && ex.LogAsError(this._log, "Failed moving {ChannelName} ({ChannelName}, guild {GuildID}) due to missing permissions")) { }
            catch (Exception ex)
                when (ex.LogAsError(this._log, "Failed moving channel {ChannelName} ({ChannelName}, guild {GuildID})")) { }
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
            try { this._client.MessageReceived -= OnClientMessageReceived; } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
