namespace DevSubmarine.DiscordBot.Caching
{
    public class CacheItemKey : IEquatable<CacheItemKey>
    {
        public Type CacheType { get; }
        public object ItemIdentifier { get; }

        public CacheItemKey(Type cacheType, object itemIdentifier)
        {
            this.CacheType = cacheType;
            this.ItemIdentifier = itemIdentifier;
        }

        public override bool Equals(object obj)
            => this.Equals(obj as CacheItemKey);

        public bool Equals(CacheItemKey other)
        {
            return other != null &&
                   this.CacheType == other.CacheType &&
                   EqualityComparer<object>.Default.Equals(this.ItemIdentifier, other.ItemIdentifier);
        }

        public override int GetHashCode()
            => HashCode.Combine(this.CacheType, this.ItemIdentifier);

        public static bool operator ==(CacheItemKey left, CacheItemKey right)
            => EqualityComparer<CacheItemKey>.Default.Equals(left, right);

        public static bool operator !=(CacheItemKey left, CacheItemKey right)
            => !(left == right);
    }
}
