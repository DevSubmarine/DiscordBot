namespace DevSubmarine.DiscordBot.PasteMyst
{
    public interface IPasteMystClient
    {
        Task<Paste> CreatePasteAsync(Paste paste, CancellationToken cancellationToken = default);
    }
}
