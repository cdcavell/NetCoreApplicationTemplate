namespace Template.Web.Models;

/// <summary>
/// View model for error pages that exposes request and status information.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// The unique request identifier, if available. May be null.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// The HTTP status code associated with the error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Indicates whether a request identifier should be shown.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

    /// <summary>
    /// A short title describing the error derived from the <see cref="StatusCode"/>.
    /// </summary>
    public string Title => StatusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Page Not Found",
        StatusCodes.Status429TooManyRequests => "Too Many Requests",
        _ => "Application Error"
    };

    /// <summary>
    /// A user-friendly message describing the error derived from the <see cref="StatusCode"/>.
    /// </summary>
    public string Message => StatusCode switch
    {
        StatusCodes.Status400BadRequest => "The request could not be processed.",
        StatusCodes.Status401Unauthorized => "Authentication is required to access this resource.",
        StatusCodes.Status403Forbidden => "You do not have permission to access this resource.",
        StatusCodes.Status404NotFound => "The requested page could not be found.",
        StatusCodes.Status429TooManyRequests => "Too many requests were received. Please try again later.",
        _ => "An unexpected error occurred while processing the request."
    };
}
