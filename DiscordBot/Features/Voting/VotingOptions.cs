namespace DevSubmarine.DiscordBot.Voting
{
    public class VotingOptions
    {
        public TimeSpan VotingCooldown { get; set; } = TimeSpan.FromMinutes(3);
    }
}
