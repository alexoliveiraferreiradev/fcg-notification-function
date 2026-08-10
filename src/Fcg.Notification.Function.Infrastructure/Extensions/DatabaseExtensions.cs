using Fcg.Core.WebApi.Database;
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
            services.AddOptions<DatabaseConnectionSettings>()
                .Bind(configuration.GetSection(DatabaseConnectionSettings.DatabaseSettingsSection))
                .ValidateDataAnnotations().ValidateOnStart();

            var dbConfig = configuration.GetSection(DatabaseConnectionSettings.DatabaseSettingsSection).Get<DatabaseConnectionSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(DatabaseConnectionSettings));

            services.AddDbContext<NotificationDbContext>(options =>
            {
                options.UseSqlServer(dbConfig.ToConnectionString(), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 3,
                       maxRetryDelay: TimeSpan.FromSeconds(10),
                       errorNumbersToAdd: null);

                });
                               
            });

            return services;
        }
    }
}
