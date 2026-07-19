using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Entities;
using ProjectTemplate.Infrastructure.Data.Options;

namespace ProjectTemplate.Web.Tests;

public sealed class ApplicationAuditReconciliationTests
{
    private static readonly DateTime _now = new(2026, 7, 19, 17, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ReconcileAsync_CleanState_HasNoFindings()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        AuditRecord record = CreateAuditRecord("clean-batch");
        database.Context.AuditRecords.Add(record);
        database.Context.ApplicationAuditCompletionOutboxEntries.Add(CreateCompletion(record));
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        ApplicationAuditReconciliationSummary summary = await database.Reconciler.ReconcileAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(0, summary.OpenFindingCount);
        Assert.Empty(await database.Context.ApplicationAuditReconciliationFindings
            .ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ReconcileAsync_MissingCompletion_CreatesStableFinding()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        database.Context.AuditRecords.Add(CreateAuditRecord("missing-batch"));
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = await database.Reconciler.ReconcileAsync(TestContext.Current.CancellationToken);
        _ = await database.Reconciler.ReconcileAsync(TestContext.Current.CancellationToken);

        ApplicationAuditReconciliationFinding finding = Assert.Single(
            await database.Context.ApplicationAuditReconciliationFindings
                .AsNoTracking()
                .ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(ApplicationAuditReconciliationReasonCodes.MissingCompletion, finding.ReasonCode);
        Assert.Equal(ApplicationAuditReconciliationSeverities.Critical, finding.Severity);
    }

    [Fact]
    public async Task ReconcileAsync_CountMismatch_IsDetected()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        AuditRecord record = CreateAuditRecord("count-batch");
        ApplicationAuditCompletionOutboxEntry completion = CreateCompletion(record);
        completion.AuditRecordCount = 2;
        database.Context.AddRange(record, completion);
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = await database.Reconciler.ReconcileAsync(TestContext.Current.CancellationToken);

        Assert.Contains(
            await database.Context.ApplicationAuditReconciliationFindings
                .AsNoTracking()
                .ToListAsync(TestContext.Current.CancellationToken),
            finding => finding.ReasonCode == ApplicationAuditReconciliationReasonCodes.AuditRecordCountMismatch);
    }

    [Fact]
    public async Task ReconcileAsync_HashMismatch_IsDetected()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        AuditRecord record = CreateAuditRecord("hash-batch");
        ApplicationAuditCompletionOutboxEntry completion = CreateCompletion(record);
        completion.MutationManifestHash = new string('0', 64);
        database.Context.AddRange(record, completion);
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = await database.Reconciler.ReconcileAsync(TestContext.Current.CancellationToken);

        Assert.Contains(
            await database.Context.ApplicationAuditReconciliationFindings
                .AsNoTracking()
                .ToListAsync(TestContext.Current.CancellationToken),
            finding => finding.ReasonCode == ApplicationAuditReconciliationReasonCodes.ManifestVerificationFailed);
    }

