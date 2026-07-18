using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Verifies retained audit records against an application mutation audit receipt.
/// </summary>
public interface IApplicationMutationManifestVerifier
{
    /// <summary>
    /// Rebuilds the canonical manifest and verifies its digest and record count.
    /// </summary>
    bool Verify(
        ApplicationMutationAuditReceipt receipt,
        IReadOnlyCollection<AuditRecord> auditRecords);
}
