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
public sealed class ApplicationSaveChangesPipeline : IApplicationSaveChangesPipeline
{
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly bool _auditOptions;
    private readonly IApplicationAuditStore _auditStore;
    private List<AuditEntry> _pendingAuditEntries = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSaveChangesPipeline" /> class.
    /// </summary>
    /// <param name="currentActorAccessor">The accessor for the actor responsible for the current save.</param>
    /// <param name="dataAccessOptions">The application data access options.</param>
    /// <param name="auditStore">The optional audit store used when auditing is enabled.</param>
    public ApplicationSaveChangesPipeline(
        ICurrentActorAccessor currentActorAccessor,
        IOptions<DataAccessOptions> dataAccessOptions,
        IApplicationAuditStore? auditStore = null)
    {
        ArgumentNullException.ThrowIfNull(currentActorAccessor);
        ArgumentNullException.ThrowIfNull(dataAccessOptions);

        _currentActorAccessor = currentActorAccessor;
        _auditOptions = dataAccessOptions.Value.Auditing.Enabled;
        _auditStore = ResolveAuditStore(
            dataAccessOptions.Value.Auditing,
            auditStore);
    }

    /// <inheritdoc />
    public bool ApplyBeforeSaveChanges(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _pendingAuditEntries = [];

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

        _pendingAuditEntries = [];

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

        if (_pendingAuditEntries.Count == 0)
        {
            return false;
        }

        foreach (AuditEntry auditEntry in _pendingAuditEntries)
        {
            CompleteTemporaryAuditProperties(auditEntry);
            _auditStore.Append(dbContext, auditEntry.ToAuditRecord());
        }

        _pendingAuditEntries = [];

        return true;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ApplyAfterSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        cancellationToken.ThrowIfCancellationRequested();

        if (_pendingAuditEntries.Count == 0)
        {
            return false;
        }

        foreach (AuditEntry auditEntry in _pendingAuditEntries)
        {
            CompleteTemporaryAuditProperties(auditEntry);
            await _auditStore
                .AppendAsync(dbContext, auditEntry.ToAuditRecord(), cancellationToken)
                .ConfigureAwait(false);
        }

        _pendingAuditEntries = [];

        return true;
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

        foreach (AuditEntry auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            _auditStore.Append(dbContext, auditEntry.ToAuditRecord());
        }

        return [.. auditEntries.Where(_ => _.HasTemporaryProperties)];
    }

    private async ValueTask<List<AuditEntry>> OnBeforeSaveChangesAsync(
        ApplicationDbContext dbContext,
        IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken)
    {
        List<AuditEntry> auditEntries = CreateAuditEntries(entries);

        foreach (AuditEntry auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            await _auditStore
                .AppendAsync(dbContext, auditEntry.ToAuditRecord(), cancellationToken)
                .ConfigureAwait(false);
        }

        return [.. auditEntries.Where(_ => _.HasTemporaryProperties)];
    }

    private List<AuditEntry> CreateAuditEntries(IEnumerable<EntityEntry> entries)
    {
        var auditEntries = new List<AuditEntry>();

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
                ModifiedBy = _currentActorAccessor.CurrentActor,
                ModifiedOnUtc = PersistenceTimestamp.UtcNow(),
                State = entry.State.ToString()
            };
            auditEntries.Add(auditEntry);

            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    // value will be generated by the database, get the value after saving
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue ?? string.Empty;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.CurrentValues[propertyName] = property.CurrentValue ?? string.Empty;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OriginalValues[propertyName] = property.OriginalValue ?? string.Empty;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OriginalValues[propertyName] = property.OriginalValue ?? string.Empty;
                            auditEntry.CurrentValues[propertyName] = property.CurrentValue ?? string.Empty;
                        }

                        break;

                    case EntityState.Detached:
                    case EntityState.Unchanged:
                    default:
                        break;
                }
            }
        }

        return auditEntries;
    }

    private static void SetPropertyCurrentValue(
        EntityEntry entry,
        string propertyName,
        object? value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }

    private static void CompleteTemporaryAuditProperties(AuditEntry auditEntry)
    {
        foreach (PropertyEntry property in auditEntry.TemporaryProperties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue ?? string.Empty;
            }
            else
            {
                auditEntry.CurrentValues[property.Metadata.Name] = property.CurrentValue ?? string.Empty;
            }
        }
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
