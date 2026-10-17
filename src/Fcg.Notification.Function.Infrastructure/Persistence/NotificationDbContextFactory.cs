using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    internal class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<NotificationDbContextFactory>(optional: true)
                .AddInMemoryCollection(ReadLocalSettings())
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = configuration.GetSection(AzureDatabaseSettings.AzureDatabaseSettingsSection).Get<AzureDatabaseSettings>();
            ArgumentNullException.ThrowIfNull(dbConfig, nameof(AzureDatabaseSettings));

            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseSqlServer(dbConfig.ConnectionString)
                .Options;

            return new NotificationDbContext(options);
        }

       
        private static IEnumerable<KeyValuePair<string, string?>> ReadLocalSettings()
        {
            var localSettings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .Build();

            return localSettings.GetSection("Values")
                .GetChildren()
                .Select(entry => new KeyValuePair<string, string?>(entry.Key.Replace("__", ":"), entry.Value));
        }
    }
}
