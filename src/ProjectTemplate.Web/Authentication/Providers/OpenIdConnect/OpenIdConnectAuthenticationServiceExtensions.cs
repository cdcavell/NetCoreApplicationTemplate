using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProjectTemplate.Web.Authentication.Providers.OpenIdConnect;

/// <summary>
/// Provides extension methods for registering OpenID Connect authentication provider services.
/// </summary>
public static class OpenIdConnectAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the OpenID Connect authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The OpenID Connect provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddOpenIdConnectAuthentication(
        this AuthenticationBuilder builder,
        OpenIdConnectAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        builder.AddOpenIdConnect(options.Scheme, options.DisplayName, openIdConnectOptions =>
        {
            openIdConnectOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            openIdConnectOptions.Authority = options.Authority;
            openIdConnectOptions.ClientId = options.ClientId;
            openIdConnectOptions.ClientSecret = options.ClientSecret;
            openIdConnectOptions.CallbackPath = options.CallbackPath;
            openIdConnectOptions.ResponseType = options.ResponseType;
            openIdConnectOptions.SaveTokens = options.SaveTokens;

            openIdConnectOptions.Scope.Clear();

            foreach (string scope in options.Scopes.Where(scope => !string.IsNullOrWhiteSpace(scope)))
            {
                openIdConnectOptions.Scope.Add(scope);
            }
        });

        return builder;
    }
}
