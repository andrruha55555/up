using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace AdminUP.Services
{
    public class CacheService
    {
        private readonly Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();
        private readonly Timer _cleanupTimer;

        public CacheService()
        {
            // Таймер для очистки устаревших записей каждые 5 минут
            _cleanupTimer = new Timer(300000); // 5 минут
            _cleanupTimer.Elapsed += CleanupCache;
            _cleanupTimer.Start();
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getter, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out var cacheItem) &&
                !cacheItem.IsExpired)
            {
                return (T)cacheItem.Value;
            }

            var value = await getter();
            Set(key, value, expiration);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var expirationTime = expiration ?? TimeSpan.FromMinutes(10);
            _cache[key] = new CacheItem(value, expirationTime);
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_cache.TryGetValue(key, out var cacheItem) &&
                !cacheItem.IsExpired)
            {
                value = (T)cacheItem.Value;
                return true;
            }

            value = default;
            return false;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private void CleanupCache(object sender, ElapsedEventArgs e)
        {
            var expiredKeys = new List<string>();

            foreach (var kvp in _cache)
            {
                if (kvp.Value.IsExpired)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }
        }

        private class CacheItem
        {
            public object Value { get; }
            public DateTime ExpirationTime { get; }

            public bool IsExpired => DateTime.Now > ExpirationTime;

            public CacheItem(object value, TimeSpan expiration)
            {
                Value = value;
                ExpirationTime = DateTime.Now.Add(expiration);
            }
        }
    }
}
