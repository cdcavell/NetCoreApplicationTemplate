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
    /// Gets or sets the optimistic concurrency token used to detect conflicting updates.
    /// </summary>
    public string ConcurrencyStamp { get; set; } = NewConcurrencyStamp();

    internal static string NewConcurrencyStamp()
    {
        return Guid.NewGuid().ToString("N");
    }
}
