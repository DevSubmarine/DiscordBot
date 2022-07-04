using DevSubmarine.DiscordBot.Voting;

namespace DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper.CollectionCreators
{
    class UserSettingsCollectionCreator : CollectionCreatorBase<Vote>
    {
        protected override string CollectionName { get; }

        public UserSettingsCollectionCreator(IMongoDatabaseClient client, ILogger<UserSettingsCollectionCreator> log, IOptions<MongoOptions> options)
            : base(client, log, options)
        {
            this.CollectionName = options.Value.UserSettingsCollectionName;
        }

        public override async Task ProcessCollectionAsync(CancellationToken cancellationToken = default)
        {
            IMongoCollection<Vote> collection = await base.GetOrCreateCollectionAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
