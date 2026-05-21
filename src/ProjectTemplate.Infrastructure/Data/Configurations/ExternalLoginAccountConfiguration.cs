using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the EF Core mapping for <see cref="ExternalLoginAccount" />.
/// </summary>
public sealed class ExternalLoginAccountConfiguration : IEntityTypeConfiguration<ExternalLoginAccount>
{
    public void Configure(EntityTypeBuilder<ExternalLoginAccount> entity)
    {
        entity.ToTable("ExternalLoginAccounts");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedNever();

        entity.Property(x => x.LocalUserId)
            .IsRequired();

        entity.Property(x => x.ProviderName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired();

        entity.Property(x => x.DisplayName)
            .HasMaxLength(200);

        entity.Property(x => x.Email)
            .HasMaxLength(320);

        entity.Property(x => x.CreatedOnUtc)
            .IsRequired();

        entity.HasIndex(x => new { x.ProviderName, x.ProviderUserId })
            .IsUnique();

        entity.HasIndex(x => x.LocalUserId);

        entity.HasIndex(x => x.Email);
    }
}
