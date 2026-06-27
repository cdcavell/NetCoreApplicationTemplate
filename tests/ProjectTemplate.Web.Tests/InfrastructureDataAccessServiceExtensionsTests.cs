using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Extensions;
using ProjectTemplate.Infrastructure.Data.ExternalLogins;

namespace ProjectTemplate.Web.Tests;

public sealed class InfrastructureDataAccessServiceExtensionsTests
{
    [Fact]
    public void AddApplicationInfrastructureDataAccess_NonWebContainer_ResolvesDbContextAndFactory()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ApplicationDatabase"] = "Data Source=:memory:",
                    ["ProjectTemplate:DataAccess:Provider"] = "Sqlite",
                    ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationDatabase"
                })
            .Build();

        ServiceCollection services = new();

        services.AddLogging();
        services.AddApplicationInfrastructureDataAccess(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();

        ICurrentActorAccessor actorAccessor = scope.ServiceProvider
            .GetRequiredService<ICurrentActorAccessor>();

        IApplicationAuditStore auditStore = scope.ServiceProvider
            .GetRequiredService<IApplicationAuditStore>();

        using ApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        IDbContextFactory<ApplicationDbContext> dbContextFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        using ApplicationDbContext factoryContext = dbContextFactory.CreateDbContext();

        Assert.Equal(SystemCurrentActorAccessor.ActorName, actorAccessor.CurrentActor);
        Assert.IsType<LocalApplicationAuditStore>(auditStore);
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", factoryContext.Database.ProviderName);
    }

    [Fact]
    public void AddApplicationInfrastructureDataAccess_DisabledProvider_SkipsDbContextFactoryAndEfBackedServices()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ProjectTemplate:DataAccess:Provider"] = "None"
                })
            .Build();

        ServiceCollection services = new();

        services.AddLogging();
        services.AddApplicationInfrastructureDataAccess(configuration);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.Null(serviceProvider.GetService<ICurrentActorAccessor>());
        Assert.Null(serviceProvider.GetService<IApplicationAuditStore>());
        Assert.Null(serviceProvider.GetService<ApplicationDbContext>());
        Assert.Null(serviceProvider.GetService<IDbContextFactory<ApplicationDbContext>>());
        Assert.Null(serviceProvider.GetService<IExternalLoginAccountResolver>());
    }
}
