using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Auditing;

namespace ProjectTemplate.Web.Services;

/// <summary>
/// Dispatches durable audit-completion entries after their originating transaction commits.
/// </summary>
public sealed partial class ApplicationAuditCompletionOutboxHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<ApplicationAuditCompletionOutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<ApplicationAuditCompletionOutboxHostedService> logger)
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory =
        scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ApplicationAuditCompletionOutboxOptions _options =
        options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly TimeProvider _timeProvider =
        timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly ILogger<ApplicationAuditCompletionOutboxHostedService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [LoggerMessage(
        EventId = 19100,
        Level = LogLevel.Error,
        Message = "The audit-completion outbox dispatch cycle failed and will be retried.")]
    private static partial void LogDispatchCycleFailure(
        ILogger logger,
        Exception exception);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IApplicationAuditCompletionOutboxDispatcher dispatcher = scope.ServiceProvider
                    .GetRequiredService<IApplicationAuditCompletionOutboxDispatcher>();
                _ = await dispatcher.DispatchReadyAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogDispatchCycleFailure(_logger, exception);
            }

            await Task.Delay(_options.PollInterval, _timeProvider, stoppingToken)
                .ConfigureAwait(false);
        }
    }
}
