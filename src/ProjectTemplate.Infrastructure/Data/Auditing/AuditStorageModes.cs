namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Defines supported audit storage mode names for application data access.
/// </summary>
public static class AuditStorageModes
{
    /// <summary>
    /// Stores audit records in the application database through the local EF Core audit record table.
    /// </summary>
    public const string Local = "Local";

    /// <summary>
    /// Indicates that a consuming application intends to route audit records through an outbox-style store.
    /// </summary>
    public const string Outbox = "Outbox";

    /// <summary>
    /// Indicates that a consuming application intends to route audit records through a custom external sink.
    /// </summary>
    public const string ExternalSink = "ExternalSink";

    /// <summary>
    /// Determines whether the supplied storage mode is one of the supported mode names.
    /// </summary>
    public static bool IsSupported(string? storageMode)
    {
        string normalizedStorageMode = Normalize(storageMode);

        return normalizedStorageMode.Equals(Local, StringComparison.OrdinalIgnoreCase)
            || normalizedStorageMode.Equals(Outbox, StringComparison.OrdinalIgnoreCase)
            || normalizedStorageMode.Equals(ExternalSink, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the supplied storage mode uses the default local audit record table.
    /// </summary>
    public static bool IsLocal(string? storageMode)
    {
        return Normalize(storageMode).Equals(Local, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes a configured storage mode value for comparison and logging.
    /// </summary>
    public static string Normalize(string? storageMode)
    {
        return storageMode?.Trim() ?? string.Empty;
    }
}
