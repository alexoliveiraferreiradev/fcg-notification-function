using Fcg.Notification.Function.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
        public DbSet<NotificationMessage> Notifications { get; set; }
        public DbSet<UserSnapshot> UserSnapshots { get;set; }   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
