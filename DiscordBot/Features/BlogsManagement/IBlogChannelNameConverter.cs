namespace DevSubmarine.DiscordBot.BlogsManagement
{
    /// <summary>Service responsible for converting usernames into blog channel names.</summary>
    public interface IBlogChannelNameConverter
    {
        /// <summary>Checks whether given username is suitable for use for blog channel name.</summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if the username can be converted to blog channel name; otherwise false.</returns>
        bool IsUsernameAllowed(string username);
        /// <summary>Converts given username to blog channel name.</summary>
        /// <param name="username">Username to convert.</param>
        /// <returns>Username converted into a blog channel name.</returns>
        /// <exception cref="ArgumentException"><paramref name="username"/> has failed the validation.</exception>
        string ConvertUsername(string username);
    }
}
