namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public interface IBlogActivator
    {
        Task ActivateBlogChannel(ulong channelID, ulong guildID, CancellationToken cancellationToken = default);
        Task DeactivateBlogChannel(ulong channelID, ulong guildID, CancellationToken cancellationToken = default);
    }
}
