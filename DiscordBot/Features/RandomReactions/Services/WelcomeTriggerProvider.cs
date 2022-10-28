namespace DevSubmarine.DiscordBot.RandomReactions.Services
{
    public class WelcomeTriggerProvider : IWelcomeTriggerProvider, IDisposable
    {
        private readonly IOptionsMonitor<RandomReactionsOptions> _options;
        private readonly IDisposable _optionsChangeHandle;

        private IEnumerable<WelcomeTrigger> _triggers;
        
        public WelcomeTriggerProvider(IOptionsMonitor<RandomReactionsOptions> options)
        {
            this._options = options;
            this._optionsChangeHandle = this._options.OnChange(_ =>
            {
                this._triggers = null;
            });
        }

        public IEnumerable<WelcomeTrigger> GetWelcomeTriggers()
        {
            this._triggers ??= LoadTriggers(this._options.CurrentValue.WelcomeTriggers ?? Enumerable.Empty<string>());
            return this._triggers;
        }

        private static IEnumerable<WelcomeTrigger> LoadTriggers(IEnumerable<string> config)
            => config.Select(e => new WelcomeTrigger(e));

        public void Dispose()
        {
            try { this._optionsChangeHandle?.Dispose(); } catch { }
        }
    }
}