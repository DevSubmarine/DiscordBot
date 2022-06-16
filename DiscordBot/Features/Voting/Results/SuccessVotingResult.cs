namespace DevSubmarine.DiscordBot.Voting
{
    internal class SuccessVotingResult : IVotingResult
    {
        public Vote CreatedVote { get; }

        /// <summary>Count of votes of same type as <see cref="CreatedVote"/> created by the same voter.</summary>
        public ulong VotesAgainstTarget { get; init; }
        /// <summary>Count of votes of same type as <see cref="CreatedVote"/> created by anyone.</summary>
        public ulong TotalVotesAgainstTarget { get; init; }

        public SuccessVotingResult(Vote vote)
        {
            this.CreatedVote = vote;
        }
    }
}
