namespace DevSubmarine.DiscordBot.Caching
{
    internal interface ICacheable
    {
        CacheItemKey GetCacheKey();
    }
}
