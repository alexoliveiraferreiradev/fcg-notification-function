using Fcg.Notification.Function.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class CachingExtensions
    {
        public static IServiceCollection AddCachingExtension(this IServiceCollection services, IConfiguration configuration )
        {
            var redisConfig = configuration.GetSection(RedisSettings.RedisSectionName).Get<RedisSettings>();
            ArgumentNullException.ThrowIfNull(redisConfig, nameof(RedisSettings));
            services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.RedisSectionName));

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { { redisConfig.Host, redisConfig.Port } },
                Password = redisConfig.Password,
                AbortOnConnectFail = false,
                ConnectRetry = 5,
                ReconnectRetryPolicy = new ExponentialRetry(5000, 30000)
            };

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = configurationOptions;
                options.InstanceName = redisConfig.InstanceName;
            });
            return services;
        }
    }
}
