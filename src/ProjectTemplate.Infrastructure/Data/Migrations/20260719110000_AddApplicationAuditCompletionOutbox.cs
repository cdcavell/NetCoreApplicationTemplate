using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectTemplate.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddApplicationAuditCompletionOutbox : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationAuditCompletionOutbox",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SchemaVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                Destination = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                MutationBatchId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                AuditRecordCount = table.Column<int>(type: "INTEGER", nullable: false),
                PersistenceOutcome = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ReceiptCompletedUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: false),
                MutationManifestHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                MutationManifestAlgorithm = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                MutationManifestSchemaVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                OperationExecutionId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                ExecutionAttemptId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                DecisionAuditRecordId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                CorrelationId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                TraceId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: false),
                LastAttemptUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: true),
                NextAttemptUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: true),
                DeliveredUtc = table.Column<DateTime>(type: "TEXT", precision: 3, nullable: true),
                LastErrorCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                LastErrorMessage = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationAuditCompletionOutbox", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditCompletionOutbox_CreatedUtc",
            table: "ApplicationAuditCompletionOutbox",
            column: "CreatedUtc");

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditCompletionOutbox_Destination_MutationBatchId",
            table: "ApplicationAuditCompletionOutbox",
            columns: new[] { "Destination", "MutationBatchId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditCompletionOutbox_IdempotencyKey",
            table: "ApplicationAuditCompletionOutbox",
            column: "IdempotencyKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationAuditCompletionOutbox_Status_NextAttemptUtc",
            table: "ApplicationAuditCompletionOutbox",
            columns: new[] { "Status", "NextAttemptUtc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApplicationAuditCompletionOutbox");
    }
}