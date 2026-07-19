namespace ProjectTemplate.Infrastructure.Data.Entities;

/// <summary>
/// Represents a minimized, durable audit-integrity finding.
/// </summary>
public sealed class ApplicationAuditReconciliationFinding : DataEntity
{
    public const string CurrentSchemaVersion = "1.0";

    public string SchemaVersion { get; set; } = CurrentSchemaVersion;

    public string FindingKey { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string MutationBatchId { get; set; } = string.Empty;

    public string? Destination { get; set; }

    public string Guidance { get; set; } = string.Empty;

    public string RemediationStatus { get; set; } = "Open";

    public DateTime FirstObservedUtc { get; set; }

    public DateTime LastObservedUtc { get; set; }

    public DateTime? ResolvedUtc { get; set; }
}

/// <summary>
/// Represents append-only evidence that an operator investigated or remediated a finding.
/// </summary>
public sealed class ApplicationAuditReconciliationRemediation : DataEntity
{
    public Guid FindingId { get; set; }

    public string MutationBatchId { get; set; } = string.Empty;

    public string ActionCode { get; set; } = string.Empty;

    public string ActorId { get; set; } = string.Empty;

    public string? EvidenceReference { get; set; }

    public DateTime RecordedUtc { get; set; }
}
