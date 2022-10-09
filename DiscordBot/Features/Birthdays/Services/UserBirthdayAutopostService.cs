using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.DiscordBot.Birthdays.Services
{
    public class UserBirthdayAutopostService : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IUserBirthdaysProvider _provider;
        private readonly IUserBirthdayEmbedBuilder _embedBuilder;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<UserBirthdaysOptions> _options;
        private CancellationTokenSource _cts;

        private UserBirthdaysOptions Options => this._options.CurrentValue;

        public UserBirthdayAutopostService(IUserBirthdaysProvider provider, IUserBirthdayEmbedBuilder embedBuilder, DiscordSocketClient client,
            ILogger<UserBirthdayAutopostService> log, IOptionsMonitor<UserBirthdaysOptions> options)
        {
            this._provider = provider;
            this._embedBuilder = embedBuilder;
            this._client = client;
            this._log = log;
            this._options = options;
        }

        private async Task ScannerLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (this._client.ConnectionState != ConnectionState.Connected)
                {
                    this._log.LogTrace("Client not connected, waiting");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
                }

                if (this.Options.AutoPostChannelID != null)
                {
                    IEnumerable<UserBirthday> allBirthdays = await this._provider.GetAllAsync(cancellationToken).ConfigureAwait(false);
                    Embed embed = await this._embedBuilder.BuildUpcomingBirthdaysEmbedAsync(allBirthdays, this.Options.AutoPostDaysAhead, false, cancellationToken).ConfigureAwait(false);
                    if (embed != null)
                    {
                        this._log.LogDebug("Auto-posting upcoming birthdays");
                        IChannel channel = await this._client.GetChannelAsync(this.Options.AutoPostChannelID.Value, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                        if (channel is not ITextChannel textChannel)
                        {
                            this._log.LogError("Channel ID {ChannelID} is not a valid text channel", channel.Id);
                            continue;
                        }

                        await textChannel.SendMessageAsync(embed: embed, options: cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                    }
                }
                else
                    this._log.LogDebug("No birthday auto-post channel set, skipping");

                this._log.LogDebug("Next birthdays autpost: {Time}", DateTime.Now.AddDays(1));
                await Task.Delay(TimeSpan.FromDays(1), cancellationToken).ConfigureAwait(false);
            }
        }

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
