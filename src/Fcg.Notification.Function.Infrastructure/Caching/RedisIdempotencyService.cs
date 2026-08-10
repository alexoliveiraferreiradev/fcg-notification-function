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

        public async Task ReleaseAsync(Guid eventId)
        {
            var db = _redis.GetDatabase();
            var key = $"{_redisOptions.InstanceName}:notifications:events:{eventId}";
            await db.KeyDeleteAsync(key);
        }

        public async Task<bool> TryProcessAsync(Guid eventId)
        {
            var db = _redis.GetDatabase();
            var key = $"{_redisOptions.InstanceName}:notifications:events:{eventId}";
            var expiry = TimeSpan.FromDays(3);

            bool isAcquired = await db.StringSetAsync(key, "processing_or_processed", expiry, When.NotExists);

            return isAcquired;
        }
    }
}
