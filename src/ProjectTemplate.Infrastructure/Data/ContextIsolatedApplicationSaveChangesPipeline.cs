using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Options;

namespace ProjectTemplate.Infrastructure.Data;

/// <summary>
/// Routes save lifecycle callbacks to one mutable pipeline instance per application database context.
/// </summary>
internal sealed class ContextIsolatedApplicationSaveChangesPipeline :
    IApplicationSaveChangesPipeline,
    IApplicationMutationAuditReceiptRegistry
{
    private readonly ConditionalWeakTable<ApplicationDbContext, ApplicationSaveChangesPipeline> _pipelines = new();
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOptions<DataAccessOptions> _dataAccessOptions;
    private readonly IApplicationAuditStore? _auditStore;
    private readonly IApplicationAuditContextAccessor? _auditContextAccessor;
    private readonly IApplicationAuditValuePolicy? _auditValuePolicy;
    private readonly IApplicationMutationManifestBuilder _manifestBuilder;
    private readonly IApplicationMutationManifestHasher _manifestHasher;

    public ContextIsolatedApplicationSaveChangesPipeline(
        ICurrentActorAccessor currentActorAccessor,
        IOptions<DataAccessOptions> dataAccessOptions,
        IApplicationMutationManifestBuilder manifestBuilder,
        IApplicationMutationManifestHasher manifestHasher,
        IApplicationAuditStore? auditStore = null,
        IApplicationAuditContextAccessor? auditContextAccessor = null,
        IApplicationAuditValuePolicy? auditValuePolicy = null)
    {
        ArgumentNullException.ThrowIfNull(currentActorAccessor);
        ArgumentNullException.ThrowIfNull(dataAccessOptions);
        ArgumentNullException.ThrowIfNull(manifestBuilder);
        ArgumentNullException.ThrowIfNull(manifestHasher);

        _currentActorAccessor = currentActorAccessor;
        _dataAccessOptions = dataAccessOptions;
        _auditStore = auditStore;
        _auditContextAccessor = auditContextAccessor;
        _auditValuePolicy = auditValuePolicy;
        _manifestBuilder = manifestBuilder;
        _manifestHasher = manifestHasher;
    }

    public bool ApplyBeforeSaveChanges(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return GetPipeline(dbContext).ApplyBeforeSaveChanges(dbContext);
    }

    public ValueTask<bool> ApplyBeforeSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return GetPipeline(dbContext).ApplyBeforeSaveChangesAsync(dbContext, cancellationToken);
    }

    public bool ApplyAfterSaveChanges(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return GetPipeline(dbContext).ApplyAfterSaveChanges(dbContext);
    }

    public ValueTask<bool> ApplyAfterSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return GetPipeline(dbContext).ApplyAfterSaveChangesAsync(dbContext, cancellationToken);
    }

    public ApplicationMutationAuditReceipt? GetLastCompletedReceipt(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return _pipelines.TryGetValue(dbContext, out ApplicationSaveChangesPipeline? pipeline)
            ? pipeline.LastCompletedReceipt
            : null;
    }

    private ApplicationSaveChangesPipeline GetPipeline(ApplicationDbContext dbContext)
    {
        return _pipelines.GetValue(dbContext, _ => new ApplicationSaveChangesPipeline(
            _currentActorAccessor,
            _dataAccessOptions,
            _auditStore,
            _auditContextAccessor,
            _auditValuePolicy,
            _manifestBuilder,
            _manifestHasher));
    }
}
