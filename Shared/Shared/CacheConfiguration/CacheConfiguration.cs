namespace Shared.CacheConfiguration
{
    public static class CacheConfiguration<T>
    {
        private static readonly Dictionary<string, T> _cache = new();

        public static void Set(string key, T value)
        {
            _cache[key] = value;
        }
            

        public static void LoadAll<TKey>(IEnumerable<T> items,Func<T, TKey> keySelector)
        {
            _cache.Clear(); 
            foreach (var item in items)
            {
                var key = keySelector(item)?.ToString();
                if (!string.IsNullOrWhiteSpace(key))
                    _cache[key] = item;
            }
        }

        public static T Get(string key)
            => _cache.TryGetValue(key, out var v) ? v : default!;
    }

}
