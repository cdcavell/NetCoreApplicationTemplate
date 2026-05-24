using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the EF Core mapping for <see cref="ExternalLoginAccount" />.
/// </summary>
public sealed class ExternalLoginAccountConfiguration : IEntityTypeConfiguration<ExternalLoginAccount>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExternalLoginAccount> builder)
    {
        builder.ToTable("ExternalLoginAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.LocalUserId)
            .IsRequired();

        builder.Property(x => x.ProviderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .HasMaxLength(320);

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.ProviderName, x.ProviderUserId })
            .IsUnique();

        builder.HasIndex(x => x.LocalUserId);

        builder.HasIndex(x => x.Email);
    }
}
