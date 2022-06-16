using DevSubmarine.DiscordBot.Database;
using MongoDB.Driver;

namespace DevSubmarine.DiscordBot.Voting.Services
{
    internal class MongoVotesStore : IVotesStore
    {
        private readonly ILogger _log;
        private readonly IMongoCollection<Vote> _collection;

        public MongoVotesStore(IMongoDatabaseClient client, IOptions<MongoOptions> databaseOptions, ILogger<MongoVotesStore> log)
        {
            this._log = log;
            this._collection = client.GetCollection<Vote>(databaseOptions.Value.SubWordsCollectionName);
        }

        public Task AddVoteAsync(Vote vote, CancellationToken cancellationToken = default)
        {
            if (vote == null)
                throw new ArgumentNullException(nameof(vote));

            this._log.LogTrace("Inserting vote {Vote} to DB", vote);
            return this._collection.InsertOneAsync(vote, null, cancellationToken);
        }

        public async Task<IEnumerable<Vote>> GetVotesAsync(ulong? targetID, ulong? voterID, VoteType? type, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Getting votes from DB; filters: TargetID = {TargetID}, VoterID = {VoterID}, Type = {Type}", targetID, voterID, type);
            FilterDefinition<Vote> targetFilter = targetID == null
                ? Builders<Vote>.Filter.Empty
                : Builders<Vote>.Filter.Eq(db => db.TargetID, targetID.Value);
            FilterDefinition<Vote> voterFilter = voterID == null
                ? Builders<Vote>.Filter.Empty
                : Builders<Vote>.Filter.Eq(db => db.VoterID, voterID.Value);
            FilterDefinition<Vote> typeFilter = type == null
                ? Builders<Vote>.Filter.Empty
                : Builders<Vote>.Filter.Eq(db => db.Type, type.Value);

            using IAsyncCursor<Vote> words =
                await this._collection
                .Find(Builders<Vote>.Filter.And(targetFilter, voterFilter, typeFilter))
                .ToCursorAsync(cancellationToken).ConfigureAwait(false);
            return await words.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
