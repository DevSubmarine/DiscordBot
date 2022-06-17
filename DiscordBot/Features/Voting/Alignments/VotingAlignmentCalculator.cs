namespace DevSubmarine.DiscordBot.Voting.Services
{
    /// <inheritdoc/>
    internal class VotingAlignmentCalculator : IVotingAlignmentCalculator
    {
        private readonly VotingOptions _options;

        public VotingAlignmentCalculator(IOptionsMonitor<VotingOptions> options)
        {
            this._options = options.CurrentValue;
        }

        /// <inheritdoc/>
        public VotingAlignment GetAlignment(double score)
        {
            VotingAlignmentLevel level = this.GetLevelForScore(score);
            string imageURL = this._options.AlignmentImages[level];
            return new VotingAlignment(score, level, imageURL);
        }

        /// <inheritdoc/>
        public VotingAlignment CalculateAlignment(double goodPoints, double badPoints)
        {
            double totalPoints = goodPoints + badPoints;
            double score = (goodPoints / totalPoints) * 100d;
            return this.GetAlignment(score);
        }

        private VotingAlignmentLevel GetLevelForScore(double score)
        {
            if (score <= 11.11)
                return VotingAlignmentLevel.ChaoticEvil;
            if (score <= 22.22)
                return VotingAlignmentLevel.NeutralEvil;
            if (score <= 33.33)
                return VotingAlignmentLevel.LawfulEvil;
            if (score <= 44.44)
                return VotingAlignmentLevel.ChaoticNeutral;
            if (score <= 55.55)
                return VotingAlignmentLevel.TrueNeutral;
            if (score <= 66.66)
                return VotingAlignmentLevel.LawfulNeutral;
            if (score <= 77.77)
                return VotingAlignmentLevel.ChaoticGood;
            if (score <= 88.88)
                return VotingAlignmentLevel.NeutralGood;

            return VotingAlignmentLevel.LawfulGood;
        }
    }
}
