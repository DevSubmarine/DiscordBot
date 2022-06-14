namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public class BlogsManagementOptions
    {
        /// <summary>ID of category that contains active blog channels.</summary>
        public ulong ActiveBlogsCategoryID { get; set; }
        /// <summary>ID of category that contains inactive blog channels.</summary>
        public ulong InactiveBlogsCategoryID { get; set; }
        /// <summary>IDs of all channels that should be ignored.</summary>
        public IEnumerable<ulong> IgnoredChannelsIDs { get; set; }
    }
}
