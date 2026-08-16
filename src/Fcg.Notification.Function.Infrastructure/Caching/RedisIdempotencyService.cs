using Fcg.Core.WebApi.Caching;
using Fcg.Notification.Function.Application.Ports;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Fcg.Notification.Function.Infrastructure.Idempotency
{
    public class RedisIdempotencyService : IIdempotencyService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisConnectionSettings _redisOptions;

        public RedisIdempotencyService(IConnectionMultiplexer redis, IOptions<RedisConnectionSettings> redisOptions)
        {
           _redis = redis;
           _redisOptions = redisOptions.Value;
        }

        public async Task ReleaseAsync(string key)
        {
            var db = _redis.GetDatabase();
            var fullKey = $"{_redisOptions.InstanceName}{key}";
            await db.KeyDeleteAsync(fullKey);
        }

        public async Task<bool> TryProcessAsync(string key)
        {
            var db = _redis.GetDatabase();
            var fullKey = $"{_redisOptions.InstanceName}{key}";
            var expiry = TimeSpan.FromDays(_redisOptions.ExpirationInDays);
            return await db.StringSetAsync(fullKey, "processing_or_processed", expiry, When.NotExists);
        }
    }
}
