using DevSubmarine.DiscordBot.Caching;
using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.DiscordBot.Birthdays
{
    public class UserBirthday : ICacheable
    {
        [BsonId]
        public ulong UserID { get; }
        [BsonElement("date")]
        public BirthdayDate Date { get; set; }

        [BsonConstructor(nameof(UserID), nameof(Date))]
        public UserBirthday(ulong userID, BirthdayDate date)
        {
            this.UserID = userID;
            this.Date = date;
        }

        public CacheItemKey GetCacheKey()
            => new CacheItemKey(this.GetType(), this.UserID);
        public static CacheItemKey GetCacheKey(ulong userID)
            => new CacheItemKey(typeof(UserBirthday), userID);
    }
}
