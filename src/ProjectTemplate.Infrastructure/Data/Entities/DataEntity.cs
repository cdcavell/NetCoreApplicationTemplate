namespace ProjectTemplate.Infrastructure.Data.Entities;

/// <summary>
/// Provides a common Guid primary key for ProjectTemplate data entities.
/// </summary>
public abstract class DataEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();


    /// <summary>
    /// Gets or sets the application-managed optimistic concurrency token used to detect conflicting updates.
    /// </summary>
    /// <remarks>
    /// This token is application-managed so the default template model remains portable across SQLite and SQL Server.
    /// SQL Server-only applications can replace this pattern with a provider-native rowversion column when appropriate.
    /// </remarks>
    public string ConcurrencyStamp { get; set; } = NewConcurrencyStamp();

    internal static string NewConcurrencyStamp()
    {
        return Guid.NewGuid().ToString("N");
    }
}
