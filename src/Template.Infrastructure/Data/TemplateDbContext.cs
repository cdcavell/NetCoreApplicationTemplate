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
    public DbSet<TemplateSampleRecord> TemplateSampleRecords => Set<TemplateSampleRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TemplateSampleRecord>(entity =>
        {
            entity.ToTable("TemplateSampleRecords");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .IsRequired();
        });
    }
}
