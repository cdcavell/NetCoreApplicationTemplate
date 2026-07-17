namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Exposes the most recently completed mutation audit receipt for the current scoped save pipeline.
/// </summary>
public interface IApplicationMutationAuditReceiptAccessor
{
    ApplicationMutationAuditReceipt? LastCompletedReceipt { get; }
}
