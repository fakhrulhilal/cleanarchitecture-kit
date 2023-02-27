using DevKit.Application.Models;
using DevKit.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class RedisCacheDependency
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services) => services
        .AddSingleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            return configuration.GetSection(typeof(CacheConfig).GetConfigName()).Get<CacheConfig>() ??
                   throw new InvalidOperationException("Cache config is not configured properly");
        })
        .AddSingleton<MemoryDistributedCache>()
        .AddSingleton<RedisCache>()
        .AddSingleton<IDistributedCache>(provider =>
        {
            var cacheConfig = provider.GetRequiredService<CacheConfig>();
            bool useRedis =
                "redis".Equals(cacheConfig.ProviderName, StringComparison.CurrentCultureIgnoreCase);
            if (!useRedis) return provider.GetRequiredService<MemoryDistributedCache>();

            services.Configure<RedisCacheOptions>(opt => opt.Configuration = cacheConfig.Connection);
            return provider.GetRequiredService<RedisCache>();
        });
}
