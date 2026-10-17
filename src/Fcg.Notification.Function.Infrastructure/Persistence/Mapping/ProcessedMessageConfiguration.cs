using Fcg.Notification.Function.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Notification.Function.Infrastructure.Persistence.Mapping
{
    internal class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
    {
        public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
        {
            builder.ToTable("ProcessedMessages");

            builder.HasKey(p => p.Key);

            builder.HasIndex(p => p.CreatedAt);
        }
    }
}
