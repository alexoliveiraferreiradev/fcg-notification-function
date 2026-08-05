using Fcg.Notification.Function.Domain.Entities;

namespace Fcg.Notification.Function.Domain.Repositories
{
    public interface INotificationRepository
    {
        void Add(NotificationMessage notificationMessage);
        Task<NotificationMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);  
    }
}
