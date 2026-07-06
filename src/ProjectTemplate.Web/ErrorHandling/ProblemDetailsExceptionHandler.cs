using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Web.ErrorHandling;

/// <summary>
/// Handles exceptions by generating and writing RFC 7807 Problem Details responses for HTTP requests when appropriate.
/// </summary>
/// <remarks>This exception handler inspects the HTTP context and exception to determine whether a Problem Details
/// response should be written. It sets the response status code and includes trace information for diagnostics. In
/// development environments, detailed exception messages are included in the response; in production, a generic error
/// message is provided for server errors. The handler does not write a response if the request does not expect Problem
/// Details or if the response has already started.</remarks>
/// <param name="problemDetailsService">The service used to write Problem Details responses to the HTTP response.</param>
/// <param name="webHostEnvironment">The hosting environment used to determine whether to include detailed error information.</param>
/// <param name="logger">The logger used to record exception and error information.</param>
internal sealed class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IWebHostEnvironment webHostEnvironment,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the specified exception by generating and writing a Problem Details response to the HTTP
    /// context asynchronously.
    /// </summary>
    /// <remarks>A Problem Details response is only written if the request is classified as requiring Problem
    /// Details and the response has not already started. If the response cannot be written, the method returns <see
    /// langword="false"/> and does not modify the response.</remarks>
    /// <param name="httpContext">The HTTP context for the current request. Cannot be null.</param>
    /// <param name="exception">The exception to handle and convert into a Problem Details response. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a Problem
    /// Details response was written; otherwise, <see langword="false"/>.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (!ProblemDetailsRequestClassifier.ShouldWriteProblemDetails(httpContext))
        {
            return false;
        }

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        ProblemDetails problemDetails = CreateProblemDetails(httpContext, exception);

        LogException(logger, exception, problemDetails.Status ?? StatusCodes.Status500InternalServerError, httpContext);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };

        return await problemDetailsService.TryWriteAsync(problemDetailsContext);
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        int statusCode = GetStatusCode(exception);
        string title = GetTitle(statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["requestId"] = httpContext.TraceIdentifier;

        if (webHostEnvironment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
        }
        else if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            problemDetails.Detail = "An unexpected error occurred. Contact support with the request ID.";
        }

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            TimeoutException => StatusCodes.Status503ServiceUnavailable,
            // Plain ArgumentException is intentionally treated as an internal failure. Broad argument failures can
            // represent server-side developer bugs; request-level failures should use explicit client-input types.
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
            _ => "Internal Server Error"
        };
    }

    private static void LogException(
        ILogger logger,
        Exception exception,
        int statusCode,
        HttpContext httpContext)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            ProblemDetailsLogMessages.UnhandledException(
                logger,
                exception,
                statusCode,
                httpContext.TraceIdentifier,
                httpContext.Request.Path);
        }
        else
        {
            ProblemDetailsLogMessages.HandledException(
                logger,
                exception,
                statusCode,
                httpContext.TraceIdentifier,
                httpContext.Request.Path);
        }
    }
}

/// <summary>
/// Provides strongly-typed logging methods for recording events related to the conversion of exceptions to Problem
/// Details responses.
/// </summary>
/// <remarks>This class defines logging message templates for use with the Microsoft.Extensions.Logging source
/// generator. The methods are intended for internal use to standardize log output when exceptions are converted to
/// Problem Details in HTTP responses.</remarks>
internal static partial class ProblemDetailsLogMessages
{
    /// <summary>
    /// Logs an unhandled exception as an error, including HTTP status code, request identifier, and request path
    /// information.
    /// </summary>
    /// <remarks>This method is intended for use in centralized exception handling scenarios to ensure
    /// consistent logging of unhandled exceptions with relevant request context.</remarks>
    /// <param name="logger">The logger used to write the error message.</param>
    /// <param name="exception">The exception that was not handled.</param>
    /// <param name="statusCode">The HTTP status code associated with the error response.</param>
    /// <param name="requestId">The unique identifier for the current request.</param>
    /// <param name="path">The request path where the exception occurred.</param>
    [LoggerMessage(
        EventId = 32001,
        Level = LogLevel.Error,
        Message = "Unhandled exception converted to Problem Details. StatusCode: {StatusCode}. RequestId: {RequestId}. Path: {Path}.")]
    public static partial void UnhandledException(
        ILogger logger,
        Exception exception,
        int statusCode,
        string requestId,
        string path);

    /// <summary>
    /// Logs a warning message indicating that a handled exception was converted to a Problem Details response,
    /// including status code, request ID, and request path information.
    /// </summary>
    /// <remarks>This method is intended for use in exception handling middleware or filters to provide
    /// consistent logging of handled exceptions that result in Problem Details responses. The log entry includes
    /// contextual information to aid in troubleshooting.</remarks>
    /// <param name="logger">The logger used to write the warning message.</param>
    /// <param name="exception">The exception that was handled and converted to a Problem Details response.</param>
    /// <param name="statusCode">The HTTP status code associated with the Problem Details response.</param>
    /// <param name="requestId">The unique identifier for the request in which the exception occurred.</param>
    /// <param name="path">The request path where the exception was handled.</param>
    [LoggerMessage(
        EventId = 32002,
        Level = LogLevel.Warning,
        Message = "Handled exception converted to Problem Details. StatusCode: {StatusCode}. RequestId: {RequestId}. Path: {Path}.")]
    public static partial void HandledException(
        ILogger logger,
        Exception exception,
        int statusCode,
        string requestId,
        string path);
}