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
            .Validate(options => DataAccessOptions.IsDisabledProvider(options.Provider)
                || !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                "ProjectTemplate:DataAccess:ConnectionStringName must not be empty when data access is enabled.")
            .ValidateOnStart();

        DataAccessRegistration registration = ResolveDataAccessRegistration(configuration);

        if (registration.IsDisabled)
        {
            return services;
        }

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

        if (DataAccessOptions.IsDisabledProvider(provider))
        {
            return DataAccessRegistration.Disabled(provider);
        }

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new InvalidOperationException(
                "Application data access connection string name was not configured.");
        }

        string connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' was not configured.");

        return DataAccessRegistration.Enabled(
            provider,
            connectionString);
    }

    private static void ConfigureProvider(
        DbContextOptionsBuilder options,
        string provider,
        string connectionString)
    {
        if (provider.Equals(DataAccessOptions.SqliteProvider, StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(connectionString);
            return;
        }

        if (provider.Equals(DataAccessOptions.SqlServerProvider, StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlServer(connectionString);
            return;
        }

        throw new InvalidOperationException(
            $"Unsupported data access provider '{provider}'. Supported providers: {DataAccessOptions.SqliteProvider}, {DataAccessOptions.SqlServerProvider}, {DataAccessOptions.DisabledProvider}.");
    }

    private readonly record struct DataAccessRegistration(
        string Provider,
        string ConnectionString,
        bool IsDisabled)
    {
        public static DataAccessRegistration Enabled(
            string provider,
            string connectionString)
        {
            return new DataAccessRegistration(provider, connectionString, false);
        }

        public static DataAccessRegistration Disabled(
            string provider)
        {
            return new DataAccessRegistration(provider, string.Empty, true);
        }
    }
}
