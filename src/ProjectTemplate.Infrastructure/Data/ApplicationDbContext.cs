using Microsoft.EntityFrameworkCore;
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
    IApplicationAuditStore? auditStore = null,
    IApplicationSaveChangesPipeline? saveChangesPipeline = null
)
    : DbContext(options)
{
    private readonly ILogger<ApplicationDbContext> _logger = logger;
    private readonly IApplicationSaveChangesPipeline _saveChangesPipeline =
        saveChangesPipeline ?? new ApplicationSaveChangesPipeline(
            currentActorAccessor,
            dataAccessOptions,
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
        ApplicationSaveChangesPipelineState pipelineState =
            _saveChangesPipeline.ApplyBeforeSaveChanges(this);

        if (!pipelineState.HasChanges)
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        int result = SaveChangesWithConcurrencyHandling(
            () => base.SaveChanges(acceptAllChangesOnSuccess));

        if (pipelineState.HasPendingAuditEntries)
        {
            _saveChangesPipeline.ApplyAfterSaveChanges(this, pipelineState);
            _ = base.SaveChanges();
        }

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
        ApplicationSaveChangesPipelineState pipelineState =
            await _saveChangesPipeline
                .ApplyBeforeSaveChangesAsync(this, cancellationToken)
                .ConfigureAwait(false);

        if (!pipelineState.HasChanges)
        {
            return await base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken).ConfigureAwait(false);
        }

        int result = await SaveChangesWithConcurrencyHandlingAsync(
            () => base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken)).ConfigureAwait(false);

        if (pipelineState.HasPendingAuditEntries)
        {
            await _saveChangesPipeline
                .ApplyAfterSaveChangesAsync(this, pipelineState, cancellationToken)
                .ConfigureAwait(false);

            _ = await base
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    private static bool IsUtcTimestampProperty(string propertyName)
    {
        return propertyName.EndsWith("Utc", StringComparison.Ordinal);
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
}
