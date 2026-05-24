using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Web.Controllers.Api;

/// <summary>
/// Provides sample versioned API endpoints for the application.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("0.9", Deprecated = true)]
[Route("api/v{version:apiVersion}/application-information")]
[Route("api/application-information")]
public sealed class ApplicationInformationController : ControllerBase
{
    private const string _deprecationHeaderName = "Deprecation";
    private const string _sunsetHeaderName = "Sunset";

    private static readonly DateTimeOffset _deprecatedVersionSunsetDate =
        new(2026, 12, 31, 23, 59, 59, TimeSpan.Zero);

    /// <summary>
    /// Returns application API information for the requested API version.
    /// </summary>
    /// <returns>Application API version information.</returns>
    [HttpGet]
    public ActionResult<ApplicationInformationResponse> Get()
    {
        ApiVersion requestedVersion = HttpContext.Features
            .Get<IApiVersioningFeature>()
            ?.RequestedApiVersion ?? new ApiVersion(1, 0);

        if (requestedVersion.MajorVersion == 0 && requestedVersion.MinorVersion == 9)
        {
            AppendDeprecationHeaders();
        }

        return Ok(new ApplicationInformationResponse(
            ApplicationName: "ProjectTemplate.Web",
            ApiVersion: FormatApiVersion(requestedVersion),
            Message: "API versioning foundation active."));
    }

    private static string FormatApiVersion(ApiVersion version)
    {
        return string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"{version.MajorVersion}.{version.MinorVersion}");
    }

    private void AppendDeprecationHeaders()
    {
        Response.Headers[_deprecationHeaderName] = "true";
        Response.Headers[_sunsetHeaderName] = _deprecatedVersionSunsetDate.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        Response.Headers.Link = "</api/v1/application-information>; rel=\"successor-version\"";
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
