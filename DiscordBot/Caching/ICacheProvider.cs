using Microsoft.Extensions.Caching.Memory;

namespace DevSubmarine.DiscordBot.Caching
{
    internal interface ICacheProvider<TItem>
    {
        void ConfigureCache(Action<MemoryCacheEntryOptions> configureOptions);
        TItem AddItem(CacheItemKey key, TItem item);
        bool TryGetItem(CacheItemKey key, out TItem item);
    }
}
