using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

/// <summary>
/// Configures durable audit-completion outbox persistence.
/// </summary>
public sealed class ApplicationAuditCompletionOutboxEntryConfiguration
    : IEntityTypeConfiguration<ApplicationAuditCompletionOutboxEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationAuditCompletionOutboxEntry> builder)
    {
        builder.ToTable("ApplicationAuditCompletionOutbox");

        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).ValueGeneratedNever();

        builder.Property(entry => entry.SchemaVersion)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(entry => entry.Destination)
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(entry => entry.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(entry => entry.MutationBatchId)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(entry => entry.PersistenceOutcome)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(entry => entry.MutationManifestHash)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(entry => entry.MutationManifestAlgorithm)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(entry => entry.MutationManifestSchemaVersion)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(entry => entry.OperationExecutionId).HasMaxLength(128);
        builder.Property(entry => entry.ExecutionAttemptId).HasMaxLength(128);
        builder.Property(entry => entry.DecisionAuditRecordId).HasMaxLength(128);
        builder.Property(entry => entry.CorrelationId).HasMaxLength(128);
        builder.Property(entry => entry.TraceId).HasMaxLength(64);
        builder.Property(entry => entry.Status)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(entry => entry.LastErrorCode).HasMaxLength(128);
        builder.Property(entry => entry.LastErrorMessage).HasMaxLength(512);

        builder.HasIndex(entry => entry.IdempotencyKey).IsUnique();
        builder.HasIndex(entry => new { entry.Destination, entry.MutationBatchId }).IsUnique();
        builder.HasIndex(entry => new { entry.Status, entry.NextAttemptUtc });
        builder.HasIndex(entry => entry.CreatedUtc);
    }
}
