using DevSubmarine.DiscordBot.Database;
using MongoDB.Driver;

namespace DevSubmarine.DiscordBot.Settings.Services
{
    /// <inheritdoc/>
    internal class MongoUserSettingsStore : IUserSettingsStore
    {
        private readonly ILogger _log;
        private readonly IMongoCollection<UserSettings> _collection;

        public MongoUserSettingsStore(IMongoDatabaseClient client, IOptions<MongoOptions> databaseOptions, ILogger<MongoUserSettingsStore> log)
        {
            this._log = log;
            this._collection = client.GetCollection<UserSettings>(databaseOptions.Value.UserSettingsCollectionName);
        }

        /// <inheritdoc/>
        public Task<UserSettings> GetUserSettingsAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            if (userID <= 0)
                throw new ArgumentException($"{userID} is not a valid Discord user ID.", nameof(userID));

            this._log.LogTrace("Retrieving User Settings for user {UserID} from DB", userID);
            return this._collection.Find(db => db.UserID == userID).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task UpsertUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (settings.UserID <= 0)
                throw new ArgumentException($"{settings.UserID} is not a valid Discord user ID.", nameof(settings.UserID));

            this._log.LogTrace("Upserting User Settings for user {UserID} to DB", settings.UserID);
            ReplaceOptions options = new ReplaceOptions();
            options.IsUpsert = true;
            return this._collection.ReplaceOneAsync(db => db.UserID == settings.UserID, settings, options, cancellationToken);
        }
    }
}
