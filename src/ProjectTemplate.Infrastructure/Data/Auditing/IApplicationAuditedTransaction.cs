using System.Data;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Coordinates one application mutation, its NCAT audit persistence, and an optional local completion handoff
/// inside a single relational database transaction boundary.
/// </summary>
public interface IApplicationAuditedTransaction
{
    /// <summary>
    /// Executes a synchronous mutation and optional local completion persistence operation.
    /// </summary>
    /// <param name="mutation">Stages the business mutation on the supplied application database context.</param>
    /// <param name="persistLocalCompletion">
    /// Optionally stages a database-local completion or outbox record after the mutation audit receipt is available.
    /// External delivery must not occur inside this callback.
    /// </param>
    /// <param name="isolationLevel">
    /// Optional isolation level for a coordinator-owned transaction. It cannot be supplied when joining an existing transaction.
    /// </param>
    /// <returns>The completed transaction result.</returns>
    ApplicationAuditedTransactionResult Execute(
        Action<ApplicationDbContext> mutation,
        Action<ApplicationDbContext, ApplicationMutationAuditReceipt>? persistLocalCompletion = null,
        IsolationLevel? isolationLevel = null);

    /// <summary>
    /// Executes an asynchronous mutation and optional local completion persistence operation.
    /// </summary>
    /// <param name="mutation">Stages the business mutation on the supplied application database context.</param>
    /// <param name="persistLocalCompletion">
    /// Optionally stages a database-local completion or outbox record after the mutation audit receipt is available.
    /// External delivery must not occur inside this callback.
    /// </param>
    /// <param name="isolationLevel">
    /// Optional isolation level for a coordinator-owned transaction. It cannot be supplied when joining an existing transaction.
    /// </param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    /// <returns>The completed transaction result.</returns>
    Task<ApplicationAuditedTransactionResult> ExecuteAsync(
        Func<ApplicationDbContext, CancellationToken, Task> mutation,
        Func<ApplicationDbContext, ApplicationMutationAuditReceipt, CancellationToken, Task>? persistLocalCompletion = null,
        IsolationLevel? isolationLevel = null,
        CancellationToken cancellationToken = default);
}
