namespace DevSubmarine.DiscordBot.Voting
{
    internal class CooldownVotingResult : IVotingResult
    {
        public TimeSpan CooldownRemaining { get; }

        public CooldownVotingResult(TimeSpan cooldownRemaining)
        {
            this.CooldownRemaining = cooldownRemaining;
        }
    }
}
