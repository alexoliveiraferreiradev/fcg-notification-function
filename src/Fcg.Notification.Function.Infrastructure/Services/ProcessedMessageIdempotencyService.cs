using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fcg.Notification.Function.Infrastructure.Services
{
    internal class ProcessedMessageIdempotencyService : IIdempotencyService
    {
        private readonly NotificationDbContext _dbContext;
        private readonly IdempotencySettings _settings;

        public ProcessedMessageIdempotencyService(NotificationDbContext dbContext, IOptions<IdempotencySettings> settings)
        {
            _dbContext = dbContext;
            _settings = settings.Value;
        }

        public async Task<bool> AlreadyProcessed(string key, CancellationToken cancellationToken)
        {
            return await _dbContext.ProcessedMessages.AnyAsync(pm => pm.Key == key, cancellationToken);
        }

        /// <summary>
        /// Só marca a chave para inserção: ela é gravada no mesmo SaveChanges da notificação.
        /// Duas entregas simultâneas podem passar daqui; a PK barra a segunda no commit
        /// (ver <see cref="UnitOfWork"/>).
        /// </summary>
        public async Task<bool> TryProcessAsync(string key, CancellationToken cancellationToken)
        {
            if (await AlreadyProcessed(key, cancellationToken))
                return false;

            _dbContext.ProcessedMessages.Add(new ProcessedMessage(key));
            return true;
        }

        public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddDays(-_settings.RetentionInDays);
            return await _dbContext.ProcessedMessages
                .Where(pm => pm.CreatedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
