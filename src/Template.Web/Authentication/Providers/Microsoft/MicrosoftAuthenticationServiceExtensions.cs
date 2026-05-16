using Microsoft.AspNetCore.Authentication;
using Template.Web.Authentication.Options;

namespace Template.Web.Authentication.Providers.Microsoft;

/// <summary>
/// Provides extension methods for registering Microsoft authentication provider services.
/// </summary>
public static class MicrosoftAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the template Microsoft authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The Microsoft provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddTemplateMicrosoftAuthentication(
        this AuthenticationBuilder builder,
        TemplateExternalAuthenticationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        // Microsoft provider support will be implemented in a future provider-specific issue.
        // This placeholder intentionally does not register a concrete authentication handler yet.
        return builder;
    }
}
