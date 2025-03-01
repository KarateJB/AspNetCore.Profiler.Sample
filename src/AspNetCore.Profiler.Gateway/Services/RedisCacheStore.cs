using AspNetCore.Profiler.Gateway.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ocelot.Cache;
using StackExchange.Redis;

namespace AspNetCore.Profiler.Gateway.Services
{
    public class RedisCacheStore : IOcelotCache<CachedResponse>
    {
        private readonly ILogger _logger;
        private readonly RedisSetting _redisSetting;
        private readonly IDatabase? _redisDb;
        private Func<string, string, string> RedisKey = (string region, string key) => $"{region}:{key}";

        public RedisCacheStore(ILogger<RedisCacheStore> logger, IOptions<AppSettings> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redisSetting = options?.Value?.Redis ?? throw new ArgumentNullException(nameof(RedisSetting));

            // Initialize Redis connection
            var redis = ConnectionMultiplexer.Connect(_redisSetting.ConnectionString);
            _redisDb = redis.GetDatabase();
        }

        public void Add(string key, CachedResponse value, TimeSpan ttl, string region)
        {
            string redisKey = RedisKey(region, key);
            if (value is not null && _redisDb != null)
            {
                var cacheValue = JsonConvert.SerializeObject(value);
                _redisDb.StringSet(redisKey, cacheValue, expiry: ttl);
                _logger.LogDebug($"Ocelot cache saved - redis key: \"{redisKey}\"");
            }
        }

        public void AddAndDelete(string key, CachedResponse value, TimeSpan ttl, string region)
        {
            Add(key, value, ttl, region);
        }

        public CachedResponse Get(string key, string region)
        {
            string redisKey = RedisKey(region, key);
            RedisValue cachedData = _redisDb != null ? _redisDb.StringGet(redisKey) : RedisValue.Null;

            if (!cachedData.IsNullOrEmpty)
            {
                _logger.LogDebug($"Ocelot cache found - redis key: \"{redisKey}\"");
                return JsonConvert.DeserializeObject<CachedResponse>(cachedData);
            }

            return null;
        }

        public bool TryGetValue(string key, string region, out CachedResponse value)
        {
            value = Get(key, region);
            return value != null;
        }

        public void ClearRegion(string region)
        {
            // Skip
        }
    }

}