namespace DevSubmarine.DiscordBot.Settings
{
    /// <summary>A store-agnostic provider for <see cref="UserSettings"/> instances.</summary>
    public interface IUserSettingsProvider
    {
        /// <summary>Gets settings for specific user.</summary>
        /// <param name="userID">User to retrieve settings for.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Retrieved user settings; new default settings created for the user if not found.</returns>
        Task<UserSettings> GetUserSettingsAsync(ulong userID, CancellationToken cancellationToken = default);
        /// <summary>Adds or updates user settings.</summary>
        /// <param name="settings">Settings instance to save.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task UpdateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
    }
}
