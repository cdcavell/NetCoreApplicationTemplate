using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProjectTemplate.Infrastructure.Data;

/// <summary>
/// Invokes the application save pipeline from the EF Core SaveChanges interception lifecycle.
/// </summary>
public sealed class ApplicationSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IApplicationSaveChangesPipeline _saveChangesPipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSaveChangesInterceptor" /> class.
    /// </summary>
    /// <param name="saveChangesPipeline">The application-owned save pipeline.</param>
    public ApplicationSaveChangesInterceptor(IApplicationSaveChangesPipeline saveChangesPipeline)
    {
        ArgumentNullException.ThrowIfNull(saveChangesPipeline);

        _saveChangesPipeline = saveChangesPipeline;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is ApplicationDbContext dbContext)
        {
            _ = _saveChangesPipeline.ApplyBeforeSaveChanges(dbContext);
        }

        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is ApplicationDbContext dbContext)
        {
            _ = await _saveChangesPipeline
                .ApplyBeforeSaveChangesAsync(dbContext, cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc />
    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is ApplicationDbContext dbContext &&
            _saveChangesPipeline.ApplyAfterSaveChanges(dbContext))
        {
            _ = dbContext.SaveChanges();
        }

        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is ApplicationDbContext dbContext &&
            await _saveChangesPipeline
                .ApplyAfterSaveChangesAsync(dbContext, cancellationToken)
                .ConfigureAwait(false))
        {
            _ = await dbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }
}
