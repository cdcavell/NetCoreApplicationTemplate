using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProjectTemplate.Web.Authentication.Options;

namespace ProjectTemplate.Web.Authentication.Providers.Microsoft;

/// <summary>
/// Provides extension methods for registering Microsoft authentication provider services.
/// </summary>
public static class MicrosoftAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the Microsoft authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The Microsoft provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddMicrosoftAuthentication(
        this AuthenticationBuilder builder,
        ApplicationExternalAuthenticationProviderOptions options)
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
