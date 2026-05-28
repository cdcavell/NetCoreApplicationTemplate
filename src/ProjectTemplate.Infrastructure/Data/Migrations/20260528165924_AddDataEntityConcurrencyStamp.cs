using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectTemplate.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddDataEntityConcurrencyStamp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ConcurrencyStamp",
            table: "ExternalLoginAccounts",
            type: "TEXT",
            maxLength: 64,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "ConcurrencyStamp",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 64,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ConcurrencyStamp",
            table: "ExternalLoginAccounts");

        migrationBuilder.DropColumn(
            name: "ConcurrencyStamp",
            table: "AuditRecords");
    }
}
