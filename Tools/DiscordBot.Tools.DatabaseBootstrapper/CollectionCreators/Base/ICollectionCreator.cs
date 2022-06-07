namespace DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper.CollectionCreators
{
    public interface ICollectionCreator
    {
        Task ProcessCollectionAsync(CancellationToken cancellationToken = default);
    }
}
