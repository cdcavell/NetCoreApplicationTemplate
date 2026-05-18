using Microsoft.EntityFrameworkCore;
using Template.Infrastructure.Data;
using Template.Web.Accessors;

namespace Template.Web.Extensions;

/// <summary>
/// Provides service registration methods for template data access.
/// </summary>
public static class DataAccessServiceExtensions
{
    /// <summary>
    /// Registers EF Core data access services for the template application.
    /// </summary>
    public static IServiceCollection AddTemplateDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("TemplateDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'TemplateDatabase' was not configured.");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActorAccessor, HttpContextCurrentActorAccessor>();

        services.AddDbContext<TemplateDbContext>(options => options.UseSqlite(connectionString));

        return services;
    }
}
