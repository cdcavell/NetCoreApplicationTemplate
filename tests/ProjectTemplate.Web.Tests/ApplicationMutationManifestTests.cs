using System.Text.Json;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Web.Tests;

public sealed class ApplicationMutationManifestTests
{
    private readonly CanonicalApplicationMutationManifestBuilder _builder = new();
    private readonly Sha256ApplicationMutationManifestHasher _hasher = new();

    [Fact]
    public void Build_ReorderedRecordsAndFields_ProducesSameCanonicalOutputAndHash()
    {
        AuditRecord first = CreateRecord(
            entity: "Users",
            state: "Modified",
            keyValues: "{\"Id\":\"1\",\"Tenant\":\"A\"}",
            originalValues: "{\"Email\":\"***\",\"DisplayName\":\"Before\"}",
            currentValues: "{\"DisplayName\":\"After\",\"Email\":\"***\"}");
        AuditRecord second = CreateRecord(
            entity: "Accounts",
            state: "Added",
            keyValues: "{\"Id\":\"2\"}",
            originalValues: string.Empty,
            currentValues: "{\"ProviderUserId\":\"ABC123\",\"Email\":\"***\"}");

        ApplicationMutationManifest ordered = _builder.Build([first, second]);

        first.KeyValues = "{\"Tenant\":\"A\",\"Id\":\"1\"}";
        first.OriginalValues = "{\"DisplayName\":\"Before\",\"Email\":\"***\"}";
        second.CurrentValues = "{\"Email\":\"***\",\"ProviderUserId\":\"ABC123\"}";
        ApplicationMutationManifest reversed = _builder.Build([second, first]);

        Assert.Equal(ordered.CanonicalJson, reversed.CanonicalJson);
        Assert.Equal(_hasher.ComputeHash(ordered), _hasher.ComputeHash(reversed));
    }

    [Fact]
    public void Build_NullsAndOmittedFields_UsesStableEmptyRepresentations()
    {
        AuditRecord record = CreateRecord(
            entity: "Accounts",
            state: "Added",
            keyValues: string.Empty,
            originalValues: string.Empty,
            currentValues: "{\"Masked\":\"***\",\"Hashed\":\"ABC123\",\"Truncated\":\"Long\",\"NullValue\":\"\"}");
        record.OperationExecutionId = null;
        record.ExecutionAttemptId = null;

        ApplicationMutationManifest manifest = _builder.Build([record]);
        using JsonDocument document = JsonDocument.Parse(manifest.CanonicalJson);
        JsonElement item = document.RootElement.GetProperty("records")[0];

        Assert.Equal(ApplicationMutationManifest.CurrentSchemaVersion,
            document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("auditRecordCount").GetInt32());
        Assert.Equal(JsonValueKind.Object, item.GetProperty("keyValues").ValueKind);
        Assert.Empty(item.GetProperty("keyValues").EnumerateObject());
        Assert.Equal(string.Empty, item.GetProperty("operationExecutionId").GetString());
        Assert.False(item.GetProperty("currentValues").TryGetProperty("Omitted", out _));
    }

    [Fact]
    public void Verify_UnchangedBatchSucceeds_ModifiedBatchFails()
    {
        List<AuditRecord> records =
        [
            CreateRecord("Accounts", "Added", "{\"Id\":\"1\"}", string.Empty, "{\"Email\":\"***\"}"),
            CreateRecord("Users", "Modified", "{\"Id\":\"2\"}", "{\"Name\":\"A\"}", "{\"Name\":\"B\"}")
        ];
        ApplicationMutationManifest manifest = _builder.Build(records);
        var receipt = new ApplicationMutationAuditReceipt(
            manifest.MutationBatchId,
            manifest.AuditRecordCount,
            "Committed",
            DateTimeOffset.UtcNow,
            _hasher.ComputeHash(manifest),
            _hasher.Algorithm,
            manifest.SchemaVersion);
        var verifier = new ApplicationMutationManifestVerifier(_builder, _hasher);

        Assert.True(verifier.Verify(receipt, records));

        records[1].CurrentValues = "{\"Name\":\"Changed\"}";
        Assert.False(verifier.Verify(receipt, records));
    }

    [Fact]
    public void Build_VersionOneContract_HasStableRootPropertyOrder()
    {
        ApplicationMutationManifest manifest = _builder.Build(
        [
            CreateRecord("Accounts", "Added", "{\"Id\":\"1\"}", string.Empty, "{\"Email\":\"***\"}")
        ]);

        Assert.StartsWith(
            "{\"schemaVersion\":\"1.0\",\"mutationBatchId\":\"batch-367\",\"auditRecordCount\":1,\"records\":[",
            manifest.CanonicalJson,
            StringComparison.Ordinal);
    }

    private static AuditRecord CreateRecord(
        string entity,
        string state,
        string keyValues,
        string originalValues,
        string currentValues)
    {
        return new AuditRecord
        {
            SchemaVersion = "1.0",
            MutationBatchId = "batch-367",
            Entity = entity,
            State = state,
            KeyValues = keyValues,
            OriginalValues = originalValues,
            CurrentValues = currentValues,
            OperationExecutionId = "operation-367",
            ExecutionAttemptId = "attempt-1",
            DecisionAuditRecordId = "decision-367",
            CorrelationId = "correlation-367",
            TraceId = "0123456789abcdef0123456789abcdef",
            SpanId = "0123456789abcdef"
        };
    }
}
