using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddExternalLoginAccounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExternalLoginAccounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                LocalUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                ProviderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                ProviderUserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                LastLoginOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ExternalLoginAccounts", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ExternalLoginAccounts_Email",
            table: "ExternalLoginAccounts",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_ExternalLoginAccounts_LocalUserId",
            table: "ExternalLoginAccounts",
            column: "LocalUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ExternalLoginAccounts_ProviderName_ProviderUserId",
            table: "ExternalLoginAccounts",
            columns: ["ProviderName", "ProviderUserId"],
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExternalLoginAccounts");
    }
}
