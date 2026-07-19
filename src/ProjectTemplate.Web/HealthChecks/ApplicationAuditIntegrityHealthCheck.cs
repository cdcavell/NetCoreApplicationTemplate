using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Auditing;

namespace ProjectTemplate.Web.HealthChecks;

public sealed class ApplicationAuditIntegrityHealthCheck(
    IServiceScopeFactory scopeFactory,
    IOptions<ApplicationAuditReconciliationOptions> options)
    : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory =
        scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ApplicationAuditReconciliationOptions _options =
        options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_options.Enabled)
        {
            return HealthCheckResult.Healthy("Audit reconciliation is disabled.");
        }

        using IServiceScope scope = _scopeFactory.CreateScope();
        IApplicationAuditReconciler reconciler = scope.ServiceProvider
            .GetRequiredService<IApplicationAuditReconciler>();
        ApplicationAuditReconciliationSummary summary = await reconciler
            .GetSummaryAsync(cancellationToken)
            .ConfigureAwait(false);

        ApplicationAuditCompletionOutboxHealth? deliveryHealth = null;
        IApplicationAuditCompletionOutboxQuery? outboxQuery = scope.ServiceProvider
            .GetService<IApplicationAuditCompletionOutboxQuery>();
        if (outboxQuery is not null)
        {
            deliveryHealth = await outboxQuery.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            scope.ServiceProvider
                .GetRequiredService<ApplicationAuditReconciliationMetrics>()
                .UpdateDelivery(deliveryHealth);
        }

        var data = new Dictionary<string, object>
        {
            ["openFindings"] = summary.OpenFindingCount,
            ["errorFindings"] = summary.ErrorFindingCount,
            ["criticalFindings"] = summary.CriticalFindingCount,
            ["manifestVerificationFailures"] = summary.ManifestVerificationFailureCount,
            ["missingCompletions"] = summary.MissingCompletionCount,
            ["staleDeliveryFindings"] = summary.StaleDeliveryCount,
            ["deadLetterFindings"] = summary.DeadLetterCount
        };

        if (summary.LastRunUtc.HasValue)
        {
            data["lastReconciliationUtc"] = summary.LastRunUtc.Value;
        }

        if (deliveryHealth is not null)
        {
            data["outboxBacklog"] = deliveryHealth.BacklogCount;
            data["outboxRetryCount"] = deliveryHealth.TotalRetryCount;
            data["outboxDeadLetters"] = deliveryHealth.DeadLetterCount;
            if (deliveryHealth.OldestPendingAge.HasValue)
            {
                data["oldestPendingAgeSeconds"] = deliveryHealth.OldestPendingAge.Value.TotalSeconds;
            }
        }

        bool unhealthy = summary.CriticalFindingCount > 0 ||
            summary.ManifestVerificationFailureCount > 0 ||
            summary.OpenFindingCount >= _options.HealthUnhealthyFindingCount;
        if (unhealthy)
        {
            return HealthCheckResult.Unhealthy(
                "Critical audit-integrity findings require operator review.",
                data: data);
        }

        bool degraded = summary.OpenFindingCount >= _options.HealthWarningFindingCount ||
            summary.StaleDeliveryCount > 0 ||
            summary.DeadLetterCount > 0 ||
            deliveryHealth?.DeadLetterCount > 0;
        return degraded
            ? HealthCheckResult.Degraded(
                "Audit reconciliation or delivery findings require attention.",
                data: data)
            : HealthCheckResult.Healthy("Audit integrity and delivery state are within configured thresholds.", data);
    }
}
