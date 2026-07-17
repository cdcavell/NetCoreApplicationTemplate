namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Defines how an application audit value should be represented.
/// </summary>
public enum ApplicationAuditValueDisposition
{
    Include = 0,
    Mask = 1,
    Hash = 2,
    Omit = 3,
    Truncate = 4
}
