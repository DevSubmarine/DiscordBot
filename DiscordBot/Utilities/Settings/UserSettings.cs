using DevSubmarine.DiscordBot.Caching;
using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.DiscordBot.Settings
{
    public class UserSettings : ICacheable
    {
        [BsonId]
        public ulong UserID { get; }

        [BsonElement]
        public bool PingOnVote { get; set; } = true;

        [BsonConstructor(nameof(UserID))]
        public UserSettings(ulong userID)
        {
            if (userID <= 0)
                throw new ArgumentException($"{userID} is not a valid Discord user ID.", nameof(userID));

            this.UserID = userID;
        }

        public CacheItemKey GetCacheKey()
            => new CacheItemKey(this.GetType(), this.UserID);
        public static CacheItemKey GetCacheKey(ulong userID)
            => new CacheItemKey(typeof(UserSettings), userID);
    }
}
