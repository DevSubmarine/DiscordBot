namespace DevSubmarine.DiscordBot.SubWords
{
    public interface ISubWordsStore
    {
        Task AddWordAsync(SubWord word, CancellationToken cancellationToken = default);
        Task<SubWord> GetWordAsync(string word, ulong authorID, CancellationToken cancellationToken = default);
        Task<IEnumerable<SubWord>> GetAllWordsAsync(ulong? authorID, CancellationToken cancellationToken = default);
        Task<SubWord> GetRandomWordAsync(ulong? authorID, CancellationToken cancellationToken = default);
        Task<long> GetWordsCountAsync(ulong? authorID, CancellationToken cancellationToken = default);
    }
}
