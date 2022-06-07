using DevSubmarine.DiscordBot.SubWords;

namespace DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper.CollectionCreators
{
    class SubWordsCollectionCreator : CollectionCreatorBase<SubWord>
    {
        protected override string CollectionName { get; }

        public SubWordsCollectionCreator(IMongoDatabaseClient client, ILogger<SubWordsCollectionCreator> log, IOptions<MongoOptions> options)
            : base(client, log, options)
        {
            this.CollectionName = options.Value.SubWordsCollectionName;
        }

        public override async Task ProcessCollectionAsync(CancellationToken cancellationToken = default)
        {
            IMongoCollection<SubWord> collection = await base.GetOrCreateCollectionAsync(cancellationToken).ConfigureAwait(false);

            await this.CreateAuthorIdIndex(collection, cancellationToken).ConfigureAwait(false);
        }

        private Task CreateAuthorIdIndex(IMongoCollection<SubWord> collection, CancellationToken cancellationToken = default)
        {
            base.Log.LogDebug("Creating index on property {Property} in {Collection}", nameof(SubWord.AuthorID), CollectionName);
            return collection.Indexes.CreateOneAsync(new CreateIndexModel<SubWord>(
                Builders<SubWord>.IndexKeys.Ascending(k => k.AuthorID),
                new CreateIndexOptions<SubWord>()
                {
                    Unique = false
                }),
                null, cancellationToken);
        }
    }
}
