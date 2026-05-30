using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectTemplate.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddExternalLoginAccountNormalizedLookupColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ExternalLoginAccounts_ProviderName_ProviderUserId",
            table: "ExternalLoginAccounts");

        migrationBuilder.AddColumn<string>(
            name: "NormalizedEmail",
            table: "ExternalLoginAccounts",
            type: "TEXT",
            maxLength: 320,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NormalizedProviderName",
            table: "ExternalLoginAccounts",
            type: "TEXT",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_ExternalLoginAccounts_NormalizedEmail",
            table: "ExternalLoginAccounts",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "IX_ExternalLoginAccounts_NormalizedProviderName_ProviderUserId",
            table: "ExternalLoginAccounts",
            columns: ["NormalizedProviderName", "ProviderUserId"],
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ExternalLoginAccounts_NormalizedEmail",
            table: "ExternalLoginAccounts");

        migrationBuilder.DropIndex(
            name: "IX_ExternalLoginAccounts_NormalizedProviderName_ProviderUserId",
            table: "ExternalLoginAccounts");

        migrationBuilder.DropColumn(
            name: "NormalizedEmail",
            table: "ExternalLoginAccounts");

        migrationBuilder.DropColumn(
            name: "NormalizedProviderName",
            table: "ExternalLoginAccounts");

        migrationBuilder.CreateIndex(
            name: "IX_ExternalLoginAccounts_ProviderName_ProviderUserId",
            table: "ExternalLoginAccounts",
            columns: ["ProviderName", "ProviderUserId"],
            unique: true);
    }
}
