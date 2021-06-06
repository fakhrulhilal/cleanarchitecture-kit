using FM.Domain.Common;
using FM.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisCacheDependency
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services) => services
            .AddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return configuration.GetSection(typeof(CacheConfig).GetConfigName()).Get<CacheConfig>();
            })
            .AddSingleton<MemoryDistributedCache>()
            .AddSingleton<RedisCache>()
            .AddSingleton<IDistributedCache>(provider =>
            {
                var cacheConfig = provider.GetRequiredService<CacheConfig>();
                if (!cacheConfig.UseRedis) return provider.GetRequiredService<MemoryDistributedCache>();

                services.Configure<RedisCacheOptions>(opt => opt.Configuration = cacheConfig.RedisConnection);
                return provider.GetRequiredService<RedisCache>();
            });
    }
}
