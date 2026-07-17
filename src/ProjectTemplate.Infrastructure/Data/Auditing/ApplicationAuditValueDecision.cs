namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Describes how an audited property value should be represented.
/// </summary>
public sealed record ApplicationAuditValueDecision(
    ApplicationAuditValueDisposition Disposition,
    int? MaximumLength = null)
{
    public static ApplicationAuditValueDecision Include { get; } = new(ApplicationAuditValueDisposition.Include);
}
