namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogChannelNameConverter
    {
        bool IsUsernameAllowed(string username);
        string ConvertUsername(string username);
    }
}
