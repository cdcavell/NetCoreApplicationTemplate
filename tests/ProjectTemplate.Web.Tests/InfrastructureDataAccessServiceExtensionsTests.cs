using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Extensions;

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

        using ApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        IDbContextFactory<ApplicationDbContext> dbContextFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        using ApplicationDbContext factoryContext = dbContextFactory.CreateDbContext();

        Assert.Equal(SystemCurrentActorAccessor.ActorName, actorAccessor.CurrentActor);
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", factoryContext.Database.ProviderName);
    }
}
