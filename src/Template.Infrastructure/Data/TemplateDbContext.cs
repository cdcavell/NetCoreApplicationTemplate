using Microsoft.EntityFrameworkCore;
using Template.Infrastructure.Data.Entities;

namespace Template.Infrastructure.Data;

/// <summary>
/// Represents the EF Core database context for the template application.
/// </summary>
public sealed class TemplateDbContext(DbContextOptions<TemplateDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Gets the sample records used to validate the initial EF Core foundation.
    /// </summary>
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditRecord>(entity =>
        {
        });
    }
}
