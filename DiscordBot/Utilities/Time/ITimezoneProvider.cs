namespace DevSubmarine.DiscordBot.Time
{
    public interface ITimezoneProvider
    {
        IEnumerable<BotTimezone> GetAllTimezones();
    }
}
