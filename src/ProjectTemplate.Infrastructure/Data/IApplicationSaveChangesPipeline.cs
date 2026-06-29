namespace ProjectTemplate.Infrastructure.Data;

/// <summary>
/// Defines the application-owned EF Core save preparation pipeline.
/// </summary>
public interface IApplicationSaveChangesPipeline
{
    /// <summary>
    /// Applies application save preparation before EF Core persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <returns>State that should be carried into the after-save pipeline.</returns>
    ApplicationSaveChangesPipelineState ApplyBeforeSaveChanges(
        ApplicationDbContext dbContext);

    /// <summary>
    /// Applies application save preparation before EF Core asynchronously persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>State that should be carried into the after-save pipeline.</returns>
    ValueTask<ApplicationSaveChangesPipelineState> ApplyBeforeSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes application save handling after EF Core persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="pipelineState">The state produced by the before-save pipeline.</param>
    void ApplyAfterSaveChanges(
        ApplicationDbContext dbContext,
        ApplicationSaveChangesPipelineState pipelineState);

    /// <summary>
    /// Completes application save handling after EF Core asynchronously persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="pipelineState">The state produced by the before-save pipeline.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>A task that completes when the after-save pipeline has finished.</returns>
    ValueTask ApplyAfterSaveChangesAsync(
        ApplicationDbContext dbContext,
        ApplicationSaveChangesPipelineState pipelineState,
        CancellationToken cancellationToken = default);
}
