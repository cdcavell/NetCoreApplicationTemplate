using System.Diagnostics.Metrics;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

public sealed class ApplicationAuditReconciliationMetrics : IDisposable
{
    public static readonly ApplicationAuditReconciliationSummary DisabledSummary =
        new(false, null, 0, 0, 0, 0, 0, 0, 0);

    private readonly Meter _meter = new("ProjectTemplate.AuditReconciliation", "1.0.0");
    private ApplicationAuditReconciliationSummary _summary = DisabledSummary;
    private long _pendingDeliveryCount;
    private double _oldestPendingAgeSeconds;
    private long _totalRetryCount;

    public ApplicationAuditReconciliationMetrics()
    {
        _meter.CreateObservableGauge("ncat.audit.reconciliation.findings.open",
            () => _summary.OpenFindingCount);
        _meter.CreateObservableGauge("ncat.audit.reconciliation.findings.error",
            () => _summary.ErrorFindingCount);
        _meter.CreateObservableGauge("ncat.audit.reconciliation.findings.critical",
            () => _summary.CriticalFindingCount);
        _meter.CreateObservableGauge("ncat.audit.reconciliation.manifest_failures",
            () => _summary.ManifestVerificationFailureCount);
        _meter.CreateObservableGauge("ncat.audit.reconciliation.missing_completions",
            () => _summary.MissingCompletionCount);
        _meter.CreateObservableGauge("ncat.audit.reconciliation.stale_delivery",
            () => _summary.StaleDeliveryCount);
        _meter.CreateObservableGauge("ncat.audit.reconciliation.dead_letters",
            () => _summary.DeadLetterCount);
        _meter.CreateObservableGauge("ncat.audit.outbox.pending",
            () => Interlocked.Read(ref _pendingDeliveryCount));
        _meter.CreateObservableGauge("ncat.audit.outbox.oldest_pending_age_seconds",
            () => Volatile.Read(ref _oldestPendingAgeSeconds));
        _meter.CreateObservableGauge("ncat.audit.outbox.retry_count",
            () => Interlocked.Read(ref _totalRetryCount));
    }

    public DateTime? LastRunUtc => _summary.LastRunUtc;

    public void Update(ApplicationAuditReconciliationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        _summary = summary;
    }

    public void UpdateDelivery(ApplicationAuditCompletionOutboxHealth health)
    {
        ArgumentNullException.ThrowIfNull(health);
        Interlocked.Exchange(ref _pendingDeliveryCount, health.BacklogCount);
        Volatile.Write(ref _oldestPendingAgeSeconds, health.OldestPendingAge?.TotalSeconds ?? 0);
        Interlocked.Exchange(ref _totalRetryCount, health.TotalRetryCount);
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
