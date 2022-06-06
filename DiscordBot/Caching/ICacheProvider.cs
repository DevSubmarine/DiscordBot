using Microsoft.Extensions.Caching.Memory;

namespace DevSubmarine.DiscordBot.Caching
{
    internal interface ICacheProvider
    {
        void ConfigureCache(string cacheName, MemoryCacheEntryOptions options);
        TItem AddItem<TItem>(CacheItemKey key, TItem item);
        bool TryGetItem<TItem>(CacheItemKey key, out TItem item);
    }
}
