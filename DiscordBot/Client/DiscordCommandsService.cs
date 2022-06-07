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
        private CancellationTokenSource _cts;

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
            this._client.SlashCommandExecuted += OnSlashCommand;
            this._client.UserCommandExecuted += OnUserCommand;
            this._client.MessageCommandExecuted += OnMessageCommand;
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

        private async Task OnSlashCommand(SocketSlashCommand interaction)
        {
            DevSubInteractionContext ctx = new DevSubInteractionContext(this._client, interaction, this._cts.Token);
            using IDisposable logScope = this._log.BeginCommandScope(ctx, null, null);
            await this._interactions.ExecuteCommandAsync(ctx, this._services).ConfigureAwait(false);
        }

        private async Task OnUserCommand(SocketUserCommand interaction)
        {
            DevSubInteractionContext ctx = new DevSubInteractionContext(this._client, interaction, this._cts.Token);
            using IDisposable logScope = this._log.BeginCommandScope(ctx, null, null);
            await this._interactions.ExecuteCommandAsync(ctx, this._services);
        }

        private async Task OnMessageCommand(SocketMessageCommand interaction)
        {
            DevSubInteractionContext ctx = new DevSubInteractionContext(this._client, interaction, this._cts.Token);
            using IDisposable logScope = this._log.BeginCommandScope(ctx, null, null);
            await this._interactions.ExecuteCommandAsync(ctx, this._services);
        }

        private Task OnLog(Discord.LogMessage logMessage)
        {
            this._log.Log(logMessage);
            return Task.CompletedTask;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            try { this._cts?.Cancel(); } catch { }
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { this._client.Ready -= OnClientReady; } catch { }
            try { this._client.SlashCommandExecuted -= OnSlashCommand; } catch { }
            try { this._client.UserCommandExecuted -= OnUserCommand; } catch { }
            try { this._client.MessageCommandExecuted -= OnMessageCommand; } catch { }
            try { this._interactions.Log -= OnLog; } catch { }
            try { this._interactions?.Dispose(); } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
