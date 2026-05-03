using System.Globalization;
using System.Threading.RateLimiting;
using Template.Web.Constants;
using Template.Web.Options;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to register rate limiting services for the application.
/// </summary>
public static partial class RateLimitingServiceExtensions
{
    /// <summary>
    /// Adds the application's predefined rate limiting policies to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the rate limiting services to.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="environment">The current hosting environment.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so calls can be chained.</returns>
    public static IServiceCollection AddTemplateRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        TemplateRateLimitingOptions rateLimitingOptions = CreateDefaultOptions(environment);

        configuration
            .GetSection(TemplateRateLimitingOptions.SectionName)
            .Bind(rateLimitingOptions);

        services.Configure<TemplateRateLimitingOptions>(
            configuration.GetSection(TemplateRateLimitingOptions.SectionName));


        _ = services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                HttpContext httpContext = context.HttpContext;
                HttpResponse response = httpContext.Response;

                TimeSpan? retryAfter = null;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue))
                {
                    retryAfter = retryAfterValue;

                    response.Headers.RetryAfter =
                        Math.Ceiling(retryAfterValue.TotalSeconds)
                            .ToString(CultureInfo.InvariantCulture);
                }

                ILogger logger = httpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Template.Web.RateLimiting");

                LogRateLimitRejectedRequest(
                    logger,
                    httpContext.Request.Method,
                    httpContext.Request.Path.Value ?? string.Empty,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.GetEndpoint()?.DisplayName,
                    retryAfter?.TotalSeconds,
                    httpContext.TraceIdentifier);

                response.StatusCode = StatusCodes.Status429TooManyRequests;
                response.ContentType = "application/json";

                await response.WriteAsJsonAsync(new
                {
                    error = "Too many requests.",
                    statusCode = StatusCodes.Status429TooManyRequests,
                    traceId = httpContext.TraceIdentifier
                }, cancellationToken);
            };

            if (!rateLimitingOptions.Enabled)
            {
                return;
            }

            if (rateLimitingOptions.UseGlobalLimiter)
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientPartitionKey(httpContext),
                        factory: _ => CreateFixedWindowRateLimiterOptions(rateLimitingOptions.GlobalFixedWindow)));
            }

            options.AddPolicy(TemplateRateLimitingPolicyNames.Fixed, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientPartitionKey(httpContext),
                    factory: _ => CreateFixedWindowRateLimiterOptions(rateLimitingOptions.FixedWindowPolicy)));

            options.AddPolicy(TemplateRateLimitingPolicyNames.Concurrency, httpContext =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: GetEndpointPartitionKey(httpContext),
                    factory: _ => CreateConcurrencyLimiterOptions(rateLimitingOptions.ConcurrencyPolicy)));
        });

        return services;
    }

    private static TemplateRateLimitingOptions CreateDefaultOptions(IHostEnvironment environment)
    {
        TemplateRateLimitingOptions options = new();

        if (environment.IsDevelopment())
        {
            options.GlobalFixedWindow.PermitLimit = 300;
            options.GlobalFixedWindow.WindowSeconds = 60;

            options.FixedWindowPolicy.PermitLimit = 120;
            options.FixedWindowPolicy.WindowSeconds = 60;

            options.ConcurrencyPolicy.PermitLimit = 20;
        }

        return options;
    }

    private static FixedWindowRateLimiterOptions CreateFixedWindowRateLimiterOptions(
        FixedWindowRateLimitingOptions options)
    {
        return new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = EnsureAtLeast(options.PermitLimit, minimum: 1),
            Window = TimeSpan.FromSeconds(EnsureAtLeast(options.WindowSeconds, minimum: 1)),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = EnsureAtLeast(options.QueueLimit, minimum: 0)
        };
    }

    private static ConcurrencyLimiterOptions CreateConcurrencyLimiterOptions(
        ConcurrencyRateLimitingOptions options)
    {
        return new ConcurrencyLimiterOptions
        {
            PermitLimit = EnsureAtLeast(options.PermitLimit, minimum: 1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = EnsureAtLeast(options.QueueLimit, minimum: 0)
        };
    }

    private static string GetClientPartitionKey(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";
    }

    private static string GetEndpointPartitionKey(HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.DisplayName
            ?? httpContext.Request.Path.Value
            ?? "unknown-endpoint";
    }

    private static int EnsureAtLeast(int value, int minimum)
    {
        return value < minimum ? minimum : value;
    }

    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Warning,
        Message = "Rate limit rejected request. Method: {Method}; Path: {Path}; RemoteIpAddress: {RemoteIpAddress}; Endpoint: {Endpoint}; RetryAfterSeconds: {RetryAfterSeconds}; TraceIdentifier: {TraceIdentifier}")]
    private static partial void LogRateLimitRejectedRequest(
        ILogger logger,
        string method,
        string path,
        string? remoteIpAddress,
        string? endpoint,
        double? retryAfterSeconds,
        string traceIdentifier);
}
