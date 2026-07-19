using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Auditing;

namespace ProjectTemplate.Web.Services;

public sealed partial class ApplicationAuditReconciliationHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<ApplicationAuditReconciliationOptions> options,
    TimeProvider timeProvider,
    ILogger<ApplicationAuditReconciliationHostedService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory =
        scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ApplicationAuditReconciliationOptions _options =
        options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly TimeProvider _timeProvider =
        timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<ApplicationAuditReconciliationHostedService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [LoggerMessage(
        EventId = 19110,
        Level = LogLevel.Error,
        Message = "The audit reconciliation cycle failed and will be retried.")]
    private static partial void LogReconciliationFailure(ILogger logger, Exception exception);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled || !_options.RunWorker)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IApplicationAuditReconciler reconciler = scope.ServiceProvider
                    .GetRequiredService<IApplicationAuditReconciler>();
                _ = await reconciler.ReconcileAsync(stoppingToken).ConfigureAwait(false);

                IApplicationAuditCompletionOutboxQuery? outboxQuery = scope.ServiceProvider
                    .GetService<IApplicationAuditCompletionOutboxQuery>();
                if (outboxQuery is not null)
                {
                    ApplicationAuditCompletionOutboxHealth deliveryHealth = await outboxQuery
                        .GetHealthAsync(stoppingToken)
                        .ConfigureAwait(false);
                    scope.ServiceProvider
                        .GetRequiredService<ApplicationAuditReconciliationMetrics>()
                        .UpdateDelivery(deliveryHealth);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogReconciliationFailure(_logger, exception);
            }

            await Task.Delay(_options.Interval, _timeProvider, stoppingToken)
                .ConfigureAwait(false);
        }
    }
}
