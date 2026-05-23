using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Web.Controllers.Api;

/// <summary>
/// Provides sample versioned API endpoints for the application.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/application-information")]
[Route("api/application-information")]
public sealed class ApplicationInformationController : ControllerBase
{
    /// <summary>
    /// Returns application API information for the requested API version.
    /// </summary>
    /// <returns>Application API version information.</returns>
    [HttpGet]
    public ActionResult<ApplicationInformationResponse> Get()
    {
        return Ok(new ApplicationInformationResponse(
            ApplicationName: "ProjectTemplate.Web",
            ApiVersion: "1.0",
            Message: "API versioning foundation active."));
    }
}

/// <summary>
/// Represents sample application API information.
/// </summary>
/// <param name="ApplicationName">The application name.</param>
/// <param name="ApiVersion">The resolved API version.</param>
/// <param name="Message">A short status message.</param>
public sealed record ApplicationInformationResponse(
    string ApplicationName,
    string ApiVersion,
    string Message);
