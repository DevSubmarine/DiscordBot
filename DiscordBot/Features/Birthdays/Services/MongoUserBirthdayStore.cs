using DevSubmarine.DiscordBot.Database;
using MongoDB.Driver;

namespace DevSubmarine.DiscordBot.Birthdays.Services
{
    internal class MongoUserBirthdayStore : IUserBirthdaysStore
    {
        private readonly ILogger _log;
        private readonly IMongoCollection<UserBirthday> _collection;

        public MongoUserBirthdayStore(IMongoDatabaseClient client, IOptions<MongoOptions> databaseOptions, ILogger<MongoUserBirthdayStore> log)
        {
            this._log = log;
            this._collection = client.GetCollection<UserBirthday>(databaseOptions.Value.SubWordsCollectionName);
        }

        public async Task<IEnumerable<UserBirthday>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving all birthdays from DB");
            return await this._collection.Find(Builders<UserBirthday>.Filter.Empty).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<UserBirthday> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving birthday for user {UserID} from DB", userID);
            return this._collection.Find(db => db.UserID == userID).FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpdateAsync(UserBirthday birthday, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Updating birthday for user {UserID} in DB", birthday.UserID);
            ReplaceOptions options = new ReplaceOptions() { IsUpsert = true };
            return this._collection.ReplaceOneAsync(db => db.UserID == birthday.UserID, birthday, options, cancellationToken);
        }
    }
}
