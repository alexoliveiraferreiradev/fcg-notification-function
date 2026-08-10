using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class ObservabilityExtension
    {
        public static IServiceCollection AddHealthCheckExtension(this IServiceCollection services,IConfiguration configuration)
        {           
            services.AddHealthChecks()
                .AddDbContextCheck<NotificationDbContext>(
                name: "database-healthcheck",
                tags: new[] { "ready" })
               .AddRedis(
                   sp => sp.GetRequiredService<IConnectionMultiplexer>(),
                   name: "redis-healthcheck",
                   tags: new[] { "ready" });

            return services;
        }
    }
}
