using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectTemplate.Infrastructure.Data.ExternalLogins;
using ProjectTemplate.Infrastructure.Data.Options;

namespace ProjectTemplate.Infrastructure.Data.Extensions;

/// <summary>
/// Provides infrastructure-owned service registration methods for application data access.
/// </summary>
public static class InfrastructureDataAccessServiceExtensions
{
    /// <summary>
    /// Registers EF Core data access services for infrastructure and non-web consumers.
    /// </summary>
    public static IServiceCollection AddApplicationInfrastructureDataAccess(
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

        DataAccessRegistration registration = ResolveDataAccessRegistration(configuration);

        services.TryAddScoped<ICurrentActorAccessor, SystemCurrentActorAccessor>();

        services.AddDbContext<ApplicationDbContext>(options => ConfigureProvider(
                options,
                registration.Provider,
                registration.ConnectionString));

        services.AddDbContextFactory<ApplicationDbContext>(
            options => ConfigureProvider(
                    options,
                    registration.Provider,
                    registration.ConnectionString),
            ServiceLifetime.Scoped);

        services.TryAddScoped<IExternalLoginAccountResolver, EfCoreExternalLoginAccountResolver>();

        return services;
    }

    private static DataAccessRegistration ResolveDataAccessRegistration(
        IConfiguration configuration)
    {
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

        return new DataAccessRegistration(
            provider,
            connectionString);
    }

    private static void ConfigureProvider(
        DbContextOptionsBuilder options,
        string provider,
        string connectionString)
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
    }

    private readonly record struct DataAccessRegistration(
        string Provider,
        string ConnectionString);
}
