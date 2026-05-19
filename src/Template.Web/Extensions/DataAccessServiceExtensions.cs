using Microsoft.EntityFrameworkCore;
using Template.Infrastructure.Data;
using Template.Infrastructure.Data.ExternalLogins;
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

        DataAccessOptions dataAccessOptions = configuration
            .GetSection("Template:DataAccess")
            .Get<DataAccessOptions>() ?? new DataAccessOptions();

        string provider = dataAccessOptions.Provider.Trim();
        string connectionStringName = dataAccessOptions.ConnectionStringName.Trim();

        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException(
                "Template data access provider was not configured.");
        }

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new InvalidOperationException(
                "Template data access connection string name was not configured.");
        }

        string connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' was not configured.");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActorAccessor, HttpContextCurrentActorAccessor>();

        services.AddDbContext<TemplateDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
                return;
            }

            throw new InvalidOperationException(
                $"Unsupported template data access provider '{provider}'. Supported providers: Sqlite, SqlServer.");
        });

        services.AddScoped<IExternalLoginAccountResolver, EfCoreExternalLoginAccountResolver>();

        return services;
    }

    private sealed class DataAccessOptions
    {
        public string Provider { get; init; } = "Sqlite";

        public string ConnectionStringName { get; init; } = "TemplateDatabase";
    }
}
