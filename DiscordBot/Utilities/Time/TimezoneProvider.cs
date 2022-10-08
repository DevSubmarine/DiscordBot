namespace DevSubmarine.DiscordBot.Time.Services
{
    internal class TimezoneProvider : ITimezoneProvider, IDisposable
    {
        private readonly IOptionsMonitor<TimezoneOptions> _timezoneOptions;
        private readonly ILogger _log;
        private readonly IDisposable _timezoneOptionsChangeRegistration;

        private IEnumerable<BotTimezone> _timezones;

        public TimezoneProvider(IOptionsMonitor<TimezoneOptions> timezoneOptions, ILogger<TimezoneProvider> log)
        {
            this._timezoneOptions = timezoneOptions;
            this._log = log;

            this._timezoneOptionsChangeRegistration = this._timezoneOptions.OnChange(_ => this._timezones = null);
        }

        public IEnumerable<BotTimezone> GetAllTimezones()
        {
            if (this._timezones == null)
            {
                this._log.LogDebug("Deserializing timezones");
                TimezoneOptions options = this._timezoneOptions.CurrentValue;
                if (options.SerializedTimezones?.Any() == true)
                    this._timezones = options.SerializedTimezones.Select(tz => BotTimezone.Deserialize(tz));
                else if (options.FallbackToSystemTimezones)
                    this._timezones = TimeZoneInfo.GetSystemTimeZones().Select(tz => new BotTimezone(tz));
                else
                    throw new InvalidOperationException("No timezones configured");
            }

            return this._timezones;
        }

        public bool ContainsTimezone(string id)
            => this.GetTimezone(id) != null;

        public void Dispose()
        {
            try { this._timezoneOptionsChangeRegistration?.Dispose(); } catch { }
        }
    }
}
