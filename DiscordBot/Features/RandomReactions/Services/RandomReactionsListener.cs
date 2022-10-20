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
        private readonly ILogger _log;
        private readonly IOptionsMonitor<RandomReactionsOptions> _options;
        private readonly IOptionsMonitor<DevSubOptions> _devsubOptions;
        private readonly IDisposable _optionsChangeHandle;
        private CancellationTokenSource _cts;

        // we sort emotes by chance to give those with low chance a fair... chance?
        // cache to do it only once per options reload
        private IEnumerable<RandomReactionEmote> _sortedWelcomeEmotes;
        private IEnumerable<RandomReactionEmote> _sortedFollowupEmotes;
        private IEnumerable<RandomReactionEmote> _sortedRandomEmotes;

        public RandomReactionsListener(DiscordSocketClient client, IRandomizer randomizer,
            ILogger<RandomReactionsListener> log, IOptionsMonitor<RandomReactionsOptions> options, IOptionsMonitor<DevSubOptions> devsubOptions)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._log = log;
            this._options = options;
            this._devsubOptions = devsubOptions;

            this._client.MessageReceived += this.OnClientMessageReceivedAsync;
            this._client.ReactionAdded += this.OnClientReactionAddedAsync;
            this._optionsChangeHandle = this._options.OnChange(_ =>
            {
                this._sortedWelcomeEmotes = null;
                this._sortedFollowupEmotes = null;
                this._sortedRandomEmotes = null;
            });
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

            this.SortAndCacheEmotes(options);
            if (this._sortedWelcomeEmotes?.Any() != true && this._sortedFollowupEmotes?.Any() != true && this._sortedRandomEmotes?.Any() != true)
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
            RandomReactionsOptions options = this._options.CurrentValue;
            if (!options.Enabled)
                return;

            if (messageChannel.Value is not SocketTextChannel channel)
                return;
            if (channel.Guild.Id != this._devsubOptions.CurrentValue.GuildID)
                return;
            IMessage message = await channel.GetMessageAsync(cachedMessage.Id, this._cts.Token.ToRequestOptions()).ConfigureAwait(false);
            if (message.Author.Id == this._client.CurrentUser.Id)
                return;

            this._log.LogTrace("Attempting to handle followup reaction for message {MessageID}", message.Id);

            this.SortAndCacheEmotes(options);
            RandomReactionEmote emote = this._sortedFollowupEmotes.FirstOrDefault(e => e.Emote.Equals(reaction.Emote));
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

            foreach (RandomReactionEmote emote in this._sortedWelcomeEmotes)
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

            foreach (RandomReactionEmote emote in this._sortedFollowupEmotes)
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

            foreach (RandomReactionEmote emote in this._sortedRandomEmotes)
            {
                if (!this._randomizer.RollChance(emote.Chance))
                    continue;

                await this.AddReactionAsync(message, emote.Emote).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private void SortAndCacheEmotes(RandomReactionsOptions options)
        {
            this._sortedWelcomeEmotes ??= SortEmotes(options.WelcomeEmotes);
            this._sortedFollowupEmotes ??= SortEmotes(options.FollowupEmotes);
            this._sortedRandomEmotes ??= SortEmotes(options.RandomEmotes);

            IOrderedEnumerable<RandomReactionEmote> SortEmotes(IEnumerable<RandomReactionsOptions.EmoteOptions> emotes)
                => emotes.Select(e => new RandomReactionEmote(e.Emote, e.Chance))
                    .OrderBy(e => e.Chance);
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
            try { this._optionsChangeHandle?.Dispose(); } catch { }
            try { this._client.MessageReceived -= this.OnClientMessageReceivedAsync; } catch { }
            try { this._client.ReactionAdded -= this.OnClientReactionAddedAsync; } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
