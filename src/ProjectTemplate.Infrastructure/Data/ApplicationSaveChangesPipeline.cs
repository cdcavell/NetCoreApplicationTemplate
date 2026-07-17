using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Entities;
using ProjectTemplate.Infrastructure.Data.Options;

namespace ProjectTemplate.Infrastructure.Data;

/// <summary>
/// Applies application-owned mutation, normalization, concurrency, and audit preparation during EF Core saves.
/// </summary>
public sealed class ApplicationSaveChangesPipeline :
    IApplicationSaveChangesPipeline,
    IApplicationMutationAuditReceiptAccessor
{
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IApplicationAuditContextAccessor? _auditContextAccessor;
    private readonly IApplicationAuditValuePolicy _auditValuePolicy;
    private readonly bool _auditOptions;
    private readonly IApplicationAuditStore _auditStore;
    private List<AuditEntry> _pendingAuditEntries = [];
    private ApplicationAuditContext? _activeAuditContext;
    private string? _activeMutationBatchId;
    private int _activeAuditRecordCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSaveChangesPipeline" /> class.
    /// </summary>
    public ApplicationSaveChangesPipeline(
        ICurrentActorAccessor currentActorAccessor,
        IOptions<DataAccessOptions> dataAccessOptions,
        IApplicationAuditStore? auditStore = null,
        IApplicationAuditContextAccessor? auditContextAccessor = null,
        IApplicationAuditValuePolicy? auditValuePolicy = null)
    {
        ArgumentNullException.ThrowIfNull(currentActorAccessor);
        ArgumentNullException.ThrowIfNull(dataAccessOptions);

        _currentActorAccessor = currentActorAccessor;
        _auditContextAccessor = auditContextAccessor;
        _auditValuePolicy = auditValuePolicy ?? new DefaultApplicationAuditValuePolicy();
        _auditOptions = dataAccessOptions.Value.Auditing.Enabled;
        _auditStore = ResolveAuditStore(
            dataAccessOptions.Value.Auditing,
            auditStore);
    }

    /// <inheritdoc />
    public ApplicationMutationAuditReceipt? LastCompletedReceipt { get; private set; }

    /// <inheritdoc />
    public bool ApplyBeforeSaveChanges(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        ResetActiveAuditState();

        IReadOnlyList<EntityEntry> entries = GetSavePipelineEntries(dbContext);

        if (entries.Count == 0)
        {
            return false;
        }

        ApplyPersistedStringCanonicalization(entries);
        ApplyLookupStringNormalization(entries);
        ApplyTimestampNormalization(entries);
        ApplyConcurrencyStamps(entries);

        if (_auditOptions)
        {
            _pendingAuditEntries = OnBeforeSaveChanges(dbContext, entries);
        }

        return true;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ApplyBeforeSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        cancellationToken.ThrowIfCancellationRequested();

        ResetActiveAuditState();

        IReadOnlyList<EntityEntry> entries = GetSavePipelineEntries(dbContext);

        if (entries.Count == 0)
        {
            return false;
        }

        ApplyPersistedStringCanonicalization(entries);
        ApplyLookupStringNormalization(entries);
        ApplyTimestampNormalization(entries);
        ApplyConcurrencyStamps(entries);

        if (_auditOptions)
        {
            _pendingAuditEntries = await OnBeforeSaveChangesAsync(
                    dbContext,
                    entries,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }

    /// <inheritdoc />
    public bool ApplyAfterSaveChanges(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        bool appendedAdditionalAuditRecords = false;

        foreach (AuditEntry auditEntry in _pendingAuditEntries)
        {
            CompleteTemporaryAuditProperties(auditEntry);
            _auditStore.Append(dbContext, auditEntry.ToAuditRecord());
            appendedAdditionalAuditRecords = true;
        }

        CompleteMutationReceipt();
        _pendingAuditEntries = [];

        return appendedAdditionalAuditRecords;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ApplyAfterSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        cancellationToken.ThrowIfCancellationRequested();

        bool appendedAdditionalAuditRecords = false;

        foreach (AuditEntry auditEntry in _pendingAuditEntries)
        {
            CompleteTemporaryAuditProperties(auditEntry);
            await _auditStore
                .AppendAsync(dbContext, auditEntry.ToAuditRecord(), cancellationToken)
                .ConfigureAwait(false);
            appendedAdditionalAuditRecords = true;
        }

        CompleteMutationReceipt();
        _pendingAuditEntries = [];

        return appendedAdditionalAuditRecords;
    }

    private void ResetActiveAuditState()
    {
        _pendingAuditEntries = [];
        _activeAuditContext = null;
        _activeMutationBatchId = null;
        _activeAuditRecordCount = 0;
    }

    private static IReadOnlyList<EntityEntry> GetSavePipelineEntries(ApplicationDbContext dbContext)
    {
        dbContext.ChangeTracker.DetectChanges();

        return [.. dbContext.ChangeTracker
            .Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)];
    }

    private static void ApplyPersistedStringCanonicalization(IEnumerable<EntityEntry> entries)
    {
        foreach (EntityEntry entry in entries)
        {
            if (entry.Entity is AuditRecord ||
                entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(string) ||
                    property.Metadata.IsPrimaryKey() ||
                    property.Metadata.IsConcurrencyToken)
                {
                    continue;
                }

                if (entry.State == EntityState.Modified && !property.IsModified)
                {
                    continue;
                }

                if (property.CurrentValue is not string currentValue)
                {
                    continue;
                }

                string canonicalValue = PersistenceStringCanonicalizer.Canonicalize(currentValue);

                if (!string.Equals(canonicalValue, currentValue, StringComparison.Ordinal))
                {
                    property.CurrentValue = canonicalValue;
                }
            }
        }
    }

    private static void ApplyLookupStringNormalization(IEnumerable<EntityEntry> entries)
    {
        foreach (EntityEntry entry in entries)
        {
            if (entry.Entity is not ExternalLoginAccount account ||
                entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            SetPropertyCurrentValue(
                entry,
                nameof(ExternalLoginAccount.ProviderName),
                PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(account.ProviderName));
            SetPropertyCurrentValue(
                entry,
                nameof(ExternalLoginAccount.NormalizedProviderName),
                PersistenceStringComparisonNormalizer.NormalizeRequiredLookupValue(account.ProviderName));
            SetPropertyCurrentValue(
                entry,
                nameof(ExternalLoginAccount.ProviderUserId),
                PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(account.ProviderUserId));
            SetPropertyCurrentValue(
                entry,
                nameof(ExternalLoginAccount.DisplayName),
                PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(account.DisplayName));
            SetPropertyCurrentValue(
                entry,
                nameof(ExternalLoginAccount.Email),
                PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(account.Email));
            SetPropertyCurrentValue(
                entry,
                nameof(ExternalLoginAccount.NormalizedEmail),
                PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(account.Email));
        }
    }

    private static void ApplyTimestampNormalization(IEnumerable<EntityEntry> entries)
    {
        foreach (EntityEntry entry in entries)
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            foreach (PropertyEntry property in entry.Properties)
            {
                if (!IsUtcTimestampProperty(property.Metadata.Name))
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(property.Metadata.ClrType)
                    ?? property.Metadata.ClrType;

                if (propertyType == typeof(DateTime) &&
                    property.CurrentValue is DateTime dateTimeValue)
                {
                    property.CurrentValue = PersistenceTimestamp.NormalizeUtc(dateTimeValue);
                    continue;
                }

                if (propertyType == typeof(DateTimeOffset) &&
                    property.CurrentValue is DateTimeOffset dateTimeOffsetValue)
                {
                    property.CurrentValue = PersistenceTimestamp.NormalizeUtc(dateTimeOffsetValue);
                }
            }
        }
    }

    private static void ApplyConcurrencyStamps(IEnumerable<EntityEntry> entries)
    {
        foreach (EntityEntry entry in entries)
        {
            if (entry.Entity is not DataEntity entity)
            {
                continue;
            }

            PropertyEntry concurrencyStampProperty = entry.Property(nameof(DataEntity.ConcurrencyStamp));

            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrWhiteSpace(entity.ConcurrencyStamp))
                {
                    concurrencyStampProperty.CurrentValue = DataEntity.NewConcurrencyStamp();
                }

                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                concurrencyStampProperty.CurrentValue = DataEntity.NewConcurrencyStamp();
            }
        }
    }

    private static bool IsUtcTimestampProperty(string propertyName)
    {
        return propertyName.EndsWith("Utc", StringComparison.Ordinal);
    }

    private List<AuditEntry> OnBeforeSaveChanges(
        ApplicationDbContext dbContext,
        IEnumerable<EntityEntry> entries)
    {
        List<AuditEntry> auditEntries = CreateAuditEntries(entries);

        foreach (AuditEntry auditEntry in auditEntries.Where(entry => !entry.HasTemporaryProperties))
        {
            _auditStore.Append(dbContext, auditEntry.ToAuditRecord());
        }

        return [.. auditEntries.Where(entry => entry.HasTemporaryProperties)];
    }

    private async ValueTask<List<AuditEntry>> OnBeforeSaveChangesAsync(
        ApplicationDbContext dbContext,
        IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken)
    {
        List<AuditEntry> auditEntries = CreateAuditEntries(entries);

        foreach (AuditEntry auditEntry in auditEntries.Where(entry => !entry.HasTemporaryProperties))
        {
            await _auditStore
                .AppendAsync(dbContext, auditEntry.ToAuditRecord(), cancellationToken)
                .ConfigureAwait(false);
        }

        return [.. auditEntries.Where(entry => entry.HasTemporaryProperties)];
    }

    private List<AuditEntry> CreateAuditEntries(IEnumerable<EntityEntry> entries)
    {
        var auditEntries = new List<AuditEntry>();
        ApplicationAuditContext auditContext = ResolveAuditContext();
        string mutationBatchId = Guid.NewGuid().ToString("N");

        foreach (EntityEntry entry in entries)
        {
            if (entry.Entity is AuditRecord ||
                entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged)
            {
                continue;
            }

            AuditEntry auditEntry = new(entry)
            {
                TableName = entry.Metadata.GetTableName() ?? string.Empty,
                ModifiedBy = auditContext.ActorDisplayName,
                ActorId = auditContext.ActorId,
                ActorType = auditContext.ActorType,
                MutationBatchId = mutationBatchId,
                OperationExecutionId = auditContext.OperationExecutionId,
                ExecutionAttemptId = auditContext.ExecutionAttemptId,
                CorrelationId = auditContext.CorrelationId,
                TraceId = auditContext.TraceId,
                SpanId = auditContext.SpanId,
                DecisionAuditRecordId = auditContext.DecisionAuditRecordId,
                TenantHash = auditContext.TenantHash,
                OrganizationHash = auditContext.OrganizationHash,
                ModifiedOnUtc = PersistenceTimestamp.UtcNow(),
                State = entry.State.ToString()
            };
            auditEntries.Add(auditEntry);

            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    AddProtectedValue(auditEntry.KeyValues, entry, propertyName, property.CurrentValue);
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        AddProtectedValue(auditEntry.CurrentValues, entry, propertyName, property.CurrentValue);
                        break;
                    case EntityState.Deleted:
                        AddProtectedValue(auditEntry.OriginalValues, entry, propertyName, property.OriginalValue);
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            AddProtectedValue(auditEntry.OriginalValues, entry, propertyName, property.OriginalValue);
                            AddProtectedValue(auditEntry.CurrentValues, entry, propertyName, property.CurrentValue);
                        }
                        break;
                    case EntityState.Detached:
                    case EntityState.Unchanged:
                    default:
                        break;
                }
            }
        }

        if (auditEntries.Count > 0)
        {
            _activeAuditContext = auditContext;
            _activeMutationBatchId = mutationBatchId;
            _activeAuditRecordCount = auditEntries.Count;
        }

        return auditEntries;
    }

    private ApplicationAuditContext ResolveAuditContext()
    {
        if (_auditContextAccessor is not null)
        {
            return _auditContextAccessor.Current
                ?? throw new InvalidOperationException("The application audit context accessor returned no context.");
        }

        string displayActor = _currentActorAccessor.CurrentActor;
        string actorId = string.IsNullOrWhiteSpace(displayActor) ? "Unknown" : displayActor.Trim();

        return new ApplicationAuditContext(
            actorId,
            actorId.Equals(SystemCurrentActorAccessor.ActorName, StringComparison.Ordinal)
                ? ApplicationAuditActorTypes.System
                : ApplicationAuditActorTypes.Unknown,
            actorId);
    }

    private void AddProtectedValue(
        Dictionary<string, object> target,
        EntityEntry entry,
        string propertyName,
        object? value)
    {
        if (ApplicationAuditValueProtector.TryProtect(
            _auditValuePolicy,
            entry.Entity.GetType(),
            propertyName,
            value,
            out object protectedValue))
        {
            target[propertyName] = protectedValue;
        }
    }

    private static void SetPropertyCurrentValue(
        EntityEntry entry,
        string propertyName,
        object? value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }

    private void CompleteTemporaryAuditProperties(AuditEntry auditEntry)
    {
        foreach (PropertyEntry property in auditEntry.TemporaryProperties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                AddProtectedValue(
                    auditEntry.KeyValues,
                    auditEntry.Entry,
                    property.Metadata.Name,
                    property.CurrentValue);
            }
            else
            {
                AddProtectedValue(
                    auditEntry.CurrentValues,
                    auditEntry.Entry,
                    property.Metadata.Name,
                    property.CurrentValue);
            }
        }
    }

    private void CompleteMutationReceipt()
    {
        if (_activeAuditContext is null ||
            string.IsNullOrWhiteSpace(_activeMutationBatchId) ||
            _activeAuditRecordCount == 0)
        {
            return;
        }

        LastCompletedReceipt = new ApplicationMutationAuditReceipt(
            _activeMutationBatchId,
            _activeAuditRecordCount,
            "Committed",
            DateTimeOffset.UtcNow,
            _activeAuditContext.OperationExecutionId,
            _activeAuditContext.ExecutionAttemptId,
            _activeAuditContext.DecisionAuditRecordId,
            _activeAuditContext.CorrelationId,
            _activeAuditContext.TraceId);

        _activeAuditContext = null;
        _activeMutationBatchId = null;
        _activeAuditRecordCount = 0;
    }

    private static IApplicationAuditStore ResolveAuditStore(
        DataAuditingOptions auditingOptions,
        IApplicationAuditStore? auditStore)
    {
        ArgumentNullException.ThrowIfNull(auditingOptions);

        return auditStore is not null
            ? auditStore
            : !auditingOptions.Enabled || AuditStorageModes.IsLocal(auditingOptions.StorageMode)
                ? (IApplicationAuditStore)new LocalApplicationAuditStore()
                : throw new InvalidOperationException(
                    $"Application audit storage mode '{AuditStorageModes.Normalize(auditingOptions.StorageMode)}' requires an {nameof(IApplicationAuditStore)} registration.");
    }
}
