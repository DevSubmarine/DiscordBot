namespace DevSubmarine.DiscordBot.RandomStatus
{
    public interface IStatusPlaceholderEngine
    {
        Task<string> ConvertPlaceholdersAsync(string status, CancellationToken cancellationToken = default);
    }
}
