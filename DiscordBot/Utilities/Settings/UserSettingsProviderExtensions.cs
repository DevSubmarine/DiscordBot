namespace DevSubmarine.DiscordBot.Settings
{
    public static class UserSettingsProviderExtensions
    {
        public static async Task UpdateUserSettingsAsync(this IUserSettingsProvider provider, ulong userID, Action<UserSettings> updates, CancellationToken cancellationToken = default)
        {
            if (updates == null)
                return;

            UserSettings settings = await provider.GetUserSettingsAsync(userID, cancellationToken).ConfigureAwait(false);
            updates.Invoke(settings);
            await provider.UpdateUserSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        }
    }
}
