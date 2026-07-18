using System.Text;
using System.Text.Json;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Builds deterministic version 1.0 mutation manifests using ordinal ordering rules.
/// </summary>
public sealed class CanonicalApplicationMutationManifestBuilder : IApplicationMutationManifestBuilder
{
    /// <inheritdoc />
    public ApplicationMutationManifest Build(IReadOnlyCollection<AuditRecord> auditRecords)
    {
        ArgumentNullException.ThrowIfNull(auditRecords);

        if (auditRecords.Count == 0)
        {
            throw new ArgumentException("At least one audit record is required to build a mutation manifest.", nameof(auditRecords));
        }

        string mutationBatchId = auditRecords.First().MutationBatchId;
        if (string.IsNullOrWhiteSpace(mutationBatchId))
        {
            throw new InvalidOperationException("Mutation audit records require a mutation batch identifier.");
        }

        if (auditRecords.Any(record => !string.Equals(record.MutationBatchId, mutationBatchId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("A canonical mutation manifest can represent only one mutation batch.");
        }

        var canonicalRecords = auditRecords
            .Select(CreateCanonicalRecord)
            .OrderBy(record => record.Entity, StringComparer.Ordinal)
            .ThenBy(record => record.State, StringComparer.Ordinal)
            .ThenBy(record => record.KeyValues, StringComparer.Ordinal)
            .ThenBy(record => record.OriginalValues, StringComparer.Ordinal)
            .ThenBy(record => record.CurrentValues, StringComparer.Ordinal)
            .ThenBy(record => record.OperationExecutionId, StringComparer.Ordinal)
            .ThenBy(record => record.ExecutionAttemptId, StringComparer.Ordinal)
            .ThenBy(record => record.DecisionAuditRecordId, StringComparer.Ordinal)
            .ThenBy(record => record.CorrelationId, StringComparer.Ordinal)
            .ThenBy(record => record.TraceId, StringComparer.Ordinal)
            .ThenBy(record => record.SpanId, StringComparer.Ordinal)
            .ThenBy(record => record.SchemaVersion, StringComparer.Ordinal)
            .ToList();

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            writer.WriteStartObject();
            writer.WriteString("schemaVersion", ApplicationMutationManifest.CurrentSchemaVersion);
            writer.WriteString("mutationBatchId", mutationBatchId);
            writer.WriteNumber("auditRecordCount", canonicalRecords.Count);
            writer.WritePropertyName("records");
            writer.WriteStartArray();

            foreach (CanonicalAuditRecord record in canonicalRecords)
            {
                WriteRecord(writer, record);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return new ApplicationMutationManifest(
            ApplicationMutationManifest.CurrentSchemaVersion,
            mutationBatchId,
            canonicalRecords.Count,
            Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static CanonicalAuditRecord CreateCanonicalRecord(AuditRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new CanonicalAuditRecord(
            record.SchemaVersion ?? string.Empty,
            record.Entity ?? string.Empty,
            record.State ?? string.Empty,
            CanonicalizeValueObject(record.KeyValues),
            CanonicalizeValueObject(record.OriginalValues),
            CanonicalizeValueObject(record.CurrentValues),
            record.OperationExecutionId ?? string.Empty,
            record.ExecutionAttemptId ?? string.Empty,
            record.DecisionAuditRecordId ?? string.Empty,
            record.CorrelationId ?? string.Empty,
            record.TraceId ?? string.Empty,
            record.SpanId ?? string.Empty);
    }

    private static string CanonicalizeValueObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Audit value payloads must be JSON objects.");
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteCanonicalElement(writer, document.RootElement);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteCanonicalElement(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (JsonProperty property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalElement(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (JsonElement item in element.EnumerateArray())
                {
                    WriteCanonicalElement(writer, item);
                }
                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                element.WriteTo(writer);
                break;
            case JsonValueKind.Undefined:
                throw new InvalidOperationException("Undefined JSON values cannot be represented in a canonical mutation manifest.");
            default:
                throw new InvalidOperationException($"Unsupported JSON value kind '{element.ValueKind}'.");
        }
    }

    private static void WriteRecord(Utf8JsonWriter writer, CanonicalAuditRecord record)
    {
        writer.WriteStartObject();
        writer.WriteString("schemaVersion", record.SchemaVersion);
        writer.WriteString("entity", record.Entity);
        writer.WriteString("state", record.State);
        WriteCanonicalObjectProperty(writer, "keyValues", record.KeyValues);
        WriteCanonicalObjectProperty(writer, "originalValues", record.OriginalValues);
        WriteCanonicalObjectProperty(writer, "currentValues", record.CurrentValues);
        writer.WriteString("operationExecutionId", record.OperationExecutionId);
        writer.WriteString("executionAttemptId", record.ExecutionAttemptId);
        writer.WriteString("decisionAuditRecordId", record.DecisionAuditRecordId);
        writer.WriteString("correlationId", record.CorrelationId);
        writer.WriteString("traceId", record.TraceId);
        writer.WriteString("spanId", record.SpanId);
        writer.WriteEndObject();
    }

    private static void WriteCanonicalObjectProperty(Utf8JsonWriter writer, string propertyName, string canonicalJson)
    {
        writer.WritePropertyName(propertyName);
        using var document = JsonDocument.Parse(canonicalJson);
        document.RootElement.WriteTo(writer);
    }

    private sealed record CanonicalAuditRecord(
        string SchemaVersion,
        string Entity,
        string State,
        string KeyValues,
        string OriginalValues,
        string CurrentValues,
        string OperationExecutionId,
        string ExecutionAttemptId,
        string DecisionAuditRecordId,
        string CorrelationId,
        string TraceId,
        string SpanId);
}
