namespace DevSubmarine.DiscordBot.BlogsManagement
{
    internal static class BlogChannelNameConverterExtensions
    {
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
