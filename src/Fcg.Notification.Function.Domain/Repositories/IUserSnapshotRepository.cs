using Fcg.Notification.Function.Domain.Entities;

namespace Fcg.Notification.Function.Domain.Repositories
{
    public interface IUserSnapshotRepository
    {
        Task<UserSnapshot?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken); 
        void Add(UserSnapshot userSnapshot);
        void Update(UserSnapshot userSnapshot);
    }
}
