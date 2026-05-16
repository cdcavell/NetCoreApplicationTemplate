using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Template.Web.Tests.Infrastructure;

/// <summary>
/// Provides a test-only external authentication handler used to verify challenge endpoint behavior.
/// </summary>
/// <param name="options">The authentication scheme options monitor.</param>
/// <param name="logger">The logger factory.</param>
/// <param name="encoder">The URL encoder.</param>
internal sealed class TestExternalChallengeAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// Returns no authentication result because this handler is only used to test challenge behavior.
    /// </summary>
    /// <returns>A task containing an empty authentication result.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }

    /// <summary>
    /// Handles a challenge by redirecting to the authentication properties redirect URI.
    /// </summary>
    /// <param name="properties">The authentication properties supplied by the challenge endpoint.</param>
    /// <returns>A completed task.</returns>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        string redirectUri = string.IsNullOrWhiteSpace(properties.RedirectUri)
            ? "/"
            : properties.RedirectUri;

        Response.Redirect(redirectUri);

        return Task.CompletedTask;
    }
}
