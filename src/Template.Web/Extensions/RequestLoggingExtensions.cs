using Serilog;
using Serilog.Events;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods for configuring request logging on a <see cref="WebApplication"/>.
/// </summary>
public static class RequestLoggingExtensions
{
    /// <summary>
    /// Configures Serilog request logging with the application's preferred message template,
    /// log level selection logic, and diagnostic context enrichment.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    public static WebApplication UseTemplateRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = (httpContext, elapsed, exception) =>
            {
                if (exception is not null)
                {
                    return LogEventLevel.Error;
                }

                int statusCode = httpContext.Response.StatusCode;

                return statusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : statusCode >= StatusCodes.Status400BadRequest ? LogEventLevel.Warning : LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RequestPathBase", httpContext.Request.PathBase.Value);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);

                diagnosticContext.Set("RemoteIpAddress",
                    httpContext.Connection.RemoteIpAddress?.ToString());

                diagnosticContext.Set("UserName",
                    httpContext.User?.Identity?.IsAuthenticated == true
                        ? httpContext.User.Identity.Name
                        : null);

                diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);
            };
        });

        return app;
    }
}

