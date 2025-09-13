using Insight.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insight.Invoicing.Infrastructure.Persistence.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("OutboxEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EventData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ProcessedAt)
            .IsRequired(false);

        builder.Property(e => e.ProcessingAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.LastError)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(e => e.NextRetry)
            .IsRequired(false);

        builder.Property(e => e.MaxRetryAttempts)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => e.EventId)
            .IsUnique()
            .HasDatabaseName("IX_OutboxEvents_EventId");

        builder.HasIndex(e => e.IsProcessed)
            .HasDatabaseName("IX_OutboxEvents_IsProcessed");

        builder.HasIndex(e => e.NextRetry)
            .HasDatabaseName("IX_OutboxEvents_NextRetry");

        builder.HasIndex(e => new { e.IsProcessed, e.NextRetry })
            .HasDatabaseName("IX_OutboxEvents_IsProcessed_NextRetry");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_OutboxEvents_CreatedAt");
    }
}


