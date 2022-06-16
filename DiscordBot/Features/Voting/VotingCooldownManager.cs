namespace DevSubmarine.DiscordBot.Voting.Services
{
    internal class VotingCooldownManager : IVotingCooldownManager
    {
        private readonly ILogger _log;
        private readonly IOptionsMonitor<VotingOptions> _options;
        private readonly IDictionary<CooldownKey, DateTime> _lastVotes;
        private readonly object _lock = new object();

        private VotingOptions Options => this._options.CurrentValue;

        public VotingCooldownManager(IOptionsMonitor<VotingOptions> options, ILogger<VotingCooldownManager> log)
        {
            this._options = options;
            this._log = log;
            this._lastVotes = new Dictionary<CooldownKey, DateTime>();
        }

        public void AddCooldown(ulong voterID, ulong targetID)
        {
            lock (this._lock)
            {
                this._log.LogTrace("Adding vote timestamp for votes by {VoterID} against {TargetID}", voterID, targetID);
                CooldownKey key = new CooldownKey(voterID, targetID);
                DateTime timestamp = DateTime.UtcNow;
                this._lastVotes[key] = timestamp;
                this._log.LogDebug("Vote by {VoterID} against {TargetID} recorded at timestamp {Timestamp}", voterID, targetID, timestamp);
            }
        }

        public bool IsReady(ulong voterID, ulong targetID, out TimeSpan cooldownRemaining)
        {
            lock (this._lock)
            {
                this._log.LogTrace("Checking vote timestamp for votes by {VoterID} against {TargetID}", voterID, targetID);
                CooldownKey key = new CooldownKey(voterID, targetID);
                if (!this._lastVotes.TryGetValue(key, out DateTime timestamp))
                {
                    this._log.LogDebug("No vote timestamp found for vote by {VoterID} against {TargetID}", voterID, targetID);
                    cooldownRemaining = TimeSpan.Zero;
                    return true;
                }

                this._log.LogDebug("Vote timestamp for voteby {VoterID} against {TargetID} found with value of {Timestamp}", voterID, targetID, timestamp);
                TimeSpan timeElapsed = DateTime.UtcNow - timestamp;
                cooldownRemaining = this.Options.VotingCooldown - timeElapsed;
                if (cooldownRemaining < TimeSpan.Zero)
                    cooldownRemaining = TimeSpan.Zero;

                return cooldownRemaining <= TimeSpan.Zero;
            }
        }

        private struct CooldownKey : IEquatable<CooldownKey>
        {
            public ulong VoterID { get; }
            public ulong TargetID { get; }

            public CooldownKey(ulong voterID, ulong targetID)
            {
                this.VoterID = voterID;
                this.TargetID = targetID;
            }

            public override bool Equals(object obj)
            {
                return obj is CooldownKey key && Equals(key);
            }

            public bool Equals(CooldownKey other)
            {
                return VoterID == other.VoterID &&
                       TargetID == other.TargetID;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(VoterID, TargetID);
            }

            public static bool operator ==(CooldownKey left, CooldownKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(CooldownKey left, CooldownKey right)
            {
                return !(left == right);
            }
        }
    }
}
