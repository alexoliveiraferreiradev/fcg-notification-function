using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class DbContextExtensions
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection(DatabaseSettings.DatabaseSettingsSection).Get<DatabaseSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(DatabaseSettings));

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
