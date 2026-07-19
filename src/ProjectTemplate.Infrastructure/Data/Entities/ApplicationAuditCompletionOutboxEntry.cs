namespace ProjectTemplate.Infrastructure.Data.Entities;

/// <summary>
/// Represents a minimized, durable audit-completion handoff awaiting external delivery.
/// </summary>
public sealed class ApplicationAuditCompletionOutboxEntry : DataEntity
{
    /// <summary>
    /// Gets the current persisted outbox schema version.
    /// </summary>
    public const string CurrentSchemaVersion = "1.0";

    public string SchemaVersion { get; set; } = CurrentSchemaVersion;

    public string Destination { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string MutationBatchId { get; set; } = string.Empty;

    public int AuditRecordCount { get; set; }

    public string PersistenceOutcome { get; set; } = string.Empty;

    public DateTime ReceiptCompletedUtc { get; set; }

    public string MutationManifestHash { get; set; } = string.Empty;

    public string MutationManifestAlgorithm { get; set; } = string.Empty;

    public string MutationManifestSchemaVersion { get; set; } = string.Empty;

    public string? OperationExecutionId { get; set; }

    public string? ExecutionAttemptId { get; set; }

    public string? DecisionAuditRecordId { get; set; }

    public string? CorrelationId { get; set; }

    public string? TraceId { get; set; }

    public string Status { get; set; } = "Pending";

    public int RetryCount { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? LastAttemptUtc { get; set; }

    public DateTime? NextAttemptUtc { get; set; }

    public DateTime? DeliveredUtc { get; set; }

    public string? LastErrorCode { get; set; }

    public string? LastErrorMessage { get; set; }
}