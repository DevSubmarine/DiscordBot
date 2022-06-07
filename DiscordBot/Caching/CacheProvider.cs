using Microsoft.Extensions.Caching.Memory;

namespace DevSubmarine.DiscordBot.Caching.Services
{
    internal class CacheProvider<TItem> : ICacheProvider<TItem>
    {
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly IMemoryCache _cache;

        public CacheProvider(IMemoryCache memoryCache)
        {
            this._cache = memoryCache;
            this._cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));
        }

        public void ConfigureCache(Action<MemoryCacheEntryOptions> configureOptions)
            => configureOptions?.Invoke(this._cacheOptions);

        public TItem AddItem(CacheItemKey key, TItem item)
            => this._cache.Set(key, item, this._cacheOptions);

        public bool TryGetItem(CacheItemKey key, out TItem item)
            => this._cache.TryGetValue(key, out item);
    }
}
