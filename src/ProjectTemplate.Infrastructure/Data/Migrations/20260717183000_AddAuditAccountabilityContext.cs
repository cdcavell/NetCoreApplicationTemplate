using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectTemplate.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddAuditAccountabilityContext : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ActorId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 256,
            nullable: false,
            defaultValue: "Unknown");

        migrationBuilder.AddColumn<string>(
            name: "ActorType",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 64,
            nullable: false,
            defaultValue: "Unknown");

        migrationBuilder.AddColumn<string>(
            name: "CorrelationId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DecisionAuditRecordId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExecutionAttemptId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MutationBatchId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 64,
            nullable: false,
            defaultValue: "legacy");

        migrationBuilder.AddColumn<string>(
            name: "OperationExecutionId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OrganizationHash",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SchemaVersion",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 32,
            nullable: false,
            defaultValue: "1.0");

        migrationBuilder.AddColumn<string>(
            name: "SpanId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TenantHash",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TraceId",
            table: "AuditRecords",
            type: "TEXT",
            maxLength: 64,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_CorrelationId",
            table: "AuditRecords",
            column: "CorrelationId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_DecisionAuditRecordId",
            table: "AuditRecords",
            column: "DecisionAuditRecordId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_MutationBatchId",
            table: "AuditRecords",
            column: "MutationBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_OperationExecutionId",
            table: "AuditRecords",
            column: "OperationExecutionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AuditRecords_CorrelationId",
            table: "AuditRecords");

        migrationBuilder.DropIndex(
            name: "IX_AuditRecords_DecisionAuditRecordId",
            table: "AuditRecords");

        migrationBuilder.DropIndex(
            name: "IX_AuditRecords_MutationBatchId",
            table: "AuditRecords");

        migrationBuilder.DropIndex(
            name: "IX_AuditRecords_OperationExecutionId",
            table: "AuditRecords");

        migrationBuilder.DropColumn(name: "ActorId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "ActorType", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "CorrelationId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "DecisionAuditRecordId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "ExecutionAttemptId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "MutationBatchId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "OperationExecutionId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "OrganizationHash", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "SchemaVersion", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "SpanId", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "TenantHash", table: "AuditRecords");
        migrationBuilder.DropColumn(name: "TraceId", table: "AuditRecords");
    }
}
