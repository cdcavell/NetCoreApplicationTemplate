using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.ExternalLogins;
using ProjectTemplate.Infrastructure.Data.Options;
using ProjectTemplate.Infrastructure.Data.Services;
using ProjectTemplate.Web.Accessors;

namespace ProjectTemplate.Web.Extensions;

/// <summary>
/// Provides service registration methods for application data access.
/// </summary>
public static class DataAccessServiceExtensions
{
    /// <summary>
    /// Registers EF Core data access services for the application.
    /// </summary>
    public static IServiceCollection AddApplicationDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<DataAccessOptions>()
            .Bind(configuration.GetSection(DataAccessOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Provider),
                "ProjectTemplate:DataAccess:Provider must not be empty.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                "ProjectTemplate:DataAccess:ConnectionStringName must not be empty.")
            .ValidateOnStart();

        DataAccessOptions dataAccessOptions = configuration
            .GetSection(DataAccessOptions.SectionName)
            .Get<DataAccessOptions>() ?? new DataAccessOptions();

        string provider = dataAccessOptions.Provider?.Trim() ?? string.Empty;
        string connectionStringName = dataAccessOptions.ConnectionStringName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException(
                "Application data access provider was not configured.");
        }

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new InvalidOperationException(
                "Application data access connection string name was not configured.");
        }

        string connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' was not configured.");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActorAccessor, HttpContextCurrentActorAccessor>();

        services.AddDbContext<ApplicationDbContext>(options =>
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
                $"Unsupported data access provider '{provider}'. Supported providers: Sqlite, SqlServer.");
        });

        services.AddScoped<IExternalLoginAccountResolver, EfCoreExternalLoginAccountResolver>();
        services.AddHostedService<DataAccessStartupLogger>();

        return services;
    }
}
