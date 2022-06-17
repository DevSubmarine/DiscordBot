namespace DevSubmarine.DiscordBot.Voting
{
    /// <summary>Voting result indicating failure due to cooldown.</summary>
    internal class CooldownVotingResult : IVotingResult
    {
        /// <summary>The remaining cooldown.</summary>
        public TimeSpan CooldownRemaining { get; }

        public CooldownVotingResult(TimeSpan cooldownRemaining)
        {
            this.CooldownRemaining = cooldownRemaining;
        }
    }
}
