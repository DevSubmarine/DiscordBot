namespace DevSubmarine.DiscordBot.Birthdays
{
    public class UserBirthdaysOptions
    {
        public ulong? AutoPostChannelID { get; set; }
        public int AutoPostDaysAhead { get; set; } = 1;
    }
}
