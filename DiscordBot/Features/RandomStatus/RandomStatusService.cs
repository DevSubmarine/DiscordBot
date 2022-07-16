using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.RandomStatus.Services
{
    /// <summary>Background service that periodically scans blog channels for last activity and activates or deactivates them.</summary>
    internal class RandomStatusService : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IRandomizer _randomizer;
        private readonly IStatusPlaceholderEngine _placeholders;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<RandomStatusOptions> _options;
        private CancellationTokenSource _cts;

        private DateTime _lastChangeUtc;

        public RandomStatusService(DiscordSocketClient client, IRandomizer randomizer, IStatusPlaceholderEngine placeholders,
            ILogger<RandomStatusService> log, IOptionsMonitor<RandomStatusOptions> options)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._placeholders = placeholders;
            this._log = log;
            this._options = options;
        }

        private async Task AutoChangeLoopAsync(CancellationToken cancellationToken)
        {
            this._log.LogDebug("Starting status randomization loop. Change rate is {ChangeRate}", this._options.CurrentValue.ChangeRate);
            if (this._options.CurrentValue.ChangeRate <= TimeSpan.FromSeconds(10))
                this._log.LogWarning("Change rate is less than 10 seconds!");

            while (!cancellationToken.IsCancellationRequested && this._options.CurrentValue.IsEnabled)
            {
                RandomStatusOptions options = this._options.CurrentValue;

                while (this._client.ConnectionState != ConnectionState.Connected)
                {
                    this._log.LogTrace("Client not connected, waiting");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
                }

                DateTime nextChangeUtc = this._lastChangeUtc + this._options.CurrentValue.ChangeRate;
                TimeSpan remainingWait = nextChangeUtc - DateTime.UtcNow;
                if (remainingWait > TimeSpan.Zero)
                    await Task.Delay(remainingWait, cancellationToken).ConfigureAwait(false);
                await this.RandomizeStatusAsync(cancellationToken).ConfigureAwait(false);
                this._log.LogTrace("Next status change: {ChangeTime}", this._lastChangeUtc + options.ChangeRate);
                await Task.Delay(options.ChangeRate, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<Status> RandomizeStatusAsync(CancellationToken cancellationToken)
        {
            RandomStatusOptions options = this._options.CurrentValue;

            if (!options.IsEnabled)
                return null;

            Status status = this._randomizer.GetRandomValue(options.Statuses);

            try
            {
                if (this._client.CurrentUser == null || this._client.ConnectionState != ConnectionState.Connected)
                    return null;
                if (status == null)
                    return null;
                if (!string.IsNullOrWhiteSpace(status.Text))
                {
                    string text = await this._placeholders.ConvertPlaceholdersAsync(status.Text, cancellationToken).ConfigureAwait(false);
                    this._log.LogDebug("Changing status to `{Status}`", text);
                }
                else
                    this._log.LogDebug("Clearing status");
                await this._client.SetGameAsync(status.Text, status.Link, status.ActivityType).ConfigureAwait(false);
                return status;
            }
            catch (Exception ex) when (options.IsEnabled && ex.LogAsError(this._log, "Failed changing status to {Status}", status))
            {
                return null;
            }
            finally
            {
                this._lastChangeUtc = DateTime.UtcNow;
            }
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = this.AutoChangeLoopAsync(this._cts.Token);
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
