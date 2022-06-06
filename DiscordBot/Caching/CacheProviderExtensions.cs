namespace DevSubmarine.DiscordBot.Caching
{
    internal static class CacheProviderExtensions
    {
        public static TItem AddItem<TItem>(this ICacheProvider<TItem> provider, TItem item) where TItem : ICacheable
            => provider.AddItem(item.GetCacheKey(), item);

        public static bool TryGetItem<TItem>(this ICacheProvider<TItem> provider, TItem item, out TItem result) where TItem : ICacheable
            => provider.TryGetItem(item.GetCacheKey(), out result);
    }
}
