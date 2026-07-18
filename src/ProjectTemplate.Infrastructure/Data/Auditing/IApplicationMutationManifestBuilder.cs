using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Builds a versioned canonical manifest from privacy-protected application audit records.
/// </summary>
public interface IApplicationMutationManifestBuilder
{
    /// <summary>
    /// Builds the canonical manifest for one mutation batch.
    /// </summary>
    ApplicationMutationManifest Build(IReadOnlyCollection<AuditRecord> auditRecords);
}
