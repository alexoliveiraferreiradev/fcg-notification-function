using Fcg.Notification.Function.Infrastructure.Caching;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class ObservabilityExtension
    {
        public static IServiceCollection AddHealthCheckExtension(this IServiceCollection services,IConfiguration configuration)
        {
            var redisConfig = configuration.GetSection(RedisSettings.RedisSectionName).Get<RedisSettings>();
            ArgumentNullException.ThrowIfNull(redisConfig, nameof(RedisSettings));

            var connectionString = $"{redisConfig.Host}:{redisConfig.Port},password={redisConfig.Password}";

            services.AddHealthChecks()
                .AddDbContextCheck<NotificationDbContext>(
                name: "database-healthcheck",
                tags: new[] { "ready" })
               .AddRedis(
                   connectionString,
                   name: "redis-healthcheck",
                   tags: new[] { "ready" });

            return services;
        }
    }
}
