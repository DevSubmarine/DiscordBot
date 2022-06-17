namespace DevSubmarine.DiscordBot.SubWords
{
    /// <summary>Store for <see cref="SubWord"/> entities.</summary>
    public interface ISubWordsStore
    {
        /// <summary>Stores a new sub word.</summary>
        /// <param name="word">Word to store</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task AddWordAsync(SubWord word, CancellationToken cancellationToken = default);
        /// <summary>Retrieves word data from the store.</summary>
        /// <param name="word">Word to retrieve data for.</param>
        /// <param name="authorID">ID of the word's author.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Word data if found; otherwise null.</returns>
        Task<SubWord> GetWordAsync(string word, ulong authorID, CancellationToken cancellationToken = default);
        /// <summary>Retrieves all words from the store.</summary>
        /// <param name="authorID">ID of the author; if null, will retrieve all words for all authors.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Enumerable of found words.</returns>
        Task<IEnumerable<SubWord>> GetAllWordsAsync(ulong? authorID, CancellationToken cancellationToken = default);
        /// <summary>Retrieves a random word from the store.</summary>
        /// <param name="authorID">ID of the author; if null, will retrieve a random word from any author.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Data of the found word.</returns>
        Task<SubWord> GetRandomWordAsync(ulong? authorID, CancellationToken cancellationToken = default);
        /// <summary>Retrieves count of the stored words.</summary>
        /// <param name="authorID">ID of the author; if null, will count all words from all authors.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Count of stored words.</returns>
        Task<long> GetWordsCountAsync(ulong? authorID, CancellationToken cancellationToken = default);
    }
}
