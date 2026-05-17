using System.ComponentModel.DataAnnotations;

namespace Template.Infrastructure.Data.Entities;

/// <summary>
/// Provides a common Guid primary key for template data entities.
/// </summary>
public abstract class DataEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}
