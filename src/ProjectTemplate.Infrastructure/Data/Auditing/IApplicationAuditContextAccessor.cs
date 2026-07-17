namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Provides the current host-owned application audit context.
/// </summary>
public interface IApplicationAuditContextAccessor
{
    ApplicationAuditContext Current { get; }
}
