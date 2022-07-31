using DevSubmarine.DiscordBot.Birthdays;

namespace DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper.CollectionCreators
{
    class UserBirthdaysCollectionCreator : CollectionCreatorBase<UserBirthday>
    {
        protected override string CollectionName { get; }

        public UserBirthdaysCollectionCreator(IMongoDatabaseClient client, ILogger<UserSettingsCollectionCreator> log, IOptions<MongoOptions> options)
            : base(client, log, options)
        {
            this.CollectionName = options.Value.UserBirthdaysCollectionName;
        }

        public override async Task ProcessCollectionAsync(CancellationToken cancellationToken = default)
        {
            IMongoCollection<UserBirthday> collection = await base.GetOrCreateCollectionAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
