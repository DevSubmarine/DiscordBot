using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.DiscordBot.Voting
{
    /// <summary>Represents a single vote.</summary>
    public class Vote : IEquatable<Vote>
    {
        [BsonId]
        [JsonProperty("id")]
        public Guid ID { get; }
        [BsonElement("type")]
        [JsonProperty("type")]
        public VoteType Type { get; init; }
        [BsonElement("voter")]
        [JsonProperty("voter")]
        public ulong VoterID { get; init; }
        [BsonElement("target")]
        [JsonProperty("target")]
        public ulong TargetID { get; init; }
        [BsonElement("timestamp")]
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; init; }

        [BsonConstructor(nameof(ID))]
        [JsonConstructor]
        private Vote(Guid id)
        {
            this.ID = id;
        }

        public Vote(VoteType type, ulong voterID, ulong targetID, DateTimeOffset timestamp) 
            : this(Guid.NewGuid())
        {
            this.Type = type;
            this.VoterID = voterID;
            this.TargetID = targetID;
            this.Timestamp = timestamp;
        }

        public Vote(VoteType type, ulong voterID, ulong targetID) 
            : this(type, voterID, targetID, DateTimeOffset.UtcNow) { }

        public override string ToString()
            => this.ID.ToString();

        public override bool Equals(object obj)
            => Equals(obj as Vote);

        public bool Equals(Vote other)
            => other != null &&
                   ID.Equals(other.ID) &&
                   Type == other.Type &&
                   VoterID == other.VoterID &&
                   TargetID == other.TargetID &&
                   Timestamp.Equals(other.Timestamp);

        public override int GetHashCode()
            => this.ID.GetHashCode();

        public static bool operator ==(Vote left, Vote right)
            => EqualityComparer<Vote>.Default.Equals(left, right);

        public static bool operator !=(Vote left, Vote right)
            => !(left == right);
    }
}
