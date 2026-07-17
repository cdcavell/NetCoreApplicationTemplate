using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data;

internal sealed class AuditEntry(EntityEntry entry)
{
    internal EntityEntry Entry { get; } = entry;
    [DataType(DataType.Text)]
    internal string TableName { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string State { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string Application { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string ModifiedBy { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string ActorId { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string ActorType { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string MutationBatchId { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    internal string? OperationExecutionId { get; set; }
    [DataType(DataType.Text)]
    internal string? ExecutionAttemptId { get; set; }
    [DataType(DataType.Text)]
    internal string? CorrelationId { get; set; }
    [DataType(DataType.Text)]
    internal string? TraceId { get; set; }
    [DataType(DataType.Text)]
    internal string? SpanId { get; set; }
    [DataType(DataType.Text)]
    internal string? DecisionAuditRecordId { get; set; }
    [DataType(DataType.Text)]
    internal string? TenantHash { get; set; }
    [DataType(DataType.Text)]
    internal string? OrganizationHash { get; set; }
    [DataType(DataType.DateTime)]
    internal DateTime ModifiedOnUtc { get; set; }
    internal Dictionary<string, object> KeyValues { get; } = [];
    internal Dictionary<string, object> OriginalValues { get; } = [];
    internal Dictionary<string, object> CurrentValues { get; } = [];
    internal List<PropertyEntry> TemporaryProperties { get; } = [];

    internal bool HasTemporaryProperties => TemporaryProperties.Count != 0;

    internal AuditRecord ToAuditRecord()
    {
        string applicationAssembly =
            Assembly.GetEntryAssembly()?.GetName().Name
            ?? GetType().Assembly.GetName().Name
            ?? "Unknown Assembly";

        AuditRecord auditRecord = new()
        {
            SchemaVersion = "1.0",
            Entity = TableName,
            State = State,
            Application = string.IsNullOrWhiteSpace(Application)
                ? applicationAssembly
                : Application,
            ModifiedBy = ModifiedBy,
            ActorId = ActorId,
            ActorType = ActorType,
            MutationBatchId = MutationBatchId,
            OperationExecutionId = OperationExecutionId,
            ExecutionAttemptId = ExecutionAttemptId,
            CorrelationId = CorrelationId,
            TraceId = TraceId,
            SpanId = SpanId,
            DecisionAuditRecordId = DecisionAuditRecordId,
            TenantHash = TenantHash,
            OrganizationHash = OrganizationHash,
            ModifiedOnUtc = ModifiedOnUtc,
            KeyValues = SerializeAuditValues(KeyValues),
            OriginalValues = SerializeAuditValues(OriginalValues),
            CurrentValues = SerializeAuditValues(CurrentValues)
        };
        return auditRecord;
    }

    private static string SerializeAuditValues(Dictionary<string, object> values)
    {
        return values.Count == 0
            ? string.Empty
            : JsonSerializer.Serialize(values);
    }
}
