using DevSubmarine.DiscordBot.Caching;
using DevSubmarine.DiscordBot.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.DiscordBot.SubWords
{
    /// <summary>Represents Luke's misspelled word + metadata.</summary>
    public class SubWord : IEquatable<SubWord>, IEquatable<string>, ICacheable
    {
        [BsonId]
        [JsonProperty("word")]
        public string Word { get; }
        [BsonElement("Author")]
        [JsonProperty("author")]
        public ulong AuthorID { get; }
        [BsonElement("AddedBy")]
        [JsonProperty("addedBy")]
        public ulong AddedByUserID { get; }
        [BsonElement("CreationTime")]
        [JsonProperty("creationTime")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreationTimeUTC { get; }

        // additional metadata, in case needed in future?
        [BsonElement("GuildID")]
        [JsonIgnore]
        public ulong? GuildID { get; set; }
        [BsonElement("ChannelID")]
        [JsonIgnore]
        public ulong? ChannelID { get; set; }
        [BsonElement("MessageID")]
        [JsonIgnore]
        public ulong? MessageID { get; set; }

        [BsonConstructor(nameof(Word), nameof(AuthorID), nameof(AddedByUserID), nameof(CreationTimeUTC))]
        public SubWord(string word, ulong authorID, ulong addedByUserID, DateTime creationTimeUTC)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));
            if (authorID <= 0)
                throw new ArgumentException("Author ID must be a valid Discord user ID", nameof(authorID));

            this.Word = SubWord.Trim(word);
            this.AuthorID = authorID;
            this.AddedByUserID = addedByUserID;
            this.CreationTimeUTC = creationTimeUTC;
        }

        // store as lowercase - case does not matter, while storing as lowercase reduces the need for complex indexing
        public static string Trim(string word)
            => word.Trim().ToLowerInvariant();

        public CacheItemKey GetCacheKey()
            => new CacheItemKey(this.GetType(), (this.AuthorID, this.Word));
        public static CacheItemKey GetCacheKey(string word, ulong authorID)
            => new CacheItemKey(typeof(SubWord), (authorID, Trim(word)));

        public SubWord(string word, ulong authorID, ulong addedByUserID)
            : this(word, authorID, addedByUserID, DateTime.UtcNow) { }

        /// <inheritdoc/>
        public override string ToString()
            => this.Word;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is SubWord wordObject)
                return this.Equals(wordObject);
            if (obj is string wordString)
                return this.Equals(wordString);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(SubWord other)
            => other != null 
            && this.Word.Equals(other.Word, StringComparison.OrdinalIgnoreCase)
            && this.AuthorID.Equals(other.AuthorID);

        /// <inheritdoc/>
        public bool Equals(string other)
            => other != null && this.Word.Equals(other, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override int GetHashCode()
            => HashCode.Combine(this.AuthorID, this.Word);

        /// <inheritdoc/>
        public static bool operator ==(SubWord left, SubWord right)
            => EqualityComparer<SubWord>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(SubWord left, SubWord right)
            => !(left == right);
    }
}
