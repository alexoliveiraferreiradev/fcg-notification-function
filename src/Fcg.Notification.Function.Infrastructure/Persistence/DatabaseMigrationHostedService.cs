using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{    
    internal class DatabaseMigrationHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseMigrationHostedService> _logger;

        public DatabaseMigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseMigrationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {           
            await Task.Yield();

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                _logger.LogInformation("Aplicando migrations do banco de notificações.");
                await NotificationSeed.ApplyMigrationsAsync(dbContext, stoppingToken);
                _logger.LogInformation("Migrations aplicadas.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao aplicar as migrations do banco de notificações.");
            }
        }
    }
}
