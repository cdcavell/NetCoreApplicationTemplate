using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Web.Authentication.Extensions;

namespace Template.Web.Tests.TestControllers;

/// <summary>
/// Provides test endpoints for verifying template authentication and authorization behavior.
/// </summary>
[ApiController]
[Route("test/authentication")]
public sealed class AuthenticationTestController : ControllerBase
{
    /// <summary>
    /// Returns an anonymous response that does not require authentication.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing an anonymous result.</returns>
    [HttpGet("anonymous")]
    [AllowAnonymous]
    public IActionResult Anonymous()
    {
        return Ok(new { result = "anonymous" });
    }

    /// <summary>
    /// Returns a protected response that requires an authenticated user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a protected result.</returns>
    [HttpGet("protected")]
    [Authorize(Policy = TemplateAuthorizationPolicyNames.AuthenticatedUser)]
    public IActionResult Protected()
    {
        return Ok(new { result = "protected" });
    }

    /// <summary>
    /// Returns a test response for users who satisfy the administrator role authorization policy.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing an administrator authorization test result.</returns>
    [HttpGet("admin")]
    [Authorize(Policy = TemplateAuthorizationPolicyNames.AdministratorRole)]
    public IActionResult Admin()
    {
        return Ok(new { result = "admin" });
    }

    /// <summary>
    /// Returns a test response for users who satisfy the manage application permission authorization policy.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a manage application permission authorization test result.</returns>
    [HttpGet("manage")]
    [Authorize(Policy = TemplateAuthorizationPolicyNames.ManageApplicationPermission)]
    public IActionResult Manage()
    {
        return Ok(new { result = "manage" });
    }
}
