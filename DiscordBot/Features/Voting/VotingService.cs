namespace DevSubmarine.DiscordBot.Voting.Services
{
    internal class VotingService : IVotingService
    {
        private readonly IVotesStore _store;
        private readonly IVotingCooldownManager _cooldown;

        public VotingService(IVotesStore store, IVotingCooldownManager cooldown)
        {
            this._store = store;
            this._cooldown = cooldown;
        }

        public async Task<IVotingResult> VoteAsync(Vote vote, CancellationToken cancellationToken = default)
        {
            if (vote == null)
                throw new ArgumentNullException(nameof(vote));

            if (!_cooldown.IsReady(vote.VoterID, vote.TargetID, out TimeSpan cooldownRemaining))
                return new CooldownVotingResult(cooldownRemaining);

            await this._store.AddVoteAsync(vote, cancellationToken).ConfigureAwait(false);
            IEnumerable<Vote> votesAgainstTarget = await this._store.GetVotesAsync(vote.TargetID, null, vote.Type, cancellationToken).ConfigureAwait(false);
            return new SuccessVotingResult(vote)
            {
                VotesAgainstTarget = (ulong)votesAgainstTarget.Where(v => v.VoterID == vote.VoterID).LongCount(),
                TotalVotesAgainstTarget = (ulong)votesAgainstTarget.LongCount()
            };
        }
    }
}
