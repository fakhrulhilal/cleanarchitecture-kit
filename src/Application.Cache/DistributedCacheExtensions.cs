using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace FM.Application.Cache
{
    /// <summary>
    /// Get or set distributed cache as JSON value
    /// </summary>
    internal static class DistributedCacheExtensions
    {
        public static Task SetAsync<TValue>(this IDistributedCache cache, TValue value, string[] keys,
            DistributedCacheEntryOptions options, CancellationToken cancellationToken = new())
        {
            var cacheItem = new CacheItem<TValue>
            {
                Keys = keys,
                Value = value
            };
            string json = JsonSerializer.Serialize(cacheItem);
            var allTasks = keys.Select(key => cache.SetStringAsync(key, json, options, cancellationToken));
            return Task.WhenAll(allTasks);
        }

        public static async Task<TValue?> GetAsync<TValue>(this IDistributedCache cache, string key,
            CancellationToken cancellationToken = new())
        {
            var cacheItem = await cache.GetInternalAsync<CacheItem<TValue>>(key, cancellationToken);
            return cacheItem != null ? cacheItem.Value : default;
        }

        public static async Task RemoveAsync(this IDistributedCache cache, string key, CancellationToken cancellationToken = new())
        {
            var cacheItem = await cache.GetInternalAsync<CacheItem<Dummy>>(key, cancellationToken);
            if (cacheItem == null) return;

            var tasks = cacheItem.Keys.Select(subKey => cache.RemoveAsync(subKey, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private static async Task<TValue?> GetInternalAsync<TValue>(this IDistributedCache cache, string key,
            CancellationToken cancellationToken)
        {
            string json = await cache.GetStringAsync(key, cancellationToken);
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<TValue>(json);
        }

        private struct Dummy { }
        private class CacheItem<TEntity>
        {
            public TEntity Value { get; init; } = default!;
            public string[] Keys { get; init; } = System.Array.Empty<string>();
        }
    }
}
