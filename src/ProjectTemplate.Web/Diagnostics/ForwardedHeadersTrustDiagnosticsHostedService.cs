using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Options;

namespace ProjectTemplate.Web.Diagnostics;

/// <summary>
/// Reports likely forwarded-header trust misconfiguration when client-IP rate limiting is enabled.
/// </summary>
internal sealed partial class ForwardedHeadersTrustDiagnosticsHostedService(
    IOptions<ApplicationForwardedHeadersOptions> forwardedHeadersOptions,
    IOptions<ApplicationRateLimitingOptions> rateLimitingOptions,
    IHostEnvironment environment,
    ILogger<ForwardedHeadersTrustDiagnosticsHostedService> logger) : IHostedService
{
    private const string StrictValidationFailureMessage =
        "Forwarded headers and client-IP rate limiting are enabled, but no trusted proxy or network is configured. " +
        "Configure ProjectTemplate:ForwardedHeaders:KnownProxies or KnownNetworks before enabling strict proxy trust validation.";

    private readonly ApplicationForwardedHeadersOptions _forwardedHeadersOptions = forwardedHeadersOptions.Value;
    private readonly ApplicationRateLimitingOptions _rateLimitingOptions = rateLimitingOptions.Value;
    private readonly IHostEnvironment _environment = environment;
    private readonly ILogger _logger = logger;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!RequiresExplicitProxyTrust(
                _forwardedHeadersOptions,
                _rateLimitingOptions,
                _environment))
        {
            return Task.CompletedTask;
        }

        if (_forwardedHeadersOptions.RequireExplicitProxyTrust)
        {
            throw new InvalidOperationException(StrictValidationFailureMessage);
        }

        LogMissingExplicitProxyTrust(_logger, _environment.EnvironmentName);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    internal static bool RequiresExplicitProxyTrust(
        ApplicationForwardedHeadersOptions forwardedHeadersOptions,
        ApplicationRateLimitingOptions rateLimitingOptions,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(forwardedHeadersOptions);
        ArgumentNullException.ThrowIfNull(rateLimitingOptions);
        ArgumentNullException.ThrowIfNull(environment);

        return !environment.IsDevelopment() &&
               forwardedHeadersOptions.Enabled &&
               ProcessesForwardedFor(forwardedHeadersOptions.Headers) &&
               rateLimitingOptions.Enabled &&
               !HasConfiguredTrustBoundary(forwardedHeadersOptions);
    }

    private static bool ProcessesForwardedFor(IEnumerable<string> headers)
    {
        return headers.Any(header =>
            string.Equals(
                NormalizeHeaderName(header),
                "XForwardedFor",
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasConfiguredTrustBoundary(ApplicationForwardedHeadersOptions options)
    {
        return options.KnownProxies.Any(value => !string.IsNullOrWhiteSpace(value)) ||
               options.KnownNetworks.Any(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string NormalizeHeaderName(string? value)
    {
        return value?
            .Trim()
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            ?? string.Empty;
    }

    [LoggerMessage(
        EventId = 5101,
        Level = LogLevel.Warning,
        Message = "Forwarded headers and client-IP rate limiting are enabled in environment {EnvironmentName}, but no deployment-specific trusted proxy or network is configured. ASP.NET Core will ignore forwarded values from untrusted proxies, which can leave RemoteIpAddress set to the proxy address. Multiple clients may then share one rate-limit bucket and client-IP logs may identify the proxy. Configure ProjectTemplate:ForwardedHeaders:KnownProxies or KnownNetworks. Do not bypass this warning by trusting arbitrary X-Forwarded-For values.")]
    private static partial void LogMissingExplicitProxyTrust(
        ILogger logger,
        string environmentName);
}
