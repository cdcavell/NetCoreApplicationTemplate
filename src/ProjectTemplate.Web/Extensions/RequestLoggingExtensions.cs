using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using ProjectTemplate.Web.Options;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace ProjectTemplate.Web.Extensions;

/// <summary>
/// Provides extension methods for configuring structured HTTP request logging.
/// </summary>
public static class RequestLoggingExtensions
{
    /// <summary>
    /// Registers structured request logging options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The original service collection for chaining.</returns>
    public static IServiceCollection AddApplicationRequestLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ApplicationRequestLoggingOptions>()
            .Bind(configuration.GetSection(ApplicationRequestLoggingOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.CorrelationHeaderName),
                "ProjectTemplate:RequestLogging:CorrelationHeaderName is required.")
            .Validate(
                options => options.ExcludedPathPrefixes.All(path =>
                    !string.IsNullOrWhiteSpace(path) &&
                    path.StartsWith('/')),
                "ProjectTemplate:RequestLogging:ExcludedPathPrefixes values must start with '/'.")
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Configures structured request logging with correlation identifiers,
    /// request duration metrics, filtering, and safe diagnostic enrichment.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The same web application instance for chaining.</returns>
    public static WebApplication UseApplicationRequestLogging(this WebApplication app)
    {
        ApplicationRequestLoggingOptions requestLoggingOptions = app.Services
            .GetRequiredService<IOptions<ApplicationRequestLoggingOptions>>()
            .Value;

        if (!requestLoggingOptions.Enabled)
        {
            return app;
        }

        app.Use(async (context, next) =>
        {
            string correlationId = GetCorrelationId(context, requestLoggingOptions);

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(requestLoggingOptions.CorrelationHeaderName))
                {
                    context.Response.Headers[requestLoggingOptions.CorrelationHeaderName] = correlationId;
                }

                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
            {
                await next(context);
            }
        });

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = (httpContext, _, exception) =>
            {
                if (IsExcludedPath(httpContext.Request.Path, requestLoggingOptions))
                {
                    return LogEventLevel.Verbose;
                }

                if (exception is not null)
                {
                    return LogEventLevel.Error;
                }

                int statusCode = httpContext.Response.StatusCode;

                return statusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : statusCode >= StatusCodes.Status400BadRequest
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;
            };

            // Do not enrich request logs with request bodies, response bodies, cookies,
            // authorization headers, access tokens, refresh tokens, SAML/OIDC payloads,
            // password fields, form fields, or query strings unless explicitly reviewed.
            // Request logging should default to operational metadata only.
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("CorrelationId", GetCorrelationId(httpContext, requestLoggingOptions));
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestPathBase", httpContext.Request.PathBase.Value);

                if (requestLoggingOptions.IncludeQueryString)
                {
                    diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                }

                if (requestLoggingOptions.IncludeRemoteIpAddress)
                {
                    diagnosticContext.Set(
                        "RemoteIpAddress",
                        httpContext.Connection.RemoteIpAddress?.ToString());
                }

                if (requestLoggingOptions.IncludeUserName)
                {
                    diagnosticContext.Set(
                        "UserName",
                        httpContext.User?.Identity?.IsAuthenticated == true
                            ? httpContext.User.Identity.Name
                            : null);
                }
            };
        });

        return app;
    }

    private static string GetCorrelationId(
        HttpContext httpContext,
        ApplicationRequestLoggingOptions options)
    {
        if (httpContext.Request.Headers.TryGetValue(options.CorrelationHeaderName, out StringValues headerValues))
        {
            string? headerValue = headerValues.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                string cleanValue = headerValue.Trim();

                return cleanValue.Length <= 128
                    ? cleanValue
                    : cleanValue[..128];
            }
        }

        return httpContext.TraceIdentifier;
    }

    private static bool IsExcludedPath(
        PathString requestPath,
        ApplicationRequestLoggingOptions options)
    {
        return requestPath.HasValue && options.ExcludedPathPrefixes.Any(prefix =>
            requestPath.StartsWithSegments(
                new PathString(prefix),
                StringComparison.OrdinalIgnoreCase));
    }
}
