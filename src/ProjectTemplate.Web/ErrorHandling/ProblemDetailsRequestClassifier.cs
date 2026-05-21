using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace ProjectTemplate.Web.ErrorHandling;

/// <summary>
/// Provides methods for determining whether a request should be classified for Problem Details response formatting.
/// </summary>
/// <remarks>This class contains logic to identify requests that are likely to expect Problem Details responses,
/// such as API requests or AJAX calls. It is intended for internal use within the application pipeline.</remarks>
internal static class ProblemDetailsRequestClassifier
{
    /// <summary>
    /// Determines whether a Problem Details response should be written for the specified HTTP context.
    /// </summary>
    /// <remarks>A Problem Details response is written if the request targets an API endpoint (path starts
    /// with '/api'), is an XMLHttpRequest (AJAX), or explicitly accepts a JSON response. HEAD requests are
    /// excluded.</remarks>
    /// <param name="httpContext">The HTTP context for the current request. Cannot be null.</param>
    /// <returns>true if a Problem Details response should be written for the request; otherwise, false.</returns>
    public static bool ShouldWriteProblemDetails(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (HttpMethods.IsHead(httpContext.Request.Method))
        {
            return false;
        }

        if (httpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (httpContext.Request.Headers.TryGetValue(HeaderNames.XRequestedWith, out StringValues requestedWith) &&
            string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        IList<MediaTypeHeaderValue>? acceptHeaders = httpContext.Request.GetTypedHeaders().Accept;

        return acceptHeaders is not null && acceptHeaders.Count != 0 && acceptHeaders.Any(header =>
            header.MediaType.Value?.Contains("json", StringComparison.OrdinalIgnoreCase) == true);
    }
}
