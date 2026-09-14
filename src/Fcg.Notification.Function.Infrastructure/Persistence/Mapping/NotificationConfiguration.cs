using Fcg.Notification.Function.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Notification.Function.Infrastructure.Persistence.Mapping
{
    internal class NotificationConfiguration : IEntityTypeConfiguration<NotificationMessage>
    {
        public void Configure(EntityTypeBuilder<NotificationMessage> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n=>n.Id);

            builder.HasIndex(n => n.UserId);

            builder.OwnsOne(n => n.Recipient, nv =>
            {
                nv.Property(e => e.Address)
                    .HasMaxLength(250)
                    .IsRequired();
            });

            
            builder.Property(n => n.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne<UserSnapshot>()
                   .WithMany()
                   .HasForeignKey(n => n.UserId)
                   .HasPrincipalKey(u => u.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
