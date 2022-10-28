using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.RandomReactions.Services
{
    internal class RandomReactionsListener : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IRandomizer _randomizer;
        private readonly IRandomReactionEmoteProvider _emotes;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<RandomReactionsOptions> _options;
        private readonly IOptionsMonitor<DevSubOptions> _devsubOptions;
        private CancellationTokenSource _cts;

        public RandomReactionsListener(DiscordSocketClient client, IRandomizer randomizer, IRandomReactionEmoteProvider emotes,
            ILogger<RandomReactionsListener> log, IOptionsMonitor<RandomReactionsOptions> options, IOptionsMonitor<DevSubOptions> devsubOptions)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._emotes = emotes;
            this._log = log;
            this._options = options;
            this._devsubOptions = devsubOptions;

            this._client.MessageReceived += this.OnClientMessageReceivedAsync;
            this._client.ReactionAdded += this.OnClientReactionAddedAsync;
        }

        private async Task OnClientMessageReceivedAsync(SocketMessage message)
        {
            RandomReactionsOptions options = this._options.CurrentValue;
            if (!options.Enabled)
                return;

            if (message.Author.Id == this._client.CurrentUser.Id)
                return;
            if (message.Channel is not SocketTextChannel channel)
                return;
            if (channel.Guild.Id != this._devsubOptions.CurrentValue.GuildID)
                return;

            if (await this.TryHandleWelcomeAsync(message, options))
                return;
            if (await this.TryHandleFollowupAsync(message))
                return;
            if (await this.TryHandleRandomAsync(message))
                return;
        }

        private async Task OnClientReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> messageChannel, SocketReaction reaction)
        {
            if (!this._options.CurrentValue.Enabled)
                return;

            if (messageChannel.Value is not SocketTextChannel channel)
                return;
            if (channel.Guild.Id != this._devsubOptions.CurrentValue.GuildID)
                return;
            IMessage message = await channel.GetMessageAsync(cachedMessage.Id, this._cts.Token.ToRequestOptions()).ConfigureAwait(false);
            if (message.Author.Id == this._client.CurrentUser.Id)
                return;

            this._log.LogTrace("Attempting to handle followup reaction for message {MessageID}", message.Id);

            RandomReactionEmote emote = this._emotes.GetFollowupEmotes().FirstOrDefault(e => e.Emote.Equals(reaction.Emote));
            if (emote == null)
                return;

            if (!this._randomizer.RollChance(emote.Chance))
                return;

            await this.AddReactionAsync(message, emote.Emote).ConfigureAwait(false);
        }

        private async ValueTask<bool> TryHandleWelcomeAsync(SocketMessage message, RandomReactionsOptions options)
        {
            this._log.LogTrace("Attempting to handle welcome reaction for message {MessageID}", message.Id);

            string content = message.Content.TrimStart();
            if (options.WelcomeTriggers?.Any(trigger => content.StartsWith(trigger, StringComparison.OrdinalIgnoreCase)) != true)
                return false;

            foreach (RandomReactionEmote emote in this._emotes.GetWelcomeEmotes())
            {
                if (!this._randomizer.RollChance(emote.Chance))
                    continue;

                await this.AddReactionAsync(message, emote.Emote).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async ValueTask<bool> TryHandleFollowupAsync(SocketMessage message)
        {
            this._log.LogTrace("Attempting to handle followup reaction for message {MessageID}", message.Id);

            foreach (RandomReactionEmote emote in this._emotes.GetFollowupEmotes())
            {
                if (!this._randomizer.RollChance(emote.Chance))
                    continue;
                if (!message.Content.Contains(emote.ToString()))
                    continue;

                await this.AddReactionAsync(message, emote.Emote).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async ValueTask<bool> TryHandleRandomAsync(SocketMessage message)
        {
            this._log.LogTrace("Attempting to handle random reaction for message {MessageID}", message.Id);

            foreach (RandomReactionEmote emote in this._emotes.GetRandomEmotes())
            {
                if (!this._randomizer.RollChance(emote.Chance))
                    continue;

                await this.AddReactionAsync(message, emote.Emote).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private Task AddReactionAsync(IMessage message, IEmote emote)
        {
            if (message.Reactions.Any(reaction => reaction.Value.IsMe && reaction.Key.Equals(emote)))
            {
                this._log.LogDebug("Already added reaction {Emote} to message {MessageID}", emote.ToString(), message.Id);
                return Task.CompletedTask;
            }

            this._log.LogDebug("Adding reaction {Emote} to message {MessageID}", emote.ToString(), message.Id);
            return message.AddReactionAsync(emote, this._cts.Token.ToRequestOptions());
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
            try { this._client.MessageReceived -= this.OnClientMessageReceivedAsync; } catch { }
            try { this._client.ReactionAdded -= this.OnClientReactionAddedAsync; } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
