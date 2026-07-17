namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Summarizes a completed application mutation audit batch without exposing audited entity values.
/// </summary>
public sealed record ApplicationMutationAuditReceipt(
    string MutationBatchId,
    int AuditRecordCount,
    string PersistenceOutcome,
    DateTimeOffset CompletedUtc,
    string? OperationExecutionId = null,
    string? ExecutionAttemptId = null,
    string? DecisionAuditRecordId = null,
    string? CorrelationId = null,
    string? TraceId = null);
