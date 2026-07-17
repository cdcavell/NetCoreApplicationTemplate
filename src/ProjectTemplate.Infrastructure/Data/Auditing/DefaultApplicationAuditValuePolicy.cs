namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Includes audited values unchanged. Applications should replace this policy when sensitive fields require minimization.
/// </summary>
public sealed class DefaultApplicationAuditValuePolicy : IApplicationAuditValuePolicy
{
    public ApplicationAuditValueDecision Evaluate(
        Type entityType,
        string propertyName,
        object? value)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        return ApplicationAuditValueDecision.Include;
    }
}
