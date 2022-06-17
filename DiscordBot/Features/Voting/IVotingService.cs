namespace DevSubmarine.DiscordBot.Voting
{
    /// <summary>Service responsible for making a vote.</summary>
    public interface IVotingService
    {
        /// <summary>Creates and saves the vote.</summary>
        /// <param name="vote">Vote to create.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Voting result. Please check for result type to determine whether the vote was successful, and what action to take.</returns>
        /// <seealso cref="SuccessVotingResult"/>
        /// <seealso cref="CooldownVotingResult"/>
        /// <exception cref="ArgumentNullException">The provided vote was null.</exception>
        Task<IVotingResult> VoteAsync(Vote vote, CancellationToken cancellationToken = default);
    }
}
