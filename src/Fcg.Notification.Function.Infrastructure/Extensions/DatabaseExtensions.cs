using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<AzureDatabaseSettings>()
                .Bind(configuration.GetSection(AzureDatabaseSettings.AzureDatabaseSettingsSection))
                .ValidateDataAnnotations().ValidateOnStart();

            var dbConfig = configuration.GetSection(AzureDatabaseSettings.AzureDatabaseSettingsSection).Get<AzureDatabaseSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(AzureDatabaseSettings));

            services.AddDbContext<NotificationDbContext>(options =>
            {
                options.UseSqlServer(dbConfig.ConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 3,
                       maxRetryDelay: TimeSpan.FromSeconds(30),
                       errorNumbersToAdd: null);

                });
            });

            return services;
        }
    }
}
