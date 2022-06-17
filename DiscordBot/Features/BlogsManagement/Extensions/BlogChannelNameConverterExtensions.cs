namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public static class BlogChannelNameConverterExtensions
    {
        /// <summary>Attempts to convert given username to blog channel name.</summary>
        /// <param name="converter">The service instance.</param>
        /// <param name="username">Username to convert.</param>
        /// <param name="result">Username converted into a blog channel name; null if conversion failed.</param>
        /// <returns>True if the username was successfully converted; otherwise false.</returns>
        public static bool TryConvertUsername(this IBlogChannelNameConverter converter, string username, out string result)
        {
            try
            {
                result = converter.ConvertUsername(username);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }
    }
}
