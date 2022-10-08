namespace DevSubmarine.DiscordBot.Time
{
    public static class TimezoneProviderExtensions
    {
        public static bool ContainsTimezone(this ITimezoneProvider provider, string id)
            => provider.GetTimezone(id) != null;

        public static BotTimezone GetTimezone(this ITimezoneProvider provider, string id)
            => provider.GetAllTimezones().FirstOrDefault(tz => id == tz.ID);

        public static BotTimezone GetTimezoneByName(this ITimezoneProvider provider, string name)
            => provider.GetAllTimezones().FirstOrDefault(tz => name == tz.Name);
    }
}
