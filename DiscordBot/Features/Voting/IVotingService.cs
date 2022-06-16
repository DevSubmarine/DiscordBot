namespace DevSubmarine.DiscordBot.Voting
{
    public interface IVotingService
    {
        Task<IVotingResult> VoteAsync(Vote vote, CancellationToken cancellationToken = default);
    }
}
