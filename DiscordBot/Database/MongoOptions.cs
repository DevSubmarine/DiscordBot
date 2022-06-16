namespace DevSubmarine.DiscordBot.Database
{
    /// <summary>Options for MongoDB.</summary>
    public class MongoOptions
    {
        /// <summary>Connection string to the database.</summary>
        /// <remarks><para>The user with this connection string needs to have `readWrite` permissions to the database.</para>
        /// <para>If used by bootstrapper, it should have `dbAdmin` permission on <see cref="DatabaseName"/> so it can create the database, create necessary indexes etc.</para></remarks>
        public string ConnectionString { get; set; }
        /// <summary>Name of the database to access.</summary>
        public string DatabaseName { get; set; } = "DiscordBot";

        public string SubWordsCollectionName { get; set; } = "SubWords";
        public string VotesCollectionName { get; set; } = "Votes";
    }
}
