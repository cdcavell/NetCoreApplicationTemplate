using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Template.Web.Authentication.Options;
using Template.Web.Authentication.Providers.Google;
using Template.Web.Authentication.Providers.Microsoft;
using Template.Web.Authentication.Providers.OpenIdConnect;
using Template.Web.Authentication.Providers.Saml2;

namespace Template.Web.Authentication.Extensions;

/// <summary>
/// Provides extension methods for registering and applying template authentication services.
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds template authentication services based on configuration.
    /// </summary>
    /// <param name="services">The service collection to add authentication services to.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddTemplateAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<TemplateAuthenticationOptions>()
            .Bind(configuration.GetSection(TemplateAuthenticationOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultScheme),
                "Template:Authentication:DefaultScheme is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultChallengeScheme),
                "Template:Authentication:DefaultChallengeScheme is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultSignInScheme),
                "Template:Authentication:DefaultSignInScheme is required.")
            .Validate(options => !options.Enabled || options.Cookie.Enabled,
                "Template:Authentication:Cookie:Enabled must be true when template authentication is enabled.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Cookie.Scheme),
                "Template:Authentication:Cookie:Scheme is required when template authentication is enabled.")
            .Validate(options => !options.Enabled || options.Cookie.ExpireMinutes > 0,
                "Template:Authentication:Cookie:ExpireMinutes must be greater than zero when template authentication is enabled.")
            .ValidateOnStart();

        AuthenticationBuilder authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
        services
            .AddOptions<AuthenticationOptions>()
            .Configure<IOptions<TemplateAuthenticationOptions>>((options, templateAuthenticationOptionsAccessor) =>
            {
                TemplateAuthenticationOptions templateAuthenticationOptions =
                    templateAuthenticationOptionsAccessor.Value;

                if (!templateAuthenticationOptions.Enabled)
                {
                    return;
                }

                options.DefaultScheme = templateAuthenticationOptions.DefaultScheme;
                options.DefaultAuthenticateScheme = templateAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = templateAuthenticationOptions.DefaultChallengeScheme;
                options.DefaultSignInScheme = templateAuthenticationOptions.DefaultSignInScheme;
            });

        authenticationBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        services
            .AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<IOptions<TemplateAuthenticationOptions>>((options, templateAuthenticationOptionsAccessor) =>
            {
                TemplateAuthenticationOptions templateAuthenticationOptions =
                    templateAuthenticationOptionsAccessor.Value;

                options.LoginPath = templateAuthenticationOptions.Cookie.LoginPath;
                options.LogoutPath = templateAuthenticationOptions.Cookie.LogoutPath;
                options.AccessDeniedPath = templateAuthenticationOptions.Cookie.AccessDeniedPath;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(templateAuthenticationOptions.Cookie.ExpireMinutes);
                options.SlidingExpiration = templateAuthenticationOptions.Cookie.SlidingExpiration;

                options.Cookie.Name = ".Template.Web.Authentication";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

        TemplateAuthenticationOptions templateAuthenticationOptions = configuration
            .GetSection(TemplateAuthenticationOptions.SectionName)
            .Get<TemplateAuthenticationOptions>() ?? new TemplateAuthenticationOptions();

        authenticationBuilder
            .AddTemplateOpenIdConnectAuthentication(templateAuthenticationOptions.Providers.OpenIdConnect)
            .AddTemplateSaml2Authentication(templateAuthenticationOptions.Providers.Saml2)
            .AddTemplateMicrosoftAuthentication(templateAuthenticationOptions.Providers.Microsoft)
            .AddTemplateGoogleAuthentication(templateAuthenticationOptions.Providers.Google);

        return services;
    }

    /// <summary>
    /// Applies template authentication middleware when authentication is enabled.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseTemplateAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        TemplateAuthenticationOptions options = app.ApplicationServices
            .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
            .Value;

        return !options.Enabled ? app : app.UseAuthentication();
    }
}
