using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
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

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = $"{dbConfig.Host},{dbConfig.Port}",
                InitialCatalog = dbConfig.DatabaseName,
                UserID = dbConfig.Username,
                Password = dbConfig.Password,
                TrustServerCertificate = true,
                Encrypt = false
            };

            services.AddDbContext<NotificationDbContext>(options =>
            {
                options.UseSqlServer(connectionStringBuilder.ConnectionString, sqlOptions =>
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
