using Microsoft.AspNetCore.Authentication;
using Template.Web.Authentication.Options;

namespace Template.Web.Authentication.Providers.Saml2;

/// <summary>
/// Provides extension methods for registering SAML2 authentication provider services.
/// </summary>
public static class Saml2AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the template SAML2 authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The SAML2 provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddTemplateSaml2Authentication(
        this AuthenticationBuilder builder,
        TemplateExternalAuthenticationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        // SAML2 provider support will be implemented in a future provider-specific issue.
        // This placeholder intentionally does not register a concrete authentication handler yet.
        return builder;
    }
}
