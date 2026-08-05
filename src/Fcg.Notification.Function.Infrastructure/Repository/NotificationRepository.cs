using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notification.Function.Infrastructure.Repository
{
    internal class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public void Add(NotificationMessage notificationMessage)
        {
            _context.Notifications.Add(notificationMessage);
        }

        public async Task<NotificationMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }
    }
}
