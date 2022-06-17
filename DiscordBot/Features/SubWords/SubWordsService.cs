using DevSubmarine.DiscordBot.Caching;
using DevSubmarine.DiscordBot.PasteMyst;
using Discord;
using Discord.WebSocket;
using System.Net.Http;

namespace DevSubmarine.DiscordBot.SubWords.Services
{
    /// <inheritdoc/>
    internal class SubWordsService : ISubWordsService
    {
        private readonly ISubWordsStore _store;
        private readonly ICacheProvider<SubWord> _cache;
        private readonly IPasteMystClient _pastemyst;
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<SubWordsOptions> _options;
        private readonly ILogger _log;

        public SubWordsService(ISubWordsStore store, ICacheProvider<SubWord> cache, IPasteMystClient pastemyst, DiscordSocketClient client,
            IOptionsMonitor<SubWordsOptions> options, ILogger<SubWordsService> log)
        {
            this._store = store;
            this._cache = cache;
            this._pastemyst = pastemyst;
            this._client = client;
            this._options = options;
            this._log = log;
        }

        /// <inheritdoc/>
        public async Task<SubWord> GetSubWordAsync(string word, ulong authorID, CancellationToken cancellationToken = default)
        {
            word = SubWord.Trim(word);

            // if already cached, we want to spare a network roundtrip to the DB
            if (this._cache.TryGetItem(SubWord.GetCacheKey(word, authorID), out SubWord result))
            {
                this._log.LogDebug("Retrieved SubWord {Word} from cache; Author ID = {AuthorID}", result.Word, result.AuthorID);
                return result;
            }

            // once retrieved from DB, we want to cache to spare a network roundtrip
            result = await this._store.GetWordAsync(word, authorID, cancellationToken).ConfigureAwait(false);
            if (result != null)
            {
                this._log.LogDebug("Retrieved SubWord {Word} from DB; Author ID = {AuthorID}", result.Word, result.AuthorID);
                this._cache.AddItem(result);
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<SubWord> AddOrGetWordAsync(SubWord word, CancellationToken cancellationToken = default)
        {
            SubWord result = await this.GetSubWordAsync(word.Word, word.AuthorID, cancellationToken).ConfigureAwait(false);
            if (result != null)
                return result;

            this._log.LogDebug("Adding SubWord {Word} to DB; Author ID = {AuthorID}", word.Word, word.AuthorID);
            await this._store.AddWordAsync(word, cancellationToken).ConfigureAwait(false);
            this._cache.AddItem(word);
            return word;
        }

        /// <inheritdoc/>
        public Task<SubWord> GetRandomWordAsync(ulong? authorID, CancellationToken cancellationToken = default)
        {
            this._log.LogDebug("Retrieving random SubWord; Author ID = {AuthorID}", authorID?.ToString() ?? "null");
            return this._store.GetRandomWordAsync(authorID, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> UploadWordsListAsync(ulong authorID, CancellationToken cancellationToken = default)
        {
            if (authorID <= 0)
                throw new ArgumentException("Author ID must be a valid Discord user ID", nameof(authorID));

            IEnumerable<SubWord> words = await this._store.GetAllWordsAsync(authorID, cancellationToken).ConfigureAwait(false);
            if (!words.Any())
            {
                this._log.LogDebug("No words for user {AuthorID} found, skipping PasteMyst upload", authorID);
                return null;
            }

            // user needs to be retrieved using Discord client so we can get username to display
            this._log.LogDebug("Uploading SubWords list to PasteMyst; Author ID = {AuthorID}", authorID);
            this._log.LogTrace("Retrieving user {AuthorID}", authorID);
            IUser user = await this._client.GetUserAsync(authorID, cancellationToken).ConfigureAwait(false);
            string userIdentifier = user?.GetUsernameWithDiscriminator() ?? authorID.ToString();

            // using pastemyst because list could potentially grow quite large
            try
            {
                this._log.LogTrace("Building Paste contents");
                string title = $"DevSub Dictionary vol. {DateTime.UtcNow}";
                string contentPlain = $"Words in DevSub Dictionary for user {userIdentifier}: {words.Count()}\r\n\r\n{string.Join("\r\n", words)}";
                string contentJson = new JArray(words.Select(w => JObject.FromObject(w))).ToString(Newtonsoft.Json.Formatting.Indented);

                Paste paste = new Paste(title, new Pasty[]
                {
                    new Pasty(contentPlain, $"{title} (Plain Text)", PastyLanguages.PlainText),
                    new Pasty(contentJson, $"{title} (JSON)", PastyLanguages.JSON)
                }, 
                this._options.CurrentValue.ListExpiration);

                paste = await this._pastemyst.CreatePasteAsync(paste, cancellationToken).ConfigureAwait(false);
                this._log.LogDebug("Paste uploaded, URL: {URL}", paste.GetURL());
                return paste.GetURL();
            }
            catch (HttpRequestException) { throw; }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed building words list")) { throw; }
        }
    }
}
