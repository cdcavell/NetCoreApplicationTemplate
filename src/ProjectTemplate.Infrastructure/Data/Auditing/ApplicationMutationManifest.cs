namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Represents a versioned canonical mutation manifest built from privacy-protected audit records.
/// </summary>
/// <param name="SchemaVersion">The canonical manifest schema version.</param>
/// <param name="MutationBatchId">The mutation batch represented by the manifest.</param>
/// <param name="AuditRecordCount">The number of audit records represented exactly once.</param>
/// <param name="CanonicalJson">The deterministic UTF-8 JSON text used for hashing and archival verification.</param>
public sealed record ApplicationMutationManifest(
    string SchemaVersion,
    string MutationBatchId,
    int AuditRecordCount,
    string CanonicalJson)
{
    /// <summary>
    /// The current canonical mutation manifest schema version.
    /// </summary>
    public const string CurrentSchemaVersion = "1.0";
}
