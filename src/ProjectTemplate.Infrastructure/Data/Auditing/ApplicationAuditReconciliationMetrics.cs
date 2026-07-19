using System.Diagnostics.Metrics;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

public sealed class ApplicationAuditReconciliationMetrics : IDisposable
{
    public static readonly ApplicationAuditReconciliationSummary DisabledSummary =
        new(false, null, 0, 0, 0, 0, 0, 0, 0);

    private readonly Meter _meter = new("ProjectTemplate.AuditReconciliation", "1.0.0");
    private ApplicationAuditReconciliationSummary _summary = DisabledSummary;

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
    }

    public DateTime? LastRunUtc => _summary.LastRunUtc;

    public void Update(ApplicationAuditReconciliationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        _summary = summary;
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
