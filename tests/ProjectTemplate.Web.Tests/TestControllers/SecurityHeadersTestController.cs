using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Web.Tests.TestControllers;

/// <summary>
/// Provides test endpoints for verifying security header middleware behavior.
/// </summary>
[ApiController]
[Route("test/security-headers")]
public sealed class SecurityHeadersTestController : ControllerBase
{
    /// <summary>
    /// Returns a successful response for security header tests.
    /// </summary>
    /// <returns>An OK response.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { result = "security-headers" });
    }

    /// <summary>
    /// Returns a successful health response for excluded path tests.
    /// </summary>
    /// <returns>An OK response.</returns>
    [HttpGet("/health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy" });
    }

    /// <summary>
    /// Returns a successful metrics response for excluded path tests.
    /// </summary>
    /// <returns>An OK response.</returns>
    [HttpGet("/metrics")]
    public IActionResult Metrics()
    {
        return Ok(new { status = "metrics" });
    }
}
