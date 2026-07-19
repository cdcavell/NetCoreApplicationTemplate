using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Implements durable audit-completion staging, dispatch, and minimized operational queries.
/// </summary>
public sealed class ApplicationAuditCompletionOutbox :
    IApplicationAuditCompletionOutbox,
    IApplicationAuditCompletionOutboxDispatcher,
    IApplicationAuditCompletionOutboxQuery
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ApplicationAuditCompletionOutboxOptions _options;
    private readonly IReadOnlyDictionary<string, IApplicationAuditCompletionPublisher> _publishers;
    private readonly TimeProvider _timeProvider;

    public ApplicationAuditCompletionOutbox(
        ApplicationDbContext dbContext,
        IOptions<ApplicationAuditCompletionOutboxOptions> options,
        IEnumerable<IApplicationAuditCompletionPublisher> publishers,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(publishers);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _options = options.Value;
        _timeProvider = timeProvider;

        var publisherMap = new Dictionary<string, IApplicationAuditCompletionPublisher>(StringComparer.OrdinalIgnoreCase);
        foreach (IApplicationAuditCompletionPublisher publisher in publishers)
        {
            string destination = NormalizeDestination(publisher.Destination);
            if (!publisherMap.TryAdd(destination, publisher))
            {
                throw new InvalidOperationException(
                    $"More than one audit-completion publisher is registered for destination '{destination}'.");
            }
        }

        _publishers = publisherMap;
    }

    /// <inheritdoc />
    public ApplicationAuditCompletionOutboxEntry? Stage(
        ApplicationDbContext dbContext,
        ApplicationMutationAuditReceipt receipt,
        string? destination = null)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(receipt);

        if (!_options.Enabled)
        {
            return null;
        }

        string resolvedDestination = ResolveDestination(destination);
        OutboxIdentity identity = CreateIdentity(resolvedDestination, receipt.MutationBatchId);

        ApplicationAuditCompletionOutboxEntry? existing = dbContext
            .ApplicationAuditCompletionOutboxEntries
            .Local
            .SingleOrDefault(entry => entry.Id == identity.Id)
            ?? dbContext.ApplicationAuditCompletionOutboxEntries.Find(identity.Id);

        return existing is not null
            ? ValidateExisting(existing, resolvedDestination, receipt.MutationBatchId, identity.IdempotencyKey)
            : AddEntry(dbContext, receipt, resolvedDestination, identity);
    }

    /// <inheritdoc />
    public async ValueTask<ApplicationAuditCompletionOutboxEntry?> StageAsync(
        ApplicationDbContext dbContext,
        ApplicationMutationAuditReceipt receipt,
        string? destination = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(receipt);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Enabled)
        {
            return null;
        }

        string resolvedDestination = ResolveDestination(destination);
        OutboxIdentity identity = CreateIdentity(resolvedDestination, receipt.MutationBatchId);

        ApplicationAuditCompletionOutboxEntry? existing = dbContext
            .ApplicationAuditCompletionOutboxEntries
            .Local
            .SingleOrDefault(entry => entry.Id == identity.Id);

        existing ??= await dbContext.ApplicationAuditCompletionOutboxEntries
            .FindAsync([identity.Id], cancellationToken)
            .ConfigureAwait(false);

        return existing is not null
            ? ValidateExisting(existing, resolvedDestination, receipt.MutationBatchId, identity.IdempotencyKey)
            : AddEntry(dbContext, receipt, resolvedDestination, identity);
    }

    /// <inheritdoc />
    public async Task<ApplicationAuditCompletionDispatchSummary> DispatchReadyAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_options.Enabled)
        {
            return new(false, 0, 0, 0, 0, 0, 0);
        }

        DateTime now = UtcNow();
        List<ApplicationAuditCompletionOutboxEntry> entries = await _dbContext
            .ApplicationAuditCompletionOutboxEntries
            .Where(entry =>
                (entry.Status == ApplicationAuditCompletionOutboxStatuses.Pending ||
                 entry.Status == ApplicationAuditCompletionOutboxStatuses.RetryableFailure ||
                 entry.Status == ApplicationAuditCompletionOutboxStatuses.Deferred) &&
                (!entry.NextAttemptUtc.HasValue || entry.NextAttemptUtc <= now))
            .OrderBy(entry => entry.CreatedUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        int delivered = 0;
        int retryScheduled = 0;
        int failed = 0;
        int deferred = 0;
        int deadLettered = 0;

        foreach (ApplicationAuditCompletionOutboxEntry entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ApplicationAuditCompletionPublishResult result;

            if (!_publishers.TryGetValue(entry.Destination, out IApplicationAuditCompletionPublisher? publisher))
            {
                result = ApplicationAuditCompletionPublishResult.Defer(
                    "PublisherUnavailable",
                    "No audit-completion publisher is registered for the configured destination.",
                    _options.DeferredRetryDelay);
            }
            else
            {
                try
                {
                    result = await publisher
                        .PublishAsync(ToMessage(entry), cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    result = ApplicationAuditCompletionPublishResult.Retry(
                        exception.GetType().Name,
                        exception.Message);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            ApplyResult(entry, result, now);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            switch (entry.Status)
            {
                case ApplicationAuditCompletionOutboxStatuses.Delivered:
                    delivered++;
                    break;
                case ApplicationAuditCompletionOutboxStatuses.RetryableFailure:
                    retryScheduled++;
                    break;
                case ApplicationAuditCompletionOutboxStatuses.Failed:
                    failed++;
                    break;
                case ApplicationAuditCompletionOutboxStatuses.Deferred:
                    deferred++;
                    break;
                case ApplicationAuditCompletionOutboxStatuses.DeadLettered:
                    deadLettered++;
                    break;
            }
        }

        return new(
            true,
            entries.Count,
            delivered,
            retryScheduled,
            failed,
            deferred,
            deadLettered);
    }

    /// <inheritdoc />
    public async Task<ApplicationAuditCompletionOutboxHealth> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_options.Enabled)
        {
            return new(false, 0, null, 0, 0);
        }

        IQueryable<ApplicationAuditCompletionOutboxEntry> backlog = _dbContext
            .ApplicationAuditCompletionOutboxEntries
            .AsNoTracking()
            .Where(entry =>
                entry.Status == ApplicationAuditCompletionOutboxStatuses.Pending ||
                entry.Status == ApplicationAuditCompletionOutboxStatuses.RetryableFailure ||
                entry.Status == ApplicationAuditCompletionOutboxStatuses.Deferred);

        long backlogCount = await backlog.LongCountAsync(cancellationToken).ConfigureAwait(false);
        DateTime? oldestCreatedUtc = await backlog
            .MinAsync(entry => (DateTime?)entry.CreatedUtc, cancellationToken)
            .ConfigureAwait(false);
        long retryCount = await _dbContext.ApplicationAuditCompletionOutboxEntries
            .AsNoTracking()
            .SumAsync(entry => (long)entry.RetryCount, cancellationToken)
            .ConfigureAwait(false);
        long deadLetterCount = await _dbContext.ApplicationAuditCompletionOutboxEntries
            .AsNoTracking()
            .LongCountAsync(
                entry => entry.Status == ApplicationAuditCompletionOutboxStatuses.DeadLettered,
                cancellationToken)
            .ConfigureAwait(false);

        TimeSpan? oldestAge = oldestCreatedUtc.HasValue
            ? UtcNow() - oldestCreatedUtc.Value
            : null;

        return new(true, backlogCount, oldestAge, retryCount, deadLetterCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationAuditCompletionOutboxItem>> QueryAsync(
        ApplicationAuditCompletionOutboxQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Enabled)
        {
            return [];
        }

        int maximumResults = Math.Clamp(request.MaximumResults, 1, 500);
        IQueryable<ApplicationAuditCompletionOutboxEntry> query = _dbContext
            .ApplicationAuditCompletionOutboxEntries
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            string status = request.Status.Trim();
            query = query.Where(entry => entry.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            string destination = request.Destination.Trim();
            query = query.Where(entry => entry.Destination == destination);
        }

        if (!string.IsNullOrWhiteSpace(request.MutationBatchId))
        {
            string mutationBatchId = request.MutationBatchId.Trim();
            query = query.Where(entry => entry.MutationBatchId == mutationBatchId);
        }

        List<ApplicationAuditCompletionOutboxEntry> entries = await query
            .OrderByDescending(entry => entry.CreatedUtc)
            .Take(maximumResults)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return entries.Select(ToItem).ToArray();
    }

    private ApplicationAuditCompletionOutboxEntry AddEntry(
        ApplicationDbContext dbContext,
        ApplicationMutationAuditReceipt receipt,
        string destination,
        OutboxIdentity identity)
    {
        var entry = new ApplicationAuditCompletionOutboxEntry
        {
            Id = identity.Id,
            SchemaVersion = ApplicationAuditCompletionOutboxEntry.CurrentSchemaVersion,
            Destination = destination,
            IdempotencyKey = identity.IdempotencyKey,
            MutationBatchId = receipt.MutationBatchId,
            AuditRecordCount = receipt.AuditRecordCount,
            PersistenceOutcome = receipt.PersistenceOutcome,
            ReceiptCompletedUtc = receipt.CompletedUtc.UtcDateTime,
            MutationManifestHash = receipt.MutationManifestHash,
            MutationManifestAlgorithm = receipt.MutationManifestAlgorithm,
            MutationManifestSchemaVersion = receipt.MutationManifestSchemaVersion,
            OperationExecutionId = receipt.OperationExecutionId,
            ExecutionAttemptId = receipt.ExecutionAttemptId,
            DecisionAuditRecordId = receipt.DecisionAuditRecordId,
            CorrelationId = receipt.CorrelationId,
            TraceId = receipt.TraceId,
            Status = ApplicationAuditCompletionOutboxStatuses.Pending,
            CreatedUtc = UtcNow()
        };

        dbContext.ApplicationAuditCompletionOutboxEntries.Add(entry);
        return entry;
    }

    private void ApplyResult(
        ApplicationAuditCompletionOutboxEntry entry,
        ApplicationAuditCompletionPublishResult result,
        DateTime attemptedUtc)
    {
        entry.LastAttemptUtc = attemptedUtc;
        entry.DeliveredUtc = null;
        entry.LastErrorCode = SanitizeError(result.ErrorCode, 128);
        entry.LastErrorMessage = SanitizeError(result.ErrorMessage, _options.MaxErrorDetailLength);
        entry.ConcurrencyStamp = DataEntity.NewConcurrencyStamp();

        switch (result.Disposition)
        {
            case ApplicationAuditCompletionPublishDisposition.Delivered:
                entry.Status = ApplicationAuditCompletionOutboxStatuses.Delivered;
                entry.DeliveredUtc = attemptedUtc;
                entry.NextAttemptUtc = null;
                entry.LastErrorCode = null;
                entry.LastErrorMessage = null;
                break;
            case ApplicationAuditCompletionPublishDisposition.RetryableFailure:
                entry.RetryCount++;
                if (entry.RetryCount >= _options.MaxRetryAttempts)
                {
                    entry.Status = ApplicationAuditCompletionOutboxStatuses.DeadLettered;
                    entry.NextAttemptUtc = null;
                }
                else
                {
                    entry.Status = ApplicationAuditCompletionOutboxStatuses.RetryableFailure;
                    entry.NextAttemptUtc = attemptedUtc +
                        (result.RetryAfter ?? CalculateRetryDelay(entry.RetryCount));
                }

                break;
            case ApplicationAuditCompletionPublishDisposition.Failed:
                entry.RetryCount++;
                entry.Status = ApplicationAuditCompletionOutboxStatuses.Failed;
                entry.NextAttemptUtc = null;
                break;
            case ApplicationAuditCompletionPublishDisposition.Deferred:
                entry.Status = ApplicationAuditCompletionOutboxStatuses.Deferred;
                entry.NextAttemptUtc = attemptedUtc +
                    (result.RetryAfter ?? _options.DeferredRetryDelay);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported audit-completion publish disposition '{result.Disposition}'.");
        }
    }

    private TimeSpan CalculateRetryDelay(int retryCount)
    {
        int exponent = Math.Clamp(retryCount - 1, 0, 20);
        double ticks = Math.Min(
            _options.MaxRetryDelay.Ticks,
            _options.BaseRetryDelay.Ticks * Math.Pow(2, exponent));
        return TimeSpan.FromTicks((long)ticks);
    }

    private string ResolveDestination(string? destination)
    {
        return NormalizeDestination(
            string.IsNullOrWhiteSpace(destination)
                ? _options.DefaultDestination
                : destination);
    }

    private static string NormalizeDestination(string? destination)
    {
        string normalized = destination?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("An audit-completion destination must be configured.");
        }

        if (normalized.Length > 128)
        {
            throw new InvalidOperationException("An audit-completion destination cannot exceed 128 characters.");
        }

        return normalized;
    }

    private static OutboxIdentity CreateIdentity(string destination, string mutationBatchId)
    {
        if (string.IsNullOrWhiteSpace(mutationBatchId))
        {
            throw new InvalidOperationException("An audit-completion receipt must contain a mutation batch identifier.");
        }

        byte[] input = Encoding.UTF8.GetBytes($"{destination}\n{mutationBatchId.Trim()}");
        byte[] hash = SHA256.HashData(input);
        return new OutboxIdentity(
            new Guid(hash.AsSpan(0, 16)),
            $"ncat-audit-completion:{Convert.ToHexString(hash)}");
    }

    private static ApplicationAuditCompletionOutboxEntry ValidateExisting(
        ApplicationAuditCompletionOutboxEntry existing,
        string destination,
        string mutationBatchId,
        string idempotencyKey)
    {
        if (!existing.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) ||
            !existing.MutationBatchId.Equals(mutationBatchId, StringComparison.Ordinal) ||
            !existing.IdempotencyKey.Equals(idempotencyKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The deterministic audit-completion outbox identity resolved to conflicting persisted data.");
        }

        return existing;
    }

    private static ApplicationAuditCompletionMessage ToMessage(
        ApplicationAuditCompletionOutboxEntry entry) =>
        new(
            entry.SchemaVersion,
            entry.Destination,
            entry.IdempotencyKey,
            entry.MutationBatchId,
            entry.AuditRecordCount,
            entry.PersistenceOutcome,
            entry.ReceiptCompletedUtc,
            entry.MutationManifestHash,
            entry.MutationManifestAlgorithm,
            entry.MutationManifestSchemaVersion,
            entry.OperationExecutionId,
            entry.ExecutionAttemptId,
            entry.DecisionAuditRecordId,
            entry.CorrelationId,
            entry.TraceId);

    private static ApplicationAuditCompletionOutboxItem ToItem(
        ApplicationAuditCompletionOutboxEntry entry) =>
        new(
            entry.Id,
            entry.SchemaVersion,
            entry.Destination,
            entry.IdempotencyKey,
            entry.MutationBatchId,
            entry.AuditRecordCount,
            entry.PersistenceOutcome,
            entry.ReceiptCompletedUtc,
            entry.MutationManifestHash,
            entry.MutationManifestAlgorithm,
            entry.MutationManifestSchemaVersion,
            entry.OperationExecutionId,
            entry.ExecutionAttemptId,
            entry.DecisionAuditRecordId,
            entry.CorrelationId,
            entry.TraceId,
            entry.Status,
            entry.RetryCount,
            entry.CreatedUtc,
            entry.LastAttemptUtc,
            entry.NextAttemptUtc,
            entry.DeliveredUtc,
            entry.LastErrorCode,
            entry.LastErrorMessage);

    private static string? SanitizeError(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string sanitized = new(value
            .Trim()
            .Select(character => char.IsControl(character) ? ' ' : character)
            .ToArray());
        return sanitized.Length <= maximumLength
            ? sanitized
            : sanitized[..maximumLength];
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private sealed record OutboxIdentity(Guid Id, string IdempotencyKey);
}
