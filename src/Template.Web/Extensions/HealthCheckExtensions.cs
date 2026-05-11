using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods for registering and mapping template health check endpoints.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers baseline ASP.NET Core health check services.
    /// </summary>
    /// <param name="services">The service collection used to register health checks.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTemplateHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Maps baseline health check endpoints for infrastructure, reverse proxies, and hosting platforms.
    /// </summary>
    /// <param name="app">The web application used to map health check endpoints.</param>
    /// <returns>The original <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication MapTemplateHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        return app;
    }
}
