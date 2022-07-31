namespace DevSubmarine.DiscordBot.Voting
{
    public static class VotingAlignmentCalculatorExtensions
    {
        /// <summary>Calculates alignment score and gets alignment information.</summary>
        /// <param name="calculator">The service instance.</param>
        /// <param name="goodVotes">The votes that are to be used as "good" points when calculating the score.</param>
        /// <param name="badVotes">he votes that are to be used as "bad" points when calculating the score.</param>
        /// <returns>Alignment information.</returns>
        public static VotingAlignment CalculateAlignment(this IVotingAlignmentCalculator calculator, IEnumerable<Vote> goodVotes, IEnumerable<Vote> badVotes)
            => calculator.CalculateAlignment(goodVotes.Count(), badVotes.Count());

        /// <summary>Calculates alignment score and gets alignment information.</summary>
        /// <param name="calculator">The service instance.</param>
        /// <param name="allVotes">All votes (both good and bad) to take into account when calculating the score.</param>
        /// <returns>Alignment information.</returns>
        public static VotingAlignment CalculateAlignment(this IVotingAlignmentCalculator calculator, IEnumerable<Vote> allVotes)
        {
            IEnumerable<Vote> goodVotes = allVotes.Where(vote => vote.IsPositive);
            IEnumerable<Vote> badVotes = allVotes.Where(vote => vote.IsNegative);
            return CalculateAlignment(calculator, goodVotes, badVotes);
        }
    }
}
