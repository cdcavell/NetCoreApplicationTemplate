using Microsoft.AspNetCore.Authentication;
using Template.Web.Authentication.Options;

namespace Template.Web.Authentication.Providers.Google;

/// <summary>
/// Provides extension methods for registering Google authentication provider services.
/// </summary>
public static class GoogleAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the template Google authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The Google provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddTemplateGoogleAuthentication(
        this AuthenticationBuilder builder,
        TemplateExternalAuthenticationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        // Google provider support will be implemented in a future provider-specific issue.
        // This placeholder intentionally does not register a concrete authentication handler yet.
        return builder;
    }
}
