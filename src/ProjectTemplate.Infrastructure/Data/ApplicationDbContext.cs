using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Entities;
using ProjectTemplate.Infrastructure.Data.Options;

namespace ProjectTemplate.Infrastructure.Data;

/// <summary>
/// Represents the EF Core database context for the ProjectTemplate application.
/// </summary>
public sealed partial class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ILogger<ApplicationDbContext> logger,
    ICurrentActorAccessor currentActorAccessor,
    IOptions<DataAccessOptions> dataAccessOptions,
    IApplicationAuditStore? auditStore = null
)
    : DbContext(options)
{
    private readonly ILogger<ApplicationDbContext> _logger = logger;
    private readonly ICurrentActorAccessor _currentActorAccessor = currentActorAccessor;
    private readonly bool _auditOptions = dataAccessOptions.Value.Auditing.Enabled;
    private readonly IApplicationAuditStore _auditStore = ResolveAuditStore(
        dataAccessOptions.Value.Auditing,
        auditStore);

    /// <summary>
    /// Gets the audit records for the application.
    /// </summary>
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
    /// <summary>
    /// Gets the external login account links for the application.
    /// </summary>
    public DbSet<ExternalLoginAccount> ExternalLoginAccounts => Set<ExternalLoginAccount>();

    [LoggerMessage(
        EventId = 19000,
        Level = LogLevel.Trace,
        Message = "{EfCoreMessage}")]
    private static partial void LogEfCoreMessage(
        ILogger logger,
        string efCoreMessage);

    [LoggerMessage(
        EventId = 19001,
        Level = LogLevel.Warning,
        Message = "Optimistic concurrency conflict detected while saving {EntryCount} tracked entity entries.")]
    private static partial void LogOptimisticConcurrencyConflict(
        ILogger logger,
        int entryCount,
        Exception exception);

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ConfigureDataEntityDefaults(modelBuilder);
        ConfigureTimestampDefaults(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        _ = optionsBuilder
            .LogTo(message => LogEfCoreMessage(_logger, message), LogLevel.Trace)
            .EnableDetailedErrors();

        base.OnConfiguring(optionsBuilder);
    }

    public bool HasUnsavedChanges()
    {
        return ChangeTracker.HasChanges();
    }

    public override int SaveChanges()
    {
        return SaveChanges(acceptAllChangesOnSuccess: true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess = true)
    {
        if (!ChangeTracker.HasChanges())
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        ApplyPersistedStringCanonicalization();
        ApplyLookupStringNormalization();
        ApplyTimestampNormalization();
        ApplyConcurrencyStamps();

        if (!_auditOptions)
        {
            return SaveChangesWithConcurrencyHandling(
                () => base.SaveChanges(acceptAllChangesOnSuccess));
        }

        List<AuditEntry> auditEntries = OnBeforeSaveChanges();

        int result = SaveChangesWithConcurrencyHandling(
            () => base.SaveChanges(acceptAllChangesOnSuccess));

        _ = OnAfterSaveChanges(auditEntries);

        return result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(
            acceptAllChangesOnSuccess: true,
            cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        if (!ChangeTracker.HasChanges())
        {
            return await base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken).ConfigureAwait(false);
        }

        ApplyPersistedStringCanonicalization();
        ApplyLookupStringNormalization();
        ApplyTimestampNormalization();
        ApplyConcurrencyStamps();

        if (!_auditOptions)
        {
            return await SaveChangesWithConcurrencyHandlingAsync(
                () => base.SaveChangesAsync(
                    acceptAllChangesOnSuccess,
                    cancellationToken)).ConfigureAwait(false);
        }

        List<AuditEntry> auditEntries = await OnBeforeSaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        int result = await SaveChangesWithConcurrencyHandlingAsync(
            () => base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken)).ConfigureAwait(false);

        _ = await OnAfterSaveChangesAsync(auditEntries, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    private void ApplyPersistedStringCanonicalization()
    {
        ChangeTracker.DetectChanges();

        foreach (EntityEntry entry in ChangeTracker.Entries())
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

    private void ApplyLookupStringNormalization()
    {
        ChangeTracker.DetectChanges();

        foreach (EntityEntry<ExternalLoginAccount> entry in ChangeTracker.Entries<ExternalLoginAccount>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            ExternalLoginAccount account = entry.Entity;

            account.ProviderName =
                PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(account.ProviderName);

            account.NormalizedProviderName =
                PersistenceStringComparisonNormalizer.NormalizeRequiredLookupValue(account.ProviderName);

            account.ProviderUserId =
                PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(account.ProviderUserId);

            account.DisplayName =
                PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(account.DisplayName);

            account.Email =
                PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(account.Email);

            account.NormalizedEmail =
                PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(account.Email);
        }
    }

    private void ApplyTimestampNormalization()
    {
        ChangeTracker.DetectChanges();

        foreach (EntityEntry entry in ChangeTracker.Entries())
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

    private static bool IsUtcTimestampProperty(string propertyName)
    {
        return propertyName.EndsWith("Utc", StringComparison.Ordinal);
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        List<AuditEntry> auditEntries = CreateAuditEntries();

        foreach (AuditEntry auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            _auditStore.Append(this, auditEntry.ToAuditRecord());
        }

        return [.. auditEntries.Where(_ => _.HasTemporaryProperties)];
    }

    private async Task<List<AuditEntry>> OnBeforeSaveChangesAsync(CancellationToken cancellationToken)
    {
        List<AuditEntry> auditEntries = CreateAuditEntries();

        foreach (AuditEntry auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            await _auditStore
                .AppendAsync(this, auditEntry.ToAuditRecord(), cancellationToken)
                .ConfigureAwait(false);
        }

        return [.. auditEntries.Where(_ => _.HasTemporaryProperties)];
    }

    private List<AuditEntry> CreateAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditRecord || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0002:Simplify Member Access",
        Justification = "The base call intentionally bypasses the overridden SaveChanges audit pipeline.")]
    private int OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
        {
            return 0;
        }

        foreach (AuditEntry auditEntry in auditEntries)
        {
            // Get the final value of the temporary properties
            foreach (PropertyEntry prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue ?? string.Empty;
                }
                else
                {
                    auditEntry.CurrentValues[prop.Metadata.Name] = prop.CurrentValue ?? string.Empty;
                }
            }

            // Save the audit entry.
            _auditStore.Append(this, auditEntry.ToAuditRecord());

        }

        return base.SaveChanges();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0002:Simplify Member Access",
        Justification = "The base call intentionally bypasses the overridden SaveChanges audit pipeline.")]
    private async Task<int> OnAfterSaveChangesAsync(
        List<AuditEntry> auditEntries,
        CancellationToken cancellationToken)
    {
        if (auditEntries == null || auditEntries.Count == 0)
        {
            return 0;
        }

        foreach (AuditEntry auditEntry in auditEntries)
        {
            // Get the final value of the temporary properties
            foreach (PropertyEntry prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue ?? string.Empty;
                }
                else
                {
                    auditEntry.CurrentValues[prop.Metadata.Name] = prop.CurrentValue ?? string.Empty;
                }
            }

            // Save the audit entry.
            await _auditStore
                .AppendAsync(this, auditEntry.ToAuditRecord(), cancellationToken)
                .ConfigureAwait(false);
        }

        return await base
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static void ConfigureDataEntityDefaults(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(DataEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            _ = modelBuilder.Entity(entityType.ClrType)
                .Property<string>(nameof(DataEntity.ConcurrencyStamp))
                .HasMaxLength(64)
                .IsRequired()
                .IsConcurrencyToken();
        }
    }

    private static void ConfigureTimestampDefaults(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(property.ClrType)
                    ?? property.ClrType;

                if ((propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset)) &&
                    IsUtcTimestampProperty(property.Name))
                {
                    property.SetPrecision(PersistenceTimestamp.Precision);
                }
            }
        }
    }

    private void ApplyConcurrencyStamps()
    {
        ChangeTracker.DetectChanges();

        foreach (EntityEntry<DataEntity> entry in ChangeTracker.Entries<DataEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrWhiteSpace(entry.Entity.ConcurrencyStamp))
                {
                    entry.Entity.ConcurrencyStamp = DataEntity.NewConcurrencyStamp();
                }

                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ConcurrencyStamp = DataEntity.NewConcurrencyStamp();
            }
        }
    }

    private int SaveChangesWithConcurrencyHandling(Func<int> saveChanges)
    {
        try
        {
            return saveChanges();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            LogOptimisticConcurrencyConflict(_logger, exception.Entries.Count, exception);
            throw;
        }
    }

    private async Task<int> SaveChangesWithConcurrencyHandlingAsync(Func<Task<int>> saveChanges)
    {
        try
        {
            return await saveChanges().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            LogOptimisticConcurrencyConflict(_logger, exception.Entries.Count, exception);
            throw;
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
