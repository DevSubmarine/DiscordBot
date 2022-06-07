using DevSubmarine.DiscordBot.Database;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DevSubmarine.DiscordBot.SubWords.Services
{
    public class MongoSubWordsStore : ISubWordsStore
    {
        private readonly ILogger _log;
        private readonly IMongoCollection<SubWord> _collection;

        public MongoSubWordsStore(IMongoDatabaseClient client, IOptions<MongoOptions> databaseOptions, ILogger<MongoSubWordsStore> log)
        {
            this._log = log;
            this._collection = client.GetCollection<SubWord>(databaseOptions.Value.SubWordsCollectionName);
        }

        public async Task AddWordAsync(SubWord word, CancellationToken cancellationToken = default)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            this._log.LogTrace("Inserting DevSub word {Word} to DB", word);
            await this._collection.InsertOneAsync(word, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task<SubWord> GetWordAsync(string word, ulong authorID, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentNullException(nameof(word));

            string wordLowercase = word.Trim().ToLowerInvariant();
            this._log.LogTrace("Retrieving DevSub word {word} from DB", word);
            return await this._collection.Find(db => db.Word == wordLowercase && db.AuthorID == authorID).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<SubWord>> GetAllWordsAsync(ulong? authorID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Bulk retrieving DevSub words from DB");
            using IAsyncCursor<SubWord> words = 
                await this._collection
                .Find(this.BuildAuthorFilter(authorID))
                .ToCursorAsync(cancellationToken).ConfigureAwait(false);
            return await words.ToListAsync(cancellationToken).ConfigureAwait(false);    // ToList because ToEnumerable complains when iterated more than once
        }

        public Task<SubWord> GetRandomWordAsync(ulong? authorID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving document sample from DB");
            IMongoQueryable<SubWord> query = this._collection.AsQueryable();
            if (authorID != null) 
                query = query.Where(db => db.AuthorID == authorID);
            return query.Sample(1).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<long> GetWordsCountAsync(ulong? authorID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Counting documents in DB");
            return this._collection.CountDocumentsAsync(BuildAuthorFilter(authorID), null, cancellationToken);
        }

        private FilterDefinition<SubWord> BuildAuthorFilter(ulong? authorID)
        {
            return authorID == null
                ? Builders<SubWord>.Filter.Empty
                : Builders<SubWord>.Filter.Eq(db => db.AuthorID, authorID.Value);
        }
    }
}
