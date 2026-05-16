using Microsoft.AspNetCore.Authentication;
using Template.Web.Authentication.Options;

namespace Template.Web.Authentication.Providers.OpenIdConnect;

/// <summary>
/// Provides extension methods for registering OpenID Connect authentication provider services.
/// </summary>
public static class OpenIdConnectAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the template OpenID Connect authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The OpenID Connect provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddTemplateOpenIdConnectAuthentication(
        this AuthenticationBuilder builder,
        TemplateExternalAuthenticationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        // OpenID Connect provider support will be implemented in a future provider-specific issue.
        // This placeholder intentionally does not register a concrete authentication handler yet.
        return builder;
    }
}
