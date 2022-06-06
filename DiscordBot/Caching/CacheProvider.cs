using Microsoft.Extensions.Caching.Memory;

namespace DevSubmarine.DiscordBot.Caching.Services
{
    internal class CacheProvider : ICacheProvider
    {
        public static MemoryCacheEntryOptions DefaultOptions { get; } 
            = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        private readonly IDictionary<string, MemoryCacheEntryOptions> _cacheOptions;
        private readonly IMemoryCache _cache;

        public CacheProvider(IMemoryCache memoryCache)
        {
            this._cache = memoryCache;
            this._cacheOptions = new Dictionary<string, MemoryCacheEntryOptions>();
        }

        public void ConfigureCache(string cacheName, MemoryCacheEntryOptions options)
            => this._cacheOptions[cacheName] = options;

        public TItem AddItem<TItem>(CacheItemKey key, TItem item)
            => this._cache.Set(key, item, GetCacheOptions(key.CacheName));

        public bool TryGetItem<TItem>(CacheItemKey key, out TItem item)
            => this._cache.TryGetValue(key, out item);

        private MemoryCacheEntryOptions GetCacheOptions(string cacheName)
        {
            if (this._cacheOptions.TryGetValue(cacheName, out MemoryCacheEntryOptions result))
                return result;
            this.ConfigureCache(cacheName, DefaultOptions);
            return DefaultOptions;
        }
    }
}
