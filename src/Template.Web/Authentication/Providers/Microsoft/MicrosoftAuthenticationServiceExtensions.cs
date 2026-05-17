using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        builder.AddMicrosoftAccount(options.Scheme, options.DisplayName, microsoftOptions =>
        {
            microsoftOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            microsoftOptions.ClientId = options.ClientId;
            microsoftOptions.ClientSecret = options.ClientSecret;
            microsoftOptions.CallbackPath = options.CallbackPath;

            foreach (string scope in options.Scopes.Where(scope => !string.IsNullOrWhiteSpace(scope)))
            {
                microsoftOptions.Scope.Add(scope);
            }
        });

        return builder;
    }
}
