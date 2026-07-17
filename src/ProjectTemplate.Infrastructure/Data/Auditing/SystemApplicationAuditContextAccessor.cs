namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Supplies a system audit context for non-HTTP consumers.
/// </summary>
public sealed class SystemApplicationAuditContextAccessor : IApplicationAuditContextAccessor
{
    public ApplicationAuditContext Current => ApplicationAuditContext.System;
}
