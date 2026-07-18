using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Coordinates an application mutation, NCAT audit persistence, generated-value audit completion,
/// and an optional local completion handoff inside one relational database transaction boundary.
/// </summary>
public sealed class ApplicationAuditedTransaction : IApplicationAuditedTransaction
{
    private const string SavepointPrefix = "NCAT_";

    private readonly ApplicationDbContext _dbContext;
    private readonly IApplicationMutationAuditReceiptAccessor _receiptAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationAuditedTransaction" /> class.
    /// </summary>
    public ApplicationAuditedTransaction(
        ApplicationDbContext dbContext,
        IApplicationMutationAuditReceiptAccessor receiptAccessor)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(receiptAccessor);

        _dbContext = dbContext;
        _receiptAccessor = receiptAccessor;
    }

    /// <inheritdoc />
    public ApplicationAuditedTransactionResult Execute(
        Action<ApplicationDbContext> mutation,
        Action<ApplicationDbContext, ApplicationMutationAuditReceipt>? persistLocalCompletion = null,
        IsolationLevel? isolationLevel = null)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        EnsureSupportedTransactionEnvironment();
        EnsureCleanChangeTracker();

        IDbContextTransaction? existingTransaction = _dbContext.Database.CurrentTransaction;
        if (existingTransaction is not null)
        {
            return ExecuteWithinExistingTransaction(
                existingTransaction,
                mutation,
                persistLocalCompletion,
                isolationLevel);
        }

        IExecutionStrategy executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        ApplicationAuditedTransactionResult? result = null;

        executionStrategy.Execute(() =>
        {
            using IDbContextTransaction transaction = BeginTransaction(isolationLevel);
            try
            {
                result = ExecuteCore(
                    mutation,
                    persistLocalCompletion,
                    ApplicationAuditedTransactionOwnership.CoordinatorOwned,
                    usedSavepoint: false);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                ResetAfterRollback();
                throw;
            }
        });

        return result
            ?? throw new InvalidOperationException("The audited transaction execution strategy completed without a result.");
    }

    /// <inheritdoc />
    public async Task<ApplicationAuditedTransactionResult> ExecuteAsync(
        Func<ApplicationDbContext, CancellationToken, Task> mutation,
        Func<ApplicationDbContext, ApplicationMutationAuditReceipt, CancellationToken, Task>? persistLocalCompletion = null,
        IsolationLevel? isolationLevel = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureSupportedTransactionEnvironment();
        EnsureCleanChangeTracker();

        IDbContextTransaction? existingTransaction = _dbContext.Database.CurrentTransaction;
        if (existingTransaction is not null)
        {
            return await ExecuteWithinExistingTransactionAsync(
                    existingTransaction,
                    mutation,
                    persistLocalCompletion,
                    isolationLevel,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        IExecutionStrategy executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        ApplicationAuditedTransactionResult? result = null;

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await BeginTransactionAsync(
                    isolationLevel,
                    cancellationToken)
                .ConfigureAwait(false);

            try
            {
                result = await ExecuteCoreAsync(
                        mutation,
                        persistLocalCompletion,
                        ApplicationAuditedTransactionOwnership.CoordinatorOwned,
                        usedSavepoint: false,
                        cancellationToken)
                    .ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                ResetAfterRollback();
                throw;
            }
        }).ConfigureAwait(false);

        return result
            ?? throw new InvalidOperationException("The audited transaction execution strategy completed without a result.");
    }

    private ApplicationAuditedTransactionResult ExecuteWithinExistingTransaction(
        IDbContextTransaction existingTransaction,
        Action<ApplicationDbContext> mutation,
        Action<ApplicationDbContext, ApplicationMutationAuditReceipt>? persistLocalCompletion,
        IsolationLevel? isolationLevel)
    {
        EnsureExistingTransactionCanBeJoined(existingTransaction, isolationLevel);
        string savepointName = CreateSavepointName();
        existingTransaction.CreateSavepoint(savepointName);

        try
        {
            ApplicationAuditedTransactionResult result = ExecuteCore(
                mutation,
                persistLocalCompletion,
                ApplicationAuditedTransactionOwnership.ExistingTransaction,
                usedSavepoint: true);
            existingTransaction.ReleaseSavepoint(savepointName);
            return result;
        }
        catch
        {
            existingTransaction.RollbackToSavepoint(savepointName);
            ResetAfterRollback();
            throw;
        }
    }

    private async Task<ApplicationAuditedTransactionResult> ExecuteWithinExistingTransactionAsync(
        IDbContextTransaction existingTransaction,
        Func<ApplicationDbContext, CancellationToken, Task> mutation,
        Func<ApplicationDbContext, ApplicationMutationAuditReceipt, CancellationToken, Task>? persistLocalCompletion,
        IsolationLevel? isolationLevel,
        CancellationToken cancellationToken)
    {
        EnsureExistingTransactionCanBeJoined(existingTransaction, isolationLevel);
        string savepointName = CreateSavepointName();
        await existingTransaction.CreateSavepointAsync(savepointName, cancellationToken).ConfigureAwait(false);

        try
        {
            ApplicationAuditedTransactionResult result = await ExecuteCoreAsync(
                    mutation,
                    persistLocalCompletion,
                    ApplicationAuditedTransactionOwnership.ExistingTransaction,
                    usedSavepoint: true,
                    cancellationToken)
                .ConfigureAwait(false);
            await existingTransaction.ReleaseSavepointAsync(savepointName, CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await existingTransaction.RollbackToSavepointAsync(savepointName, CancellationToken.None).ConfigureAwait(false);
            ResetAfterRollback();
            throw;
        }
    }

    private ApplicationAuditedTransactionResult ExecuteCore(
        Action<ApplicationDbContext> mutation,
        Action<ApplicationDbContext, ApplicationMutationAuditReceipt>? persistLocalCompletion,
        ApplicationAuditedTransactionOwnership ownership,
        bool usedSavepoint)
    {
        ApplicationMutationAuditReceipt? previousReceipt = _receiptAccessor.LastCompletedReceipt;
        mutation(_dbContext);

        int mutationSaveChangesCount = _dbContext.SaveChanges();
        ApplicationMutationAuditReceipt? receipt = ResolveNewReceipt(previousReceipt);
        int completionSaveChangesCount = 0;

        if (persistLocalCompletion is not null)
        {
            if (receipt is null)
            {
                throw new InvalidOperationException(
                    "A local audit completion handoff was requested, but the mutation save produced no new audit receipt.");
            }

            persistLocalCompletion(_dbContext, receipt);
            completionSaveChangesCount = _dbContext.SaveChanges();
        }

        return new ApplicationAuditedTransactionResult(
            mutationSaveChangesCount,
            completionSaveChangesCount,
            receipt,
            ownership,
            usedSavepoint);
    }

    private async Task<ApplicationAuditedTransactionResult> ExecuteCoreAsync(
        Func<ApplicationDbContext, CancellationToken, Task> mutation,
        Func<ApplicationDbContext, ApplicationMutationAuditReceipt, CancellationToken, Task>? persistLocalCompletion,
        ApplicationAuditedTransactionOwnership ownership,
        bool usedSavepoint,
        CancellationToken cancellationToken)
    {
        ApplicationMutationAuditReceipt? previousReceipt = _receiptAccessor.LastCompletedReceipt;
        await mutation(_dbContext, cancellationToken).ConfigureAwait(false);

        int mutationSaveChangesCount = await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        ApplicationMutationAuditReceipt? receipt = ResolveNewReceipt(previousReceipt);
        int completionSaveChangesCount = 0;

        if (persistLocalCompletion is not null)
        {
            if (receipt is null)
            {
                throw new InvalidOperationException(
                    "A local audit completion handoff was requested, but the mutation save produced no new audit receipt.");
            }

            await persistLocalCompletion(_dbContext, receipt, cancellationToken).ConfigureAwait(false);
            completionSaveChangesCount = await _dbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new ApplicationAuditedTransactionResult(
            mutationSaveChangesCount,
            completionSaveChangesCount,
            receipt,
            ownership,
            usedSavepoint);
    }

    private ApplicationMutationAuditReceipt? ResolveNewReceipt(
        ApplicationMutationAuditReceipt? previousReceipt)
    {
        ApplicationMutationAuditReceipt? currentReceipt = _receiptAccessor.LastCompletedReceipt;
        return ReferenceEquals(previousReceipt, currentReceipt) ? null : currentReceipt;
    }

    private IDbContextTransaction BeginTransaction(IsolationLevel? isolationLevel)
    {
        return isolationLevel.HasValue
            ? _dbContext.Database.BeginTransaction(isolationLevel.Value)
            : _dbContext.Database.BeginTransaction();
    }

    private Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel? isolationLevel,
        CancellationToken cancellationToken)
    {
        return isolationLevel.HasValue
            ? _dbContext.Database.BeginTransactionAsync(isolationLevel.Value, cancellationToken)
            : _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    private void EnsureSupportedTransactionEnvironment()
    {
        if (!_dbContext.Database.IsRelational())
        {
            throw new NotSupportedException(
                "Application audited transactions require an EF Core relational database provider.");
        }

        if (System.Transactions.Transaction.Current is not null)
        {
            throw new NotSupportedException(
                "Ambient System.Transactions transactions are not supported. Use an explicit EF Core transaction so ownership and savepoint behavior remain visible.");
        }
    }

    private void EnsureCleanChangeTracker()
    {
        if (_dbContext.HasUnsavedChanges())
        {
            throw new InvalidOperationException(
                "The application database context already contains unsaved changes. Stage the audited mutation inside the coordinator delegate so transaction ownership and retry behavior remain explicit.");
        }
    }

    private void ResetAfterRollback()
    {
        _dbContext.ChangeTracker.Clear();
    }

    private static void EnsureExistingTransactionCanBeJoined(
        IDbContextTransaction existingTransaction,
        IsolationLevel? isolationLevel)
    {
        if (isolationLevel.HasValue)
        {
            throw new InvalidOperationException(
                "An isolation level cannot be supplied when the audited transaction coordinator joins an existing EF Core transaction.");
        }

        if (!existingTransaction.SupportsSavepoints)
        {
            throw new NotSupportedException(
                "The existing EF Core transaction does not support savepoints, so the audited transaction coordinator cannot safely join it.");
        }
    }

    private static string CreateSavepointName()
    {
        string suffix = Guid.NewGuid().ToString("N");
        return $"{SavepointPrefix}{suffix[..27]}";
    }
}
