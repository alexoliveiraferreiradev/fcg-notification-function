using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Exceptions;
using Fcg.Notification.Function.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly NotificationDbContext _context;

        public UnitOfWork(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            var pendingKeys = _context.ChangeTracker.Entries<ProcessedMessage>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity.Key)
                .ToList();

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException) when (pendingKeys.Count > 0)
            {
                // O que falhou não vale mais para este escopo; sem limpar, o próximo
                // SaveChanges do mesmo escopo tentaria inserir tudo de novo.
                _context.ChangeTracker.Clear();

                // Em vez de interpretar o código de erro do provedor, confirmamos no banco:
                // se a chave já existe, outra entrega da mesma mensagem gravou primeiro.
                if (await _context.ProcessedMessages.AnyAsync(pm => pendingKeys.Contains(pm.Key)))
                    throw new DuplicateMessageException($"Mensagem com chave {string.Join(", ", pendingKeys)} já foi processada por outra entrega.");

                throw;
            }
        }
    }
}
