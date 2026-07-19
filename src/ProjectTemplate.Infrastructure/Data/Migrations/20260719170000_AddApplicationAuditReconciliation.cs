using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectTemplate.Infrastructure.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260719170000_AddApplicationAuditReconciliation")]
public partial class AddApplicationAuditReconciliation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationAuditReconciliationFindings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SchemaVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                FindingKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                ReasonCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Severity = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                MutationBatchId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Destination = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                Guidance = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                RemediationStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                FirstObservedUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: false),
                LastObservedUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: false),
                ResolvedUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ApplicationAuditReconciliationFindings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ApplicationAuditReconciliationRemediations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                MutationBatchId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ActionCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ActorId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                EvidenceReference = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                RecordedUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ApplicationAuditReconciliationRemediations", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditReconciliationFindings_FindingKey",
            table: "ApplicationAuditReconciliationFindings",
            column: "FindingKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditReconciliationFindings_MutationBatchId_ReasonCode",
            table: "ApplicationAuditReconciliationFindings",
            columns: ["MutationBatchId", "ReasonCode"]);

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditReconciliationFindings_RemediationStatus_Severity",
            table: "ApplicationAuditReconciliationFindings",
            columns: ["RemediationStatus", "Severity"]);

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditReconciliationRemediations_FindingId",
            table: "ApplicationAuditReconciliationRemediations",
            column: "FindingId");

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditReconciliationRemediations_MutationBatchId",
            table: "ApplicationAuditReconciliationRemediations",
            column: "MutationBatchId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApplicationAuditReconciliationRemediations");
        migrationBuilder.DropTable(name: "ApplicationAuditReconciliationFindings");
    }
}
