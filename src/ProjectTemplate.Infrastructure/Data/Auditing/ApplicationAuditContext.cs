namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Carries host-owned identity and correlation data into application mutation audit records.
/// </summary>
public sealed record ApplicationAuditContext
{
    public static ApplicationAuditContext System { get; } = new(
        SystemCurrentActorAccessor.ActorName,
        ApplicationAuditActorTypes.System,
        SystemCurrentActorAccessor.ActorName);

    public ApplicationAuditContext(
        string actorId,
        string actorType,
        string actorDisplayName,
        string? operationExecutionId = null,
        string? executionAttemptId = null,
        string? correlationId = null,
        string? traceId = null,
        string? spanId = null,
        string? decisionAuditRecordId = null,
        string? tenantHash = null,
        string? organizationHash = null)
    {
        ActorId = NormalizeRequired(actorId, nameof(actorId));
        ActorType = NormalizeRequired(actorType, nameof(actorType));
        ActorDisplayName = NormalizeRequired(actorDisplayName, nameof(actorDisplayName));
        OperationExecutionId = NormalizeOptional(operationExecutionId);
        ExecutionAttemptId = NormalizeOptional(executionAttemptId);
        CorrelationId = NormalizeOptional(correlationId);
        TraceId = NormalizeOptional(traceId);
        SpanId = NormalizeOptional(spanId);
        DecisionAuditRecordId = NormalizeOptional(decisionAuditRecordId);
        TenantHash = NormalizeOptional(tenantHash);
        OrganizationHash = NormalizeOptional(organizationHash);
    }

    public string ActorId { get; }

    public string ActorType { get; }

    public string ActorDisplayName { get; }

    public string? OperationExecutionId { get; }

    public string? ExecutionAttemptId { get; }

    public string? CorrelationId { get; }

    public string? TraceId { get; }

    public string? SpanId { get; }

    public string? DecisionAuditRecordId { get; }

    public string? TenantHash { get; }

    public string? OrganizationHash { get; }

    private static string NormalizeRequired(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value must not be empty.", parameterName)
            : value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
