using Template.Web.Middleware;
using Template.Web.Options;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to register and enable security headers functionality.
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Registers the <see cref="TemplateSecurityHeadersOptions"/> configuration section with the DI container.
    /// </summary>
    /// <param name="services">The service collection to add the configuration to.</param>
    /// <param name="configuration">The application configuration containing the "SecurityHeaders" section.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTemplateSecurityHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<TemplateSecurityHeadersOptions>()
            .Bind(configuration.GetSection(TemplateSecurityHeadersOptions.SectionName))
            .Validate(
                options =>
                    !options.EnableContentSecurityPolicy ||
                    !string.IsNullOrWhiteSpace(options.ContentSecurityPolicy),
                "Template:SecurityHeaders:ContentSecurityPolicy is required when CSP is enabled.")
            .Validate(
                options =>
                    !options.EnablePermissionsPolicy ||
                    !string.IsNullOrWhiteSpace(options.PermissionsPolicy),
                "Template:SecurityHeaders:PermissionsPolicy is required when Permissions-Policy is enabled.")
            .Validate(
                options =>
                    options.ExcludedPathPrefixes.All(path =>
                        !string.IsNullOrWhiteSpace(path) &&
                        path.StartsWith('/')),
                "Template:SecurityHeaders:ExcludedPathPrefixes values must start with '/'.")
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Adds the security headers middleware to the application's request pipeline.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    /// <returns>The original <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseTemplateSecurityHeaders(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}

