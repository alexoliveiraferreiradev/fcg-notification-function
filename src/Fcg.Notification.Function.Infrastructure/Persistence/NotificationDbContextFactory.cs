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
                .AddInMemoryCollection(ReadLocalSettings())
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = configuration.GetSection(DatabaseSettings.DatabaseSettingsSection).Get<DatabaseSettings>();

            if (dbConfig is null || string.IsNullOrWhiteSpace(dbConfig.Host) || string.IsNullOrWhiteSpace(dbConfig.DatabaseName))
            {
                throw new InvalidOperationException(
                    "DatabaseSettings nao configurado para design-time. Preencha a secao Values do local.settings.json " +
                    "(DatabaseSettings__Host, DatabaseSettings__DatabaseName, DatabaseSettings__Username, DatabaseSettings__Password) " +
                    "ou exporte as mesmas chaves como variaveis de ambiente.");
            }

            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseSqlServer(dbConfig.ToConnectionString())
                .Options;

            return new NotificationDbContext(options);
        }

        /// <summary>
        /// Em runtime é o host do Functions que promove Values do local.settings.json a variáveis
        /// de ambiente. Esse host não roda em design-time, então replicamos a conversão aqui:
        /// as chaves vêm achatadas com "__", que é o separador hierárquico da configuração.
        /// </summary>
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
