using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Template.Infrastructure.Data.Entities;

namespace Template.Infrastructure.Data;

internal class AuditEntry(EntityEntry entry)
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
            Entity = TableName,
            State = State,
            Application = string.IsNullOrWhiteSpace(Application)
            ? applicationAssembly
            : Application,
            ModifiedBy = ModifiedBy,
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
