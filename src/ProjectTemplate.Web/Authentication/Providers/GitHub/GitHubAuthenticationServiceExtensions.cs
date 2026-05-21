using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProjectTemplate.Web.Authentication.Options;

namespace ProjectTemplate.Web.Authentication.Providers.GitHub;

/// <summary>
/// Provides extension methods for registering GitHub authentication provider services.
/// </summary>
public static class GitHubAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the GitHub authentication provider registration.
    /// </summary>
    /// <param name="builder">The authentication builder used to register authentication handlers.</param>
    /// <param name="options">The GitHub provider options.</param>
    /// <returns>The same <see cref="AuthenticationBuilder"/> instance for chaining.</returns>
    public static AuthenticationBuilder AddGitHubAuthentication(
        this AuthenticationBuilder builder,
        ApplicationExternalAuthenticationProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        builder.AddGitHub(options.Scheme, options.DisplayName, githubOptions =>
        {
            githubOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            githubOptions.ClientId = options.ClientId;
            githubOptions.ClientSecret = options.ClientSecret;
            githubOptions.CallbackPath = options.CallbackPath;

            foreach (string scope in options.Scopes.Where(scope => !string.IsNullOrWhiteSpace(scope)))
            {
                githubOptions.Scope.Add(scope);
            }
        });

        return builder;
    }
}
