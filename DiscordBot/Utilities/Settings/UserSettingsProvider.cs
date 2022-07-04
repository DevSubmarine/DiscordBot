using DevSubmarine.DiscordBot.Caching;

namespace DevSubmarine.DiscordBot.Settings.Services
{
    /// <inheritdoc/>
    internal class UserSettingsProvider : IUserSettingsProvider
    {
        private readonly ILogger _log;
        private readonly IUserSettingsStore _store;
        private readonly ICacheProvider<UserSettings> _cache;

        public UserSettingsProvider(IUserSettingsStore store, ICacheProvider<UserSettings> cache, ILogger<UserSettingsProvider> log)
        {
            this._store = store;
            this._cache = cache;
            this._log = log;
        }

        /// <inheritdoc/>
        public async Task<UserSettings> GetUserSettingsAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            if (this._cache.TryGetItem(UserSettings.GetCacheKey(userID), out UserSettings result))
            {
                this._log.LogTrace("User Settings for user {UserID} retrieved from cache", userID);
                return result;
            }

            result = await this._store.GetUserSettingsAsync(userID, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                this._log.LogDebug("Creating a new default User Settings for user {UserID}", userID);
                result = new UserSettings(userID);
            }

            this._cache.AddItem(result);
            return result;
        }

        /// <inheritdoc/>
        public async Task UpdateUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            await this._store.UpsertUserSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
            this._cache.AddItem(settings);
        }
    }
}
