namespace DevSubmarine.DiscordBot.Voting
{
    public class VotingOptions
    {
        /// <summary>Minimum time between votes.</summary>
        public TimeSpan VotingCooldown { get; set; } = TimeSpan.FromMinutes(3);
        /// <summary>Map of image URL for each voting alignment.</summary>
        public IDictionary<VotingAlignmentLevel, string> AlignmentImages { get; set; }
    }
}
