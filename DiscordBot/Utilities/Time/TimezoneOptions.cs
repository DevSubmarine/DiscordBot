namespace DevSubmarine.DiscordBot.Time
{
    public class TimezoneOptions
    {
        public IEnumerable<string> SerializedTimezones { get; set; }
        public bool FallbackToSystemTimezones { get; set; } = false;
    }
}
