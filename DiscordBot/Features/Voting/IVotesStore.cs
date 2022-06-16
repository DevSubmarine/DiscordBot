namespace DevSubmarine.DiscordBot.Voting
{
    public interface IVotesStore
    {
        Task AddVoteAsync(Vote vote, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vote>> GetVotesAsync(ulong? targetID, ulong? voterID, VoteType? type, CancellationToken cancellationToken = default);
    }
}
