using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        builder.AddGoogle(options.Scheme, options.DisplayName, googleOptions =>
        {
            googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            googleOptions.ClientId = options.ClientId;
            googleOptions.ClientSecret = options.ClientSecret;
            googleOptions.CallbackPath = options.CallbackPath;

            foreach (string scope in options.Scopes.Where(scope => !string.IsNullOrWhiteSpace(scope)))
            {
                if (!googleOptions.Scope.Contains(scope, StringComparer.Ordinal))
                {
                    googleOptions.Scope.Add(scope);
                }
            }
        });

        return builder;
    }
}
