using Fcg.Notification.Function.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    internal class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
        public DbSet<NotificationMessage> Notifications { get; set; }
        public DbSet<UserSnapshot> UserSnapshots { get;set; }   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
