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

        /// <summary>How often blogs should be scanned for activity.</summary>
        /// <remarks>It doesn't really matter if blogs are scanned at exact intervals, so the actual interval might differ slightly.</remarks>
        public TimeSpan ActivityScanningRate { get; set; } = TimeSpan.FromDays(1);
        /// <summary>The time since last message for blog to be considered inactive.</summary>
        public TimeSpan MaxBlogInactivityTime { get; set; } = TimeSpan.FromDays(14);

        /// <summary>Time the member must be in the guild at minimum to create a blog channel.</summary>
        public TimeSpan MinMemberAge { get; set; } = TimeSpan.FromDays(3);
        /// <summary>Words that cannot be contained in blog channel name.</summary>
        /// <remarks>This can be used to forbid certain words, such as swears or whatever.</remarks>
        public IEnumerable<string> ForbiddenChannelNameWords { get; set; }
    }
}
