namespace FM.Infrastructure.Cache.Redis
{
    public class CacheConfig
    {
        public bool UseRedis { get; set; }
        public string RedisConnection { get; set; } = string.Empty;
    }
}
