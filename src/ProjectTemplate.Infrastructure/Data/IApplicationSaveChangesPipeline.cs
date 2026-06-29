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
    /// <returns><see langword="true" /> when tracked changes should be persisted; otherwise, <see langword="false" />.</returns>
    bool ApplyBeforeSaveChanges(
        ApplicationDbContext dbContext);

    /// <summary>
    /// Applies application save preparation before EF Core asynchronously persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns><see langword="true" /> when tracked changes should be persisted; otherwise, <see langword="false" />.</returns>
    ValueTask<bool> ApplyBeforeSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes application save handling after EF Core persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <returns><see langword="true" /> when additional audit records were appended; otherwise, <see langword="false" />.</returns>
    bool ApplyAfterSaveChanges(
        ApplicationDbContext dbContext);

    /// <summary>
    /// Completes application save handling after EF Core asynchronously persists tracked changes.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns><see langword="true" /> when additional audit records were appended; otherwise, <see langword="false" />.</returns>
    ValueTask<bool> ApplyAfterSaveChangesAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default);
}
