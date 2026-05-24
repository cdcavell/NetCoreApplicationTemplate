using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the EF Core mapping for <see cref="AuditRecord"/>.
/// </summary>
public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("AuditRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ModifiedOnUtc)
            .IsRequired();

        builder.Property(x => x.Application)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Entity)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.State)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.KeyValues)
            .IsRequired();

        builder.Property(x => x.OriginalValues)
            .IsRequired();

        builder.Property(x => x.CurrentValues)
            .IsRequired();
    }
}
