namespace DevSubmarine.DiscordBot.SubWords
{
    /// <summary>Service used for sub words operations.</summary>
    public interface ISubWordsService
    {
        /// <summary>Retrieves word data.</summary>
        /// <param name="word">Word to retrieve data for.</param>
        /// <param name="authorID">ID of the word's author.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Word data if found; otherwise null.</returns>
        Task<SubWord> GetSubWordAsync(string word, ulong authorID, CancellationToken cancellationToken = default);
        /// <summary>Retrieves existing or adds new word.</summary>
        /// <param name="word">Word to add or retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Found or newly created word data.</returns>
        Task<SubWord> AddOrGetWordAsync(SubWord word, CancellationToken cancellationToken = default);
        /// <summary>Retrieves a random word.</summary>
        /// <param name="authorID">ID of the author; if null, will retrieve a random word from any author.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Data of the found word.</returns>
        Task<SubWord> GetRandomWordAsync(ulong? authorID, CancellationToken cancellationToken = default);
        /// <summary>Uploads list of words to external hosting, and retrieves the link.</summary>
        /// <param name="authorID">ID of the author of words.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>URL at which the words list can be accessed.</returns>
        Task<string> UploadWordsListAsync(ulong authorID, CancellationToken cancellationToken = default);
    }
}
