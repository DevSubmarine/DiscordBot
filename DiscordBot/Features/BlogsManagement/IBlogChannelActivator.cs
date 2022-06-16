namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogChannelActivator
    {
        Task ActivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default);
        Task DeactivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default);
    }
}
