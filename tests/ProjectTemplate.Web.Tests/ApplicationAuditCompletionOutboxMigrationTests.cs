using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Migrations;

namespace ProjectTemplate.Web.Tests;

public sealed class ApplicationAuditCompletionOutboxMigrationTests
{
    private const string _provider = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string _entityName =
        "ProjectTemplate.Infrastructure.Data.Entities.ApplicationAuditCompletionOutboxEntry";

    [Fact]
    public void Migration_UpAndDown_DefinesTableAndIndexes()
    {
        var migration = new AddApplicationAuditCompletionOutbox();
        var upBuilder = new MigrationBuilder(_provider);
        Invoke(migration, "Up", upBuilder);
        var downBuilder = new MigrationBuilder(_provider);
        Invoke(migration, "Down", downBuilder);

        Assert.Equal(5, upBuilder.Operations.Count);
        Assert.Equal(1, downBuilder.Operations.Count);
    }

    [Fact]
    public void Migration_TargetModel_ContainsOutboxIdentityAndSchedulingIndexes()
    {
        IModel model = new AddApplicationAuditCompletionOutbox().TargetModel;
        IEntityType entity = Assert.IsAssignableFrom<IEntityType>(model.FindEntityType(_entityName));

        Assert.NotNull(entity.FindProperty("IdempotencyKey"));
        Assert.NotNull(entity.FindProperty("MutationBatchId"));
        Assert.NotNull(entity.FindProperty("Destination"));
        Assert.NotNull(entity.FindProperty("NextAttemptUtc"));
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["Destination", "MutationBatchId"]));
        Assert.Contains(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["Status", "NextAttemptUtc"]));
    }

    [Fact]
    public void ModelSnapshot_ContainsDurableOutboxShape()
    {
        Type snapshotType = typeof(ApplicationDbContext).Assembly.GetType(
            "ProjectTemplate.Infrastructure.Data.Migrations.ApplicationDbContextModelSnapshot",
            throwOnError: true)!;
        var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType, nonPublic: true)!;
        IEntityType entity = Assert.IsAssignableFrom<IEntityType>(snapshot.Model.FindEntityType(_entityName));

        Assert.Equal("ApplicationAuditCompletionOutbox", entity.GetTableName());
        Assert.Equal(3, entity.FindProperty("CreatedUtc")!.GetPrecision());
        Assert.Equal(512, entity.FindProperty("LastErrorMessage")!.GetMaxLength());
    }

    private static void Invoke(Migration migration, string methodName, MigrationBuilder builder)
    {
        MethodInfo method = Assert.IsAssignableFrom<MethodInfo>(migration.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic));
        _ = method.Invoke(migration, [builder]);
    }
}
