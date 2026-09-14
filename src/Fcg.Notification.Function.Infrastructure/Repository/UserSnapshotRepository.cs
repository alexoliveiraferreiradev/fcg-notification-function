using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notification.Function.Infrastructure.Repository
{
    internal class UserSnapshotRepository : IUserSnapshotRepository
    {
        private readonly NotificationDbContext _context;

        public UserSnapshotRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public void Add(UserSnapshot userSnapshot)
        {
            _context.UserSnapshots.Add(userSnapshot);
        }

        public async Task<UserSnapshot?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserSnapshots
                 .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }

        public void Update(UserSnapshot userSnapshot)
        {
            _context.Update(userSnapshot);
        }
    }
}
