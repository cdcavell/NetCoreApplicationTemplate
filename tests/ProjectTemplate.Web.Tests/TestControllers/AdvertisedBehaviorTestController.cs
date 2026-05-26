using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Web.Tests.TestControllers;

/// <summary>
/// Provides test endpoints used to verify v1.0 advertised runtime behavior.
/// </summary>
[ApiController]
[Route("test/advertised-behavior")]
public sealed class AdvertisedBehaviorTestController : ControllerBase
{
    /// <summary>
    /// Returns request information after middleware has processed forwarded headers.
    /// </summary>
    /// <returns>Request scheme, host, and remote IP information.</returns>
    [HttpGet("request-info")]
    public IActionResult RequestInfo()
    {
        return Ok(new
        {
            scheme = Request.Scheme,
            host = Request.Host.Value,
            remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }

    /// <summary>
    /// Throws an argument exception to verify API-style Problem Details handling.
    /// </summary>
    /// <exception cref="ArgumentException">Always thrown for test coverage.</exception>
    [HttpGet("problem-details")]
    public static IActionResult ProblemDetailsException()
    {
        throw new ArgumentException("Invalid advertised behavior test request.");
    }
}
