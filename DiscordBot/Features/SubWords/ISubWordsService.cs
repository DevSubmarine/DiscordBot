namespace DevSubmarine.DiscordBot.SubWords
{
    public interface ISubWordsService
    {
        Task<SubWord> GetSubWordAsync(string word, ulong authorID, CancellationToken cancellationToken = default);
        Task<SubWord> AddOrGetWordAsync(SubWord word, CancellationToken cancellationToken = default);
        Task<SubWord> GetRandomWordAsync(ulong? authorID, CancellationToken cancellationToken = default);
        Task<string> UploadWordsListAsync(ulong authorID, CancellationToken cancellationToken = default);
    }
}
