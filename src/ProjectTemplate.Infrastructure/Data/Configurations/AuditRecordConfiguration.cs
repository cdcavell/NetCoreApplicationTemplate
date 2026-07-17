using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the EF Core mapping for <see cref="AuditRecord"/>.
/// </summary>
public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("AuditRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.SchemaVersion)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ActorId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ActorType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ModifiedOnUtc)
            .IsRequired();

        builder.Property(x => x.Application)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Entity)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.State)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MutationBatchId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.OperationExecutionId)
            .HasMaxLength(128);

        builder.Property(x => x.ExecutionAttemptId)
            .HasMaxLength(128);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(128);

        builder.Property(x => x.TraceId)
            .HasMaxLength(64);

        builder.Property(x => x.SpanId)
            .HasMaxLength(32);

        builder.Property(x => x.DecisionAuditRecordId)
            .HasMaxLength(128);

        builder.Property(x => x.TenantHash)
            .HasMaxLength(128);

        builder.Property(x => x.OrganizationHash)
            .HasMaxLength(128);

        builder.Property(x => x.KeyValues)
            .IsRequired();

        builder.Property(x => x.OriginalValues)
            .IsRequired();

        builder.Property(x => x.CurrentValues)
            .IsRequired();

        builder.HasIndex(x => x.MutationBatchId);
        builder.HasIndex(x => x.OperationExecutionId);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.DecisionAuditRecordId);
    }
}
