namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Exposes the most recently completed mutation receipt for one application database context.
/// </summary>
internal sealed class ApplicationDbContextMutationAuditReceiptAccessor(
    ApplicationDbContext dbContext,
    IApplicationMutationAuditReceiptRegistry receiptRegistry)
    : IApplicationMutationAuditReceiptAccessor
{
    private readonly ApplicationDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IApplicationMutationAuditReceiptRegistry _receiptRegistry =
        receiptRegistry ?? throw new ArgumentNullException(nameof(receiptRegistry));

    public ApplicationMutationAuditReceipt? LastCompletedReceipt =>
        _receiptRegistry.GetLastCompletedReceipt(_dbContext);
}
