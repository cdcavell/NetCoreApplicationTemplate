using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Extensions;
using ProjectTemplate.Infrastructure.Data.Services;
using ProjectTemplate.Web.Accessors;

namespace ProjectTemplate.Web.Extensions;

/// <summary>
/// Provides service registration methods for application data access.
/// </summary>
public static class DataAccessServiceExtensions
{
    /// <summary>
    /// Registers EF Core data access services for the web application.
    /// </summary>
    public static IServiceCollection AddApplicationDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActorAccessor, HttpContextCurrentActorAccessor>();

        services.AddApplicationInfrastructureDataAccess(configuration);

        services.AddHostedService<DataAccessStartupLogger>();

        return services;
    }
}
