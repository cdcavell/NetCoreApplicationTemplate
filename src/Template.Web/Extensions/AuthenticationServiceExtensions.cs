using Microsoft.AspNetCore.Authentication;
using Template.Web.Options;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to register template authentication services.
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds the template authentication baseline and optional external provider registrations.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="configureProviders">Optional provider registration callback for OIDC, SAML2, or social providers.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddTemplateAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthenticationBuilder>? configureProviders = null)
    {
        TemplateAuthenticationOptions authenticationOptions = new();
        configuration.GetSection(TemplateAuthenticationOptions.SectionName).Bind(authenticationOptions);

        services
            .AddOptions<TemplateAuthenticationOptions>()
            .Bind(configuration.GetSection(TemplateAuthenticationOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultScheme),
                "Template:Authentication:DefaultScheme is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultChallengeScheme),
                "Template:Authentication:DefaultChallengeScheme is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Cookie.Scheme),
                "Template:Authentication:Cookie:Scheme is required.")
            .Validate(options => options.Cookie.ExpireTimeSpanMinutes > 0,
                "Template:Authentication:Cookie:ExpireTimeSpanMinutes must be greater than zero.")
            .ValidateOnStart();

        AuthenticationBuilder authenticationBuilder = services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = authenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = authenticationOptions.DefaultChallengeScheme;
            })
            .AddCookie(authenticationOptions.Cookie.Scheme, options =>
            {
                options.Cookie.Name = authenticationOptions.Cookie.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.LoginPath = authenticationOptions.Cookie.LoginPath;
                options.LogoutPath = authenticationOptions.Cookie.LogoutPath;
                options.AccessDeniedPath = authenticationOptions.Cookie.AccessDeniedPath;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(authenticationOptions.Cookie.ExpireTimeSpanMinutes);
                options.SlidingExpiration = authenticationOptions.Cookie.SlidingExpiration;
            });

        configureProviders?.Invoke(authenticationBuilder);

        return services;
    }
}
