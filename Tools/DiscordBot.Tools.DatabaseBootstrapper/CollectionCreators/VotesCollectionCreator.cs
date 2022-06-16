using DevSubmarine.DiscordBot.Voting;

namespace DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper.CollectionCreators
{
    class VotesCollectionCreator : CollectionCreatorBase<Vote>
    {
        protected override string CollectionName { get; }

        public VotesCollectionCreator(IMongoDatabaseClient client, ILogger<VotesCollectionCreator> log, IOptions<MongoOptions> options)
            : base(client, log, options)
        {
            this.CollectionName = options.Value.VotesCollectionName;
        }

        public override async Task ProcessCollectionAsync(CancellationToken cancellationToken = default)
        {
            IMongoCollection<Vote> collection = await base.GetOrCreateCollectionAsync(cancellationToken).ConfigureAwait(false);

            await this.CreateTargetIdIndex(collection, cancellationToken).ConfigureAwait(false);
            await this.CreateVoterIdIndex(collection, cancellationToken).ConfigureAwait(false);
            await this.CreateTypeIndex(collection, cancellationToken).ConfigureAwait(false);
        }

        private Task CreateTargetIdIndex(IMongoCollection<Vote> collection, CancellationToken cancellationToken = default)
        {
            base.Log.LogDebug("Creating index on property {Property} in {Collection}", nameof(Vote.TargetID), CollectionName);
            return collection.Indexes.CreateOneAsync(new CreateIndexModel<Vote>(
                Builders<Vote>.IndexKeys.Ascending(k => k.TargetID),
                new CreateIndexOptions<Vote>()
                {
                    Unique = false
                }),
                null, cancellationToken);
        }

        private Task CreateVoterIdIndex(IMongoCollection<Vote> collection, CancellationToken cancellationToken = default)
        {
            base.Log.LogDebug("Creating index on property {Property} in {Collection}", nameof(Vote.VoterID), CollectionName);
            return collection.Indexes.CreateOneAsync(new CreateIndexModel<Vote>(
                Builders<Vote>.IndexKeys.Ascending(k => k.VoterID),
                new CreateIndexOptions<Vote>()
                {
                    Unique = false
                }),
                null, cancellationToken);
        }

        private Task CreateTypeIndex(IMongoCollection<Vote> collection, CancellationToken cancellationToken = default)
        {
            base.Log.LogDebug("Creating index on property {Property} in {Collection}", nameof(Vote.Type), CollectionName);
            return collection.Indexes.CreateOneAsync(new CreateIndexModel<Vote>(
                Builders<Vote>.IndexKeys.Ascending(k => k.Type),
                new CreateIndexOptions<Vote>()
                {
                    Unique = false
                }),
                null, cancellationToken);
        }
    }
}
