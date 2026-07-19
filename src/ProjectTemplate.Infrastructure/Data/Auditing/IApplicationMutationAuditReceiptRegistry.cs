namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Resolves the most recently completed mutation audit receipt for a specific application database context.
/// </summary>
public interface IApplicationMutationAuditReceiptRegistry
{
    /// <summary>
    /// Gets the most recently completed receipt produced by <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">The application database context whose receipt should be resolved.</param>
    /// <returns>The context-specific receipt, or <see langword="null" /> when the context has not completed an audited save.</returns>
    ApplicationMutationAuditReceipt? GetLastCompletedReceipt(ApplicationDbContext dbContext);
}
