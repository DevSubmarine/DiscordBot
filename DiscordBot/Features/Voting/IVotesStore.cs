namespace DevSubmarine.DiscordBot.Voting
{
    /// <summary>Stores votes.</summary>
    public interface IVotesStore
    {
        /// <summary>Stores a new vote.</summary>
        /// <param name="vote">Vote to store.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task AddVoteAsync(Vote vote, CancellationToken cancellationToken = default);
        /// <summary>Retrieves all stored votes that match the search criteria.</summary>
        /// <param name="targetID">ID of the target user. Ignored if null.</param>
        /// <param name="voterID">ID of the source user. Ignored if null.</param>
        /// <param name="type">Type of the vote. Ignored if null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of found votes.</returns>
        Task<IEnumerable<Vote>> GetVotesAsync(ulong? targetID, ulong? voterID, VoteType? type, CancellationToken cancellationToken = default);
    }
}
