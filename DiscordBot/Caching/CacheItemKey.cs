namespace DevSubmarine.DiscordBot.Caching
{
    internal class CacheItemKey : IEquatable<CacheItemKey>
    {
        public string CacheName { get; }
        public object ItemKey { get; }

        public CacheItemKey(string cacheName, object itemKey)
        {
            this.CacheName = cacheName;
            this.ItemKey = itemKey;
        }

        public static CacheItemKey FromItem<TItem>(TItem item)
            => new CacheItemKey(GetCacheName<TItem>(), item.GetHashCode());

        public static string GetCacheName<TItem>()
            => typeof(TItem).FullName;

        public override bool Equals(object obj)
            => this.Equals(obj as CacheItemKey);

        public bool Equals(CacheItemKey other)
        {
            return other != null &&
                   this.CacheName == other.CacheName &&
                   EqualityComparer<object>.Default.Equals(this.ItemKey, other.ItemKey);
        }

        public override int GetHashCode()
            => HashCode.Combine(this.CacheName, this.ItemKey);

        public static bool operator ==(CacheItemKey left, CacheItemKey right)
            => EqualityComparer<CacheItemKey>.Default.Equals(left, right);

        public static bool operator !=(CacheItemKey left, CacheItemKey right)
            => !(left == right);
    }
}
