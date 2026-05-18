using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Infrastructure.Data.Entities;

namespace Template.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the EF Core mapping for <see cref="AuditRecord"/>.
/// </summary>
public sealed class AuditRecordsConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditRecord> entity)
    {
        entity.ToTable("AuditRecords");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedNever();

        entity.Property(x => x.ModifiedBy)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.ModifiedOnUtc)
            .IsRequired();

        entity.Property(x => x.Application)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Entity)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.State)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.KeyValues)
            .IsRequired();

        entity.Property(x => x.OriginalValues)
            .IsRequired();

        entity.Property(x => x.CurrentValues)
            .IsRequired();
    }
}
