using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data.Options;

namespace ProjectTemplate.Infrastructure.Data.Services;

internal sealed partial class DataAccessStartupLogger(
    ILogger<DataAccessStartupLogger> logger,
    IOptions<DataAccessOptions> options) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        LogDataAccessConfiguration(
            logger,
            options.Value.Provider,
            options.Value.ConnectionStringName,
            options.Value.Auditing.Enabled ? "enabled" : "disabled");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 19100,
        Level = LogLevel.Information,
        Message = "Data access configured. Provider: {Provider}; ConnectionStringName: {ConnectionStringName}; EF Core auditing: {AuditingStatus}.")]
    private static partial void LogDataAccessConfiguration(
        ILogger logger,
        string provider,
        string connectionStringName,
        string auditingStatus);
}
