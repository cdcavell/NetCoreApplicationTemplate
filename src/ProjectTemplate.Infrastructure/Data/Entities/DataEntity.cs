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
}
