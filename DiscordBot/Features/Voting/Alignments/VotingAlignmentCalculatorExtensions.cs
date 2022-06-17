namespace DevSubmarine.DiscordBot.Voting
{
    public static class VotingAlignmentCalculatorExtensions
    {
        public static VotingAlignment CalculateAlignment(this IVotingAlignmentCalculator calculator, IEnumerable<Vote> goodVotes, IEnumerable<Vote> badVotes)
            => calculator.CalculateAlignment(goodVotes.Count(), badVotes.Count());

        public static VotingAlignment CalculateAlignment(this IVotingAlignmentCalculator calculator, IEnumerable<Vote> allVotes)
        {
            IEnumerable<Vote> goodVotes = allVotes.Where(vote => vote.Type == VoteType.Mod);
            IEnumerable<Vote> badVotes = allVotes.Where(vote => vote.Type == VoteType.Kick || vote.Type == VoteType.Ban);
            return CalculateAlignment(calculator, goodVotes, badVotes);
        }
    }
}
