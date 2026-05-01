using Serilog;
using Serilog.Events;

namespace Template.Web.Extensions
{
    public static class RequestLoggingExtensions
    {
        public static WebApplication UseTemplateRequestLogging(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

                options.GetLevel = (httpContext, elapsed, exception) =>
                {
                    if (exception is not null)
                        return LogEventLevel.Error;

                    int statusCode = httpContext.Response.StatusCode;

                    if (statusCode >= StatusCodes.Status500InternalServerError)
                        return LogEventLevel.Error;

                    if (statusCode >= StatusCodes.Status400BadRequest)
                        return LogEventLevel.Warning;

                    return LogEventLevel.Information;
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
}
