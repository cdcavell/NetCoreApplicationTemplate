namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Identifies who owns the database transaction used by an audited mutation.
/// </summary>
public enum ApplicationAuditedTransactionOwnership
{
    /// <summary>
    /// The coordinator created and committed the transaction.
    /// </summary>
    CoordinatorOwned,

    /// <summary>
    /// The coordinator joined an existing EF Core transaction and protected its work with a savepoint.
    /// The caller still owns the final commit or rollback.
    /// </summary>
    ExistingTransaction
}

/// <summary>
/// Summarizes an audited transaction execution without exposing audited entity values.
/// </summary>
/// <param name="MutationSaveChangesCount">The number of state entries written by the mutation save.</param>
/// <param name="CompletionSaveChangesCount">The number of state entries written by the optional local completion save.</param>
/// <param name="AuditReceipt">The mutation audit receipt produced by the mutation save, when auditing produced a batch.</param>
/// <param name="Ownership">The database transaction ownership model used for the execution.</param>
/// <param name="UsedSavepoint">Whether the coordinator created a savepoint inside an existing transaction.</param>
public sealed record ApplicationAuditedTransactionResult(
    int MutationSaveChangesCount,
    int CompletionSaveChangesCount,
    ApplicationMutationAuditReceipt? AuditReceipt,
    ApplicationAuditedTransactionOwnership Ownership,
    bool UsedSavepoint)
{
    /// <summary>
    /// Gets a value indicating whether the coordinator committed the database transaction before returning.
    /// </summary>
    public bool CommittedByCoordinator => Ownership == ApplicationAuditedTransactionOwnership.CoordinatorOwned;

    /// <summary>
    /// Gets a value indicating whether the caller must still commit the existing outer transaction.
    /// </summary>
    public bool RequiresOuterCommit => Ownership == ApplicationAuditedTransactionOwnership.ExistingTransaction;
}
