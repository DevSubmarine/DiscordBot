using Discord.Interactions;
using Discord.Net;
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
        private readonly SemaphoreSlim _initializationLock;
        private CancellationTokenSource _cts;
        private bool _commandsInitialized;

        public DiscordCommandsService(DiscordSocketClient client, IServiceProvider services, ILogger<DiscordCommandsService> log, IOptions<DiscordOptions> options)
        {
            this._client = client;
            this._services = services;
            this._log = log;
            this._options = options.Value;
            this._initializationLock = new SemaphoreSlim(1, 1);
            this._interactions = new InteractionService(this._client, new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Sync,
                UseCompiledLambda = this._options.CompileCommands
            });

            this._client.Ready += this.OnClientReady;
            this._client.SlashCommandExecuted += this.OnSlashCommandAsync;
            this._client.UserCommandExecuted += this.OnUserCommandAsync;
            this._client.MessageCommandExecuted += this.OnMessageCommandAsync;
            this._client.ButtonExecuted += this.OnButtonCommandAsync;
            this._client.SelectMenuExecuted += this.OnMenuCommandAsync;
            this._client.AutocompleteExecuted += this.OnAutocompleteAsync;
            this._interactions.Log += this.OnLog;
        }

        private async Task OnClientReady()
        {
            await this._initializationLock.WaitAsync(this._cts?.Token ?? default).ConfigureAwait(false);
            try
            {
                if (this._commandsInitialized)
                    return;

                if (this._options.PurgeGlobalCommands)
                {
                    this._log.LogDebug("Purging global commands");
                    await this._interactions.RegisterCommandsGloballyAsync().ConfigureAwait(false);
                }

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

                this._commandsInitialized = true;
            }
            finally
            {
                this._initializationLock.Release();
            }
        }


        // currently interactions don't really differ much
        // it might change later, but for now we can delegate all event handlers to the same method
        private Task OnSlashCommandAsync(SocketSlashCommand interaction)
            => this.OnInteractionAsync(interaction);
        private  Task OnUserCommandAsync(SocketUserCommand interaction)
            => this.OnInteractionAsync(interaction);
        private Task OnMessageCommandAsync(SocketMessageCommand interaction)
            => this.OnInteractionAsync(interaction);
        private Task OnMenuCommandAsync(SocketMessageComponent interaction)
            => this.OnInteractionAsync(interaction);
        private Task OnButtonCommandAsync(SocketMessageComponent interaction)
            => this.OnInteractionAsync(interaction);
        private Task OnAutocompleteAsync(SocketAutocompleteInteraction interaction)
            => this.OnInteractionAsync(interaction);

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            DevSubInteractionContext ctx = new DevSubInteractionContext(this._client, interaction, this._cts.Token);
            using IDisposable logScope = this._log.BeginCommandScope(ctx, null, null);
            try
            {
                await this._interactions.ExecuteCommandAsync(ctx, this._services);
            }
            catch (HttpException ex) when (ex.IsMissingPermissions())
            {
                try
                {
                    await interaction.RespondAsync(
                        text: $"{ResponseEmoji.Failure} Bot missing permissions. Please contact guild admin.",
                        ephemeral: true,
                        options: ctx.CancellationToken.ToRequestOptions())
                        .ConfigureAwait(false);
                }
                catch { }
                throw;
            }
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
            try { this._client.Ready -= this.OnClientReady; } catch { }
            try { this._client.SlashCommandExecuted -= this.OnSlashCommandAsync; } catch { }
            try { this._client.UserCommandExecuted -= this.OnUserCommandAsync; } catch { }
            try { this._client.MessageCommandExecuted -= this.OnMessageCommandAsync; } catch { }
            try { this._client.ButtonExecuted -= this.OnMenuCommandAsync; } catch { }
            try { this._client.SelectMenuExecuted -= this.OnButtonCommandAsync; } catch { }
            try { this._interactions.Log -= this.OnLog; } catch { }
            try { this._interactions?.Dispose(); } catch { }
            try { this._initializationLock?.Dispose(); } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
