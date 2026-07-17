namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Determines how entity property values are represented in application audit records.
/// </summary>
public interface IApplicationAuditValuePolicy
{
    ApplicationAuditValueDecision Evaluate(
        Type entityType,
        string propertyName,
        object? value);
}
