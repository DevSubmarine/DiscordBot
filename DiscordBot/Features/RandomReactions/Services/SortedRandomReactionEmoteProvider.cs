namespace DevSubmarine.DiscordBot.RandomReactions.Services
{
    public class SortedRandomReactionEmoteProvider : IRandomReactionEmoteProvider, IDisposable
    {
        private readonly IOptionsMonitor<RandomReactionsOptions> _options;
        private readonly IDisposable _optionsChangeHandle;

        private readonly object _lock = new object();

        // we sort emotes by chance to give those with low chance a fair... chance?
        // cache to do it only once per options reload
        private IOrderedEnumerable<RandomReactionEmote> _sortedWelcomeEmotes;
        private IOrderedEnumerable<RandomReactionEmote> _sortedFollowupEmotes;
        private IOrderedEnumerable<RandomReactionEmote> _sortedRandomEmotes;
        
        public SortedRandomReactionEmoteProvider(IOptionsMonitor<RandomReactionsOptions> options)
        {
            this._options = options;
            this._optionsChangeHandle = this._options.OnChange(_ =>
            {
                this._sortedWelcomeEmotes = null;
                this._sortedFollowupEmotes = null;
                this._sortedRandomEmotes = null;
            });
        }
        
        public IEnumerable<RandomReactionEmote> GetWelcomeEmotes()
        {
            lock (this._lock)
                this._sortedWelcomeEmotes ??= LoadEmotes(this._options.CurrentValue.WelcomeEmotes);
            return this._sortedWelcomeEmotes;
        }
        
        public IEnumerable<RandomReactionEmote> GetFollowupEmotes()
        {
            lock (this._lock)
                this._sortedFollowupEmotes ??= LoadEmotes(this._options.CurrentValue.FollowupEmotes);
            return this._sortedFollowupEmotes;
        }

        public IEnumerable<RandomReactionEmote> GetRandomEmotes()
        {
            lock (this._lock)
                this._sortedRandomEmotes ??= LoadEmotes(this._options.CurrentValue.RandomEmotes);
            return this._sortedRandomEmotes;
        }

        private static IOrderedEnumerable<RandomReactionEmote> LoadEmotes(IEnumerable<RandomReactionsOptions.EmoteOptions> config)
            => config.Select(e => new RandomReactionEmote(e.Emote, e.Chance))
                .OrderBy(e => e.Chance);

        public void Dispose()
        {
            try { this._optionsChangeHandle?.Dispose(); } catch { }
        }
    }
}