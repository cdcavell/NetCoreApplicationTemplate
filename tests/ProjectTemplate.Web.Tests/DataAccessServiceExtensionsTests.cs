using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.ExternalLogins;
using ProjectTemplate.Infrastructure.Data.Options;
using ProjectTemplate.Web.Extensions;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides tests for application data access provider configuration.
/// </summary>
public sealed class DataAccessServiceExtensionsTests
{
    /// <summary>
    /// Verifies that SQLite remains the default provider when no data access provider is explicitly configured.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_DefaultConfiguration_UsesSqliteProvider()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        using ApplicationDbContext context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
    }

    /// <summary>
    /// Verifies that SQL Server can be selected through configuration.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_SqlServerProvider_UsesSqlServerProvider()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationSqlServer"] = "Server=(localdb)\\MSSQLLocalDB;Database=ApplicationApplicationTests;Trusted_Connection=True;TrustServerCertificate=True",
                ["ProjectTemplate:DataAccess:Provider"] = "SqlServer",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationSqlServer"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        using ApplicationDbContext context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
    }

    /// <summary>
    /// Verifies that provider matching is case-insensitive.
    /// </summary>
    [Theory]
    [InlineData("sqlite", "Microsoft.EntityFrameworkCore.Sqlite")]
    [InlineData("SQLITE", "Microsoft.EntityFrameworkCore.Sqlite")]
    [InlineData("sqlserver", "Microsoft.EntityFrameworkCore.SqlServer")]
    [InlineData("SQLSERVER", "Microsoft.EntityFrameworkCore.SqlServer")]
    public void AddApplicationDataAccess_ProviderNameMatching_IsCaseInsensitive(
        string provider,
        string expectedProviderName)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                ["ConnectionStrings:ApplicationSqlServer"] = "Server=(localdb)\\MSSQLLocalDB;Database=ApplicationApplicationTests;Trusted_Connection=True;TrustServerCertificate=True",
                ["ProjectTemplate:DataAccess:Provider"] = provider,
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = provider.Equals("sqlserver", StringComparison.OrdinalIgnoreCase)
                    ? "ApplicationSqlServer"
                    : "ApplicationDatabase"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        using ApplicationDbContext context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal(expectedProviderName, context.Database.ProviderName);
    }

    /// <summary>
    /// Verifies that data access can be intentionally disabled without requiring EF Core services.
    /// </summary>
    [Theory]
    [InlineData("None")]
    [InlineData("none")]
    [InlineData("Disabled")]
    [InlineData("disabled")]
    public void AddApplicationDataAccess_DisabledProvider_SkipsEfCoreRegistrations(string provider)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ProjectTemplate:DataAccess:Provider"] = provider
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        Assert.Null(serviceProvider.GetService<ApplicationDbContext>());
        Assert.Null(serviceProvider.GetService<IDbContextFactory<ApplicationDbContext>>());
        Assert.Null(serviceProvider.GetService<IExternalLoginAccountResolver>());
    }

    /// <summary>
    /// Verifies that disabled data access does not require a configured connection string.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_DisabledProvider_DoesNotRequireConnectionStringName()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ProjectTemplate:DataAccess:Provider"] = "None",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = string.Empty
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        DataAccessOptions options = serviceProvider
            .GetRequiredService<IOptions<DataAccessOptions>>()
            .Value;

        Assert.Equal("None", options.Provider);
        Assert.Null(serviceProvider.GetService<ApplicationDbContext>());
    }

    /// <summary>
    /// Verifies that blank provider values fail fast during service registration.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddApplicationDataAccess_BlankProvider_ThrowsInvalidOperationException(string provider)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                ["ProjectTemplate:DataAccess:Provider"] = provider,
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationDatabase"
            });

        ServiceCollection services = new();
        services.AddLogging();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddApplicationDataAccess(configuration));

        Assert.Contains(
            "Application data access provider was not configured.",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that blank connection string names fail fast during service registration.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddApplicationDataAccess_BlankConnectionStringName_ThrowsInvalidOperationException(
        string connectionStringName)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                ["ProjectTemplate:DataAccess:Provider"] = "Sqlite",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = connectionStringName
            });

        ServiceCollection services = new();
        services.AddLogging();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddApplicationDataAccess(configuration));

        Assert.Contains(
            "Application data access connection string name was not configured.",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that unsupported provider names fail when the DbContext provider is configured.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_UnsupportedProvider_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                ["ProjectTemplate:DataAccess:Provider"] = "PostgreSql",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationDatabase"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            serviceProvider.GetRequiredService<ApplicationDbContext>);

        Assert.Contains(
            "Unsupported data access provider",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "Sqlite, SqlServer",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that a configured provider fails fast when its configured connection string is missing.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_MissingConnectionString_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ProjectTemplate:DataAccess:Provider"] = "Sqlite",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = "MissingDatabase"
            });

        ServiceCollection services = new();
        services.AddLogging();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddApplicationDataAccess(configuration));

        Assert.Contains(
            "Connection string 'MissingDatabase' was not configured.",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that EF Core audit record creation is enabled when configured.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_AuditingEnabled_BindsAuditOptions()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                ["ProjectTemplate:DataAccess:Provider"] = "Sqlite",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationDatabase",
                ["ProjectTemplate:DataAccess:Auditing:Enabled"] = "true"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        DataAccessOptions options = serviceProvider
            .GetRequiredService<IOptions<DataAccessOptions>>()
            .Value;

        Assert.True(options.Auditing.Enabled);
    }

    /// <summary>
    /// Verifies that EF Core audit record creation can be disabled through configuration.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_AuditingDisabled_BindsAuditOptions()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                ["ProjectTemplate:DataAccess:Provider"] = "Sqlite",
                ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationDatabase",
                ["ProjectTemplate:DataAccess:Auditing:Enabled"] = "false"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        DataAccessOptions options = serviceProvider
            .GetRequiredService<IOptions<DataAccessOptions>>()
            .Value;

        Assert.False(options.Auditing.Enabled);
    }

    /// <summary>
    /// Verifies that EF Core audit record creation is enabled by default when not explicitly configured.
    /// </summary>
    [Fact]
    public void AddApplicationDataAccess_AuditingNotConfigured_DefaultsToEnabled()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        DataAccessOptions options = serviceProvider
            .GetRequiredService<IOptions<DataAccessOptions>>()
            .Value;

        Assert.True(options.Auditing.Enabled);
    }

    private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddApplicationDataAccess(configuration);

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration(
        IReadOnlyDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
