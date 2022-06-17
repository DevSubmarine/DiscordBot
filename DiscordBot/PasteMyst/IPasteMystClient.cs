namespace DevSubmarine.DiscordBot.PasteMyst
{
    /// <summary>A client for posting to https://paste.myst.rs</summary>
    public interface IPasteMystClient
    {
        /// <summary>Submits a new paste to pastemyst.</summary>
        /// <param name="paste">Paste to create.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>Created paste, with actual ID assigned.</returns>
        Task<Paste> CreatePasteAsync(Paste paste, CancellationToken cancellationToken = default);
    }
}
