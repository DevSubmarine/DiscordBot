namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogActivator
    {
        Task ActivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default);
        Task DeactivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default);
    }
}
