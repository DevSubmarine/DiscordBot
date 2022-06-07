using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.DiscordBot.Client
{
    internal class DiscordCommandsService : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordOptions _options;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public DiscordCommandsService(DiscordSocketClient client, IServiceProvider services, ILogger<DiscordCommandsService> log, IOptions<DiscordOptions> options)
        {
            this._client = client;
            this._services = services;
            this._log = log;
            this._options = options.Value;
            this._interactions = new InteractionService(this._client, new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Async,
                UseCompiledLambda = this._options.CompileCommands
            });

            this._client.Ready += OnClientReady;
            this._interactions.Log += OnLog;
        }

        private async Task OnClientReady()
        {
            this._log.LogTrace("Loading all command modules");
            await this._interactions.AddModulesAsync(this.GetType().Assembly, this._services);

            if (this._options.CommandsGuildID != null)
            {
                this._log.LogDebug("Registering all commands for guild {GuildID}", this._options.CommandsGuildID.Value);
                await this._interactions.RegisterCommandsToGuildAsync(this._options.CommandsGuildID.Value).ConfigureAwait(false);
            }
            else
            {
                this._log.LogDebug("Registering all commands globally");
                await this._interactions.RegisterCommandsGloballyAsync().ConfigureAwait(false);
            }
        }

        private Task OnLog(Discord.LogMessage logMessage)
        {
            this._log.Log(logMessage);
            return Task.CompletedTask;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { this._client.Ready -= OnClientReady; } catch { }
            try { this._interactions.Log += OnLog; } catch { }
            try { this._interactions?.Dispose(); } catch { }
        }
    }
}
