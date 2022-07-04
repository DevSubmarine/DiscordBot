namespace DevSubmarine.DiscordBot.Settings
{
    /// <summary>Store for <see cref="UserSettings"/> entities.</summary>
    public interface IUserSettingsStore
    {
        /// <summary>Retrieves user settings from the store.</summary>
        /// <param name="userID">User to retrieve settings for.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Retrieved user settings; null if not found.</returns>
        Task<UserSettings> GetUserSettingsAsync(ulong userID, CancellationToken cancellationToken = default);
        /// <summary>Adds or updates user settings in the store.</summary>
        /// <param name="settings">Settings instance to save in store.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task UpsertUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
    }
}
