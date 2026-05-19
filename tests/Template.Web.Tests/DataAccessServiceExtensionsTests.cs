using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure.Data;
using Template.Web.Extensions;

namespace Template.Web.Tests;

/// <summary>
/// Provides tests for template data access provider configuration.
/// </summary>
public sealed class DataAccessServiceExtensionsTests
{
    /// <summary>
    /// Verifies that SQLite remains the default provider when no data access provider is explicitly configured.
    /// </summary>
    [Fact]
    public void AddTemplateDataAccess_DefaultConfiguration_UsesSqliteProvider()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:TemplateDatabase"] = "Data Source=:memory:"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        using TemplateDbContext context = serviceProvider.GetRequiredService<TemplateDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
    }

    /// <summary>
    /// Verifies that SQL Server can be selected through configuration.
    /// </summary>
    [Fact]
    public void AddTemplateDataAccess_SqlServerProvider_UsesSqlServerProvider()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:TemplateSqlServer"] = "Server=(localdb)\\MSSQLLocalDB;Database=TemplateApplicationTests;Trusted_Connection=True;TrustServerCertificate=True",
                ["Template:DataAccess:Provider"] = "SqlServer",
                ["Template:DataAccess:ConnectionStringName"] = "TemplateSqlServer"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        using TemplateDbContext context = serviceProvider.GetRequiredService<TemplateDbContext>();

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
    public void AddTemplateDataAccess_ProviderNameMatching_IsCaseInsensitive(
        string provider,
        string expectedProviderName)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:TemplateDatabase"] = "Data Source=:memory:",
                ["ConnectionStrings:TemplateSqlServer"] = "Server=(localdb)\\MSSQLLocalDB;Database=TemplateApplicationTests;Trusted_Connection=True;TrustServerCertificate=True",
                ["Template:DataAccess:Provider"] = provider,
                ["Template:DataAccess:ConnectionStringName"] = provider.Equals("sqlserver", StringComparison.OrdinalIgnoreCase)
                    ? "TemplateSqlServer"
                    : "TemplateDatabase"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        using TemplateDbContext context = serviceProvider.GetRequiredService<TemplateDbContext>();

        Assert.Equal(expectedProviderName, context.Database.ProviderName);
    }

    /// <summary>
    /// Verifies that blank provider values fail fast during service registration.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddTemplateDataAccess_BlankProvider_ThrowsInvalidOperationException(string provider)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:TemplateDatabase"] = "Data Source=:memory:",
                ["Template:DataAccess:Provider"] = provider,
                ["Template:DataAccess:ConnectionStringName"] = "TemplateDatabase"
            });

        ServiceCollection services = new();
        services.AddLogging();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddTemplateDataAccess(configuration));

        Assert.Contains(
            "Template data access provider was not configured.",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that blank connection string names fail fast during service registration.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddTemplateDataAccess_BlankConnectionStringName_ThrowsInvalidOperationException(
        string connectionStringName)
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:TemplateDatabase"] = "Data Source=:memory:",
                ["Template:DataAccess:Provider"] = "Sqlite",
                ["Template:DataAccess:ConnectionStringName"] = connectionStringName
            });

        ServiceCollection services = new();
        services.AddLogging();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddTemplateDataAccess(configuration));

        Assert.Contains(
            "Template data access connection string name was not configured.",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that unsupported provider names fail when the DbContext provider is configured.
    /// </summary>
    [Fact]
    public void AddTemplateDataAccess_UnsupportedProvider_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:TemplateDatabase"] = "Data Source=:memory:",
                ["Template:DataAccess:Provider"] = "PostgreSql",
                ["Template:DataAccess:ConnectionStringName"] = "TemplateDatabase"
            });

        using ServiceProvider serviceProvider = CreateServiceProvider(configuration);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            serviceProvider.GetRequiredService<TemplateDbContext>);

        Assert.Contains(
            "Unsupported template data access provider",
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
    public void AddTemplateDataAccess_MissingConnectionString_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Template:DataAccess:Provider"] = "Sqlite",
                ["Template:DataAccess:ConnectionStringName"] = "MissingDatabase"
            });

        ServiceCollection services = new();
        services.AddLogging();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddTemplateDataAccess(configuration));

        Assert.Contains(
            "Connection string 'MissingDatabase' was not configured.",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddTemplateDataAccess(configuration);

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
