using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Template.Web.Constants;
using Template.Web.Models;

namespace Template.Web.Controllers;

/// <summary>
/// Controller for handling the home page and error routes.
/// </summary>
/// <param name="logger">The logger instance for the controller.</param>
public partial class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    /// <summary>
    /// Displays an error page for the current request. If a status code is provided it will be used;
    /// otherwise a 500 Internal Server Error status code is assumed. The method logs either the
    /// unhandled exception or the status code routing information and returns an Error view
    /// containing an ErrorViewModel with the resolved status code and request id.
    /// </summary>
    /// <param name="statusCode">Optional HTTP status code to use for the response. If null, 500 is used.</param>
    /// <returns>An IActionResult that renders the Error view.</returns>
    [AllowAnonymous]
    [Route("Home/Error/{statusCode:int?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        int resolvedStatusCode = statusCode ?? StatusCodes.Status500InternalServerError;

        Response.StatusCode = resolvedStatusCode;

        IExceptionHandlerPathFeature? exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        IStatusCodeReExecuteFeature? statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        string originalPath =
            exceptionFeature?.Path ??
            statusCodeFeature?.OriginalPath ??
            HttpContext.Request.Path.ToString();

        string? remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        string requestId = HttpContext.TraceIdentifier;

        if (exceptionFeature?.Error is not null)
        {
            LogUnhandledExceptionRoutedToErrorPage(
                _logger,
                exceptionFeature.Error,
                resolvedStatusCode,
                originalPath,
                remoteIpAddress,
                requestId);
        }
        else
        {
            LogStatusCodePageRoutedToErrorPage(
                _logger,
                resolvedStatusCode,
                originalPath,
                remoteIpAddress,
                requestId);
        }

        return View(new ErrorViewModel
        {
            StatusCode = resolvedStatusCode,
            RequestId = requestId
        });
    }

    [LoggerMessage(
        EventId = TemplateLogEventIds.UnhandledExceptionRoutedToErrorPage,
        Level = LogLevel.Error,
        Message = "Unhandled exception routed to error page. StatusCode: {StatusCode}; OriginalPath: {OriginalPath}; RemoteIpAddress: {RemoteIpAddress}; TraceIdentifier: {TraceIdentifier}")]
    private static partial void LogUnhandledExceptionRoutedToErrorPage(
        ILogger logger,
        Exception exception,
        int statusCode,
        string originalPath,
        string? remoteIpAddress,
        string traceIdentifier);

    [LoggerMessage(
        EventId = TemplateLogEventIds.StatusCodePageRoutedToErrorPage,
        Level = LogLevel.Warning,
        Message = "Status code page routed to error page. StatusCode: {StatusCode}; OriginalPath: {OriginalPath}; RemoteIpAddress: {RemoteIpAddress}; TraceIdentifier: {TraceIdentifier}")]
    private static partial void LogStatusCodePageRoutedToErrorPage(
        ILogger logger,
        int statusCode,
        string originalPath,
        string? remoteIpAddress,
        string traceIdentifier);
}
