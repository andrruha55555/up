using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AdminUP.Services
{
    public class CacheService
    {
        private sealed class CacheEntry
        {
            public object Value { get; init; }
            public DateTime ExpiresAt { get; init; }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        /// <summary>
        /// Вернуть значение из кэша или положить туда результат фабрики (один раз).
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? ttl = null)
        {
            ttl ??= TimeSpan.FromMinutes(5);

            if (_cache.TryGetValue(key, out var existing))
            {
                if (existing.ExpiresAt > DateTime.Now && existing.Value is T ok)
                    return ok;

                _cache.TryRemove(key, out _);
            }

            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                // повторная проверка после блокировки
                if (_cache.TryGetValue(key, out existing))
                {
                    if (existing.ExpiresAt > DateTime.Now && existing.Value is T ok2)
                        return ok2;

                    _cache.TryRemove(key, out _);
                }

                var value = await factory();
                _cache[key] = new CacheEntry
                {
                    Value = value,
                    ExpiresAt = DateTime.Now.Add(ttl.Value)
                };
                return value;
            }
            finally
            {
                gate.Release();
            }
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }
        public Task<T> GetOrAddAsync<T>(
          string key,
          Func<Task<T>> factory,
         TimeSpan? ttl = null)
        {
            return GetOrSetAsync(key, factory, ttl);
        }

    }
}
