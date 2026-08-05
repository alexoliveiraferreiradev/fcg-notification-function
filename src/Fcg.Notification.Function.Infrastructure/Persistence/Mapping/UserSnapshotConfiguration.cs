using Fcg.Notification.Function.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Notification.Function.Infrastructure.Persistence.Mapping
{
    internal class UserSnapshotConfiguration : IEntityTypeConfiguration<UserSnapshot>
    {
        public void Configure(EntityTypeBuilder<UserSnapshot> builder)
        {
            builder.ToTable("UsersSnapshots");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(250)
                .IsRequired();
        }
    }
}