    [Fact]
    public async Task ReconcileAsync_StalePending_IsVisible()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        AuditRecord record = CreateAuditRecord("stale-batch");
        ApplicationAuditCompletionOutboxEntry completion = CreateCompletion(record);
        completion.CreatedUtc = _now.AddHours(-1);
        completion.Status = ApplicationAuditCompletionOutboxStatuses.Pending;
        database.Context.AddRange(record, completion);
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        ApplicationAuditReconciliationSummary summary = await database.Reconciler.ReconcileAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(1, summary.StaleDeliveryCount);
        Assert.Contains(
            await database.Context.ApplicationAuditReconciliationFindings
                .AsNoTracking()
                .ToListAsync(TestContext.Current.CancellationToken),
            finding => finding.ReasonCode == ApplicationAuditReconciliationReasonCodes.StalePending);
    }

    [Fact]
    public async Task ReconcileAsync_DuplicateCompletion_IsDetected()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        await database.Context.Database.ExecuteSqlRawAsync(
            "DROP INDEX IX_ApplicationAuditCompletionOutbox_Destination_MutationBatchId",
            TestContext.Current.CancellationToken);
        AuditRecord record = CreateAuditRecord("duplicate-batch");
        ApplicationAuditCompletionOutboxEntry first = CreateCompletion(record);
        ApplicationAuditCompletionOutboxEntry second = CreateCompletion(record);
        second.Id = Guid.NewGuid();
        second.IdempotencyKey = $"duplicate-{Guid.NewGuid():N}";
        database.Context.AddRange(record, first, second);
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = await database.Reconciler.ReconcileAsync(TestContext.Current.CancellationToken);

        Assert.Contains(
            await database.Context.ApplicationAuditReconciliationFindings
                .AsNoTracking()
                .ToListAsync(TestContext.Current.CancellationToken),
            finding => finding.ReasonCode == ApplicationAuditReconciliationReasonCodes.DuplicateCompletion);
    }

    [Fact]
    public async Task RecordRemediationAsync_AppendsEvidenceAndResolvesFinding()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        database.Context.AuditRecords.Add(CreateAuditRecord("remediation-batch"));
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _ = await database.Reconciler.ReconcileAsync(TestContext.Current.CancellationToken);
        ApplicationAuditReconciliationFinding finding = await database.Context
            .ApplicationAuditReconciliationFindings
            .AsNoTracking()
            .SingleAsync(TestContext.Current.CancellationToken);

        ApplicationAuditReconciliationRemediationItem remediation = await database.Reconciler
            .RecordRemediationAsync(
                finding.Id,
                new("OperatorReviewed", "operator-1", "ticket-123", ResolveFinding: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(finding.Id, remediation.FindingId);
        Assert.Single(await database.Context.ApplicationAuditReconciliationRemediations
            .AsNoTracking()
            .ToListAsync(TestContext.Current.CancellationToken));
        ApplicationAuditReconciliationFinding resolved = await database.Context
            .ApplicationAuditReconciliationFindings
            .AsNoTracking()
            .SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ApplicationAuditReconciliationRemediationStatuses.Resolved, resolved.RemediationStatus);
        Assert.NotNull(resolved.ResolvedUtc);
    }

    [Fact]
    public async Task DisabledMode_DoesNotCreateFindings()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync(enabled: false);
        database.Context.AuditRecords.Add(CreateAuditRecord("disabled-batch"));
        _ = await database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        ApplicationAuditReconciliationSummary summary = await database.Reconciler.ReconcileAsync(
            TestContext.Current.CancellationToken);

        Assert.False(summary.Enabled);
        Assert.Empty(await database.Context.ApplicationAuditReconciliationFindings
            .ToListAsync(TestContext.Current.CancellationToken));
    }

    private static AuditRecord CreateAuditRecord(string batchId)
    {
        return new()
        {
            SchemaVersion = "1.0",
            ModifiedBy = "test",
            ActorId = "test",
            ActorType = "System",
            ModifiedOnUtc = _now.AddMinutes(-10),
            Application = "tests",
            Entity = "Example",
            State = "Modified",
            MutationBatchId = batchId,
            KeyValues = "{\"Id\":\"1\"}",
            OriginalValues = "{\"Value\":\"before\"}",
            CurrentValues = "{\"Value\":\"after\"}"
        };
    }

    private static ApplicationAuditCompletionOutboxEntry CreateCompletion(AuditRecord record)
    {
        var builder = new CanonicalApplicationMutationManifestBuilder();
        var hasher = new Sha256ApplicationMutationManifestHasher();
        ApplicationMutationManifest manifest = builder.Build([record]);
        return new()
        {
            Id = Guid.NewGuid(),
            SchemaVersion = ApplicationAuditCompletionOutboxEntry.CurrentSchemaVersion,
            Destination = "default",
            IdempotencyKey = $"completion-{Guid.NewGuid():N}",
            MutationBatchId = record.MutationBatchId,
            AuditRecordCount = 1,
            PersistenceOutcome = "Committed",
            ReceiptCompletedUtc = _now.AddMinutes(-9),
            MutationManifestHash = hasher.ComputeHash(manifest),
            MutationManifestAlgorithm = hasher.Algorithm,
            MutationManifestSchemaVersion = manifest.SchemaVersion,
            Status = ApplicationAuditCompletionOutboxStatuses.Delivered,
            CreatedUtc = _now.AddMinutes(-9),
            DeliveredUtc = _now.AddMinutes(-8)
        };
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(
            SqliteConnection connection,
            ApplicationDbContext context,
            ApplicationAuditReconciler reconciler,
            ApplicationAuditReconciliationMetrics metrics)
        {
            Connection = connection;
            Context = context;
            Reconciler = reconciler;
            Metrics = metrics;
        }

        public SqliteConnection Connection { get; }

        public ApplicationDbContext Context { get; }

        public ApplicationAuditReconciler Reconciler { get; }

        public ApplicationAuditReconciliationMetrics Metrics { get; }

        public static async Task<TestDatabase> CreateAsync(bool enabled = true)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            DbContextOptions<ApplicationDbContext> dbOptions =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;
            var pipeline = new ApplicationSaveChangesPipeline(
                new TestCurrentActorAccessor(),
                Microsoft.Extensions.Options.Options.Create(new DataAccessOptions
                {
                    Auditing = new DataAuditingOptions { Enabled = false }
                }));
            var context = new ApplicationDbContext(
                dbOptions,
                NullLogger<ApplicationDbContext>.Instance,
                pipeline);
            _ = await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
            var builder = new CanonicalApplicationMutationManifestBuilder();
            var hasher = new Sha256ApplicationMutationManifestHasher();
            var verifier = new ApplicationMutationManifestVerifier(builder, hasher);
            var metrics = new ApplicationAuditReconciliationMetrics();
            var reconciler = new ApplicationAuditReconciler(
                context,
                verifier,
                Microsoft.Extensions.Options.Options.Create(new ApplicationAuditReconciliationOptions
                {
                    Enabled = enabled,
                    CompletionGracePeriod = TimeSpan.Zero,
                    StalePendingThreshold = TimeSpan.FromMinutes(15),
                    StaleRetryReadyThreshold = TimeSpan.FromMinutes(15)
                }),
                metrics,
                new FixedTimeProvider(_now));
            return new(connection, context, reconciler, metrics);
        }

        public async ValueTask DisposeAsync()
        {
            Metrics.Dispose();
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        private readonly DateTimeOffset _utcNow = new(utcNow);

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    private sealed class TestCurrentActorAccessor : ICurrentActorAccessor
    {
        public string CurrentActor => "Audit Reconciliation Test Actor";
    }
}
