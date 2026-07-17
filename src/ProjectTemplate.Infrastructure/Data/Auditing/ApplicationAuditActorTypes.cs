namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Provides stable actor-type names for application audit records.
/// </summary>
public static class ApplicationAuditActorTypes
{
    public const string Human = "Human";

    public const string Service = "Service";

    public const string System = "System";

    public const string Network = "Network";

    public const string Unknown = "Unknown";
}
