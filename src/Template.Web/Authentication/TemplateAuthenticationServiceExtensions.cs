using Template.Web.Options;

namespace Template.Web.Authentication;

/// <summary>
/// Provides extension methods for registering template authentication services.
/// </summary>
public static class TemplateAuthenticationServiceExtensions
{
    /// <summary>
    /// Adds template authentication configuration and baseline authentication services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddTemplateAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<TemplateAuthenticationOptions>()
            .Bind(configuration.GetSection(TemplateAuthenticationOptions.SectionName))
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.DefaultScheme),
                "Template:Authentication:DefaultScheme is required when authentication is enabled.")
            .ValidateOnStart();

        services.AddAuthorization();

        services
            .AddAuthentication()
            .AddCookie(TemplateAuthenticationDefaults.CookieScheme);

        return services;
    }
}
