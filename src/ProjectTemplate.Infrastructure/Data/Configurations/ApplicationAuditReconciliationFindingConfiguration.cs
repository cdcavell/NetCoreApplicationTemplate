using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

public sealed class ApplicationAuditReconciliationFindingConfiguration
    : IEntityTypeConfiguration<ApplicationAuditReconciliationFinding>
{
    public void Configure(EntityTypeBuilder<ApplicationAuditReconciliationFinding> builder)
    {
        builder.ToTable("ApplicationAuditReconciliationFindings");
        builder.HasKey(finding => finding.Id);
        builder.Property(finding => finding.Id).ValueGeneratedNever();
        builder.Property(finding => finding.SchemaVersion).HasMaxLength(32).IsRequired();
        builder.Property(finding => finding.FindingKey).HasMaxLength(128).IsRequired();
        builder.Property(finding => finding.ReasonCode).HasMaxLength(64).IsRequired();
        builder.Property(finding => finding.Severity).HasMaxLength(32).IsRequired();
        builder.Property(finding => finding.MutationBatchId).HasMaxLength(64).IsRequired();
        builder.Property(finding => finding.Destination).HasMaxLength(128);
        builder.Property(finding => finding.Guidance).HasMaxLength(512).IsRequired();
        builder.Property(finding => finding.RemediationStatus).HasMaxLength(32).IsRequired();
        builder.HasIndex(finding => finding.FindingKey).IsUnique();
        builder.HasIndex(finding => new { finding.RemediationStatus, finding.Severity });
        builder.HasIndex(finding => new { finding.MutationBatchId, finding.ReasonCode });
    }
}

public sealed class ApplicationAuditReconciliationRemediationConfiguration
    : IEntityTypeConfiguration<ApplicationAuditReconciliationRemediation>
{
    public void Configure(EntityTypeBuilder<ApplicationAuditReconciliationRemediation> builder)
    {
        builder.ToTable("ApplicationAuditReconciliationRemediations");
        builder.HasKey(remediation => remediation.Id);
        builder.Property(remediation => remediation.Id).ValueGeneratedNever();
        builder.Property(remediation => remediation.MutationBatchId).HasMaxLength(64).IsRequired();
        builder.Property(remediation => remediation.ActionCode).HasMaxLength(64).IsRequired();
        builder.Property(remediation => remediation.ActorId).HasMaxLength(256).IsRequired();
        builder.Property(remediation => remediation.EvidenceReference).HasMaxLength(256);
        builder.HasIndex(remediation => remediation.FindingId);
        builder.HasIndex(remediation => remediation.MutationBatchId);
    }
}
