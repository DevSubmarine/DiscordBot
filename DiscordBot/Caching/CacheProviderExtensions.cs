using Microsoft.Extensions.Caching.Memory;

namespace DevSubmarine.DiscordBot.Caching
{
    internal static class CacheProviderExtensions
    {
        public static void ConfigureCache<TItem>(this ICacheProvider provider, MemoryCacheEntryOptions options)
            => provider.ConfigureCache(CacheItemKey.GetCacheName<TItem>(), options);

        public static TItem AddItem<TItem>(this ICacheProvider provider, string cacheName, object itemKey, TItem item)
            => provider.AddItem(new CacheItemKey(cacheName, itemKey), item);
        public static TItem AddItem<TItem>(this ICacheProvider provider, object itemKey, TItem item)
            => provider.AddItem(CacheItemKey.GetCacheName<TItem>(), itemKey, item);
        public static TItem AddItem<TItem>(this ICacheProvider provider, TItem item)
            => provider.AddItem(CacheItemKey.FromItem<TItem>(item), item);

        public static bool TryGetItem<TItem>(this ICacheProvider provider, string cacheName, object itemKey, out TItem item)
            => provider.TryGetItem(new CacheItemKey(cacheName, itemKey), out item);
        public static bool TryGetItem<TItem>(this ICacheProvider provider, object itemKey, out TItem item)
            => provider.TryGetItem(CacheItemKey.GetCacheName<TItem>(), itemKey, out item);
    }
}
