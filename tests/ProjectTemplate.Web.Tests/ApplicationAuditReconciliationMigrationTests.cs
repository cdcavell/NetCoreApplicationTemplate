using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Migrations;

namespace ProjectTemplate.Web.Tests;

public sealed class ApplicationAuditReconciliationMigrationTests
{
    private const string _provider = "Microsoft.EntityFrameworkCore.Sqlite";

    [Fact]
    public void Migration_UpAndDown_DefinesFindingAndRemediationPersistence()
    {
        var migration = new AddApplicationAuditReconciliation();
        var upBuilder = new MigrationBuilder(_provider);
        Invoke(migration, "Up", upBuilder);
        var downBuilder = new MigrationBuilder(_provider);
        Invoke(migration, "Down", downBuilder);

        Assert.Equal(7, upBuilder.Operations.Count);
        Assert.Equal(2, downBuilder.Operations.Count);
    }

    [Fact]
    public void ModelSnapshot_ContainsReconciliationEvidenceShape()
    {
        Type snapshotType = typeof(ApplicationDbContext).Assembly.GetType(
            "ProjectTemplate.Infrastructure.Data.Migrations.ApplicationDbContextModelSnapshot",
            throwOnError: true)!;
        var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType, nonPublic: true)!;
        IEntityType finding = Assert.IsAssignableFrom<IEntityType>(snapshot.Model.FindEntityType(
            "ProjectTemplate.Infrastructure.Data.Entities.ApplicationAuditReconciliationFinding"));
        IEntityType remediation = Assert.IsAssignableFrom<IEntityType>(snapshot.Model.FindEntityType(
            "ProjectTemplate.Infrastructure.Data.Entities.ApplicationAuditReconciliationRemediation"));

        Assert.Equal("ApplicationAuditReconciliationFindings", finding.GetTableName());
        Assert.NotNull(finding.FindProperty("FindingKey"));
        Assert.Contains(finding.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(["FindingKey"]));
        Assert.Equal("ApplicationAuditReconciliationRemediations", remediation.GetTableName());
        Assert.Equal(256, remediation.FindProperty("EvidenceReference")!.GetMaxLength());
    }

    private static void Invoke(Migration migration, string methodName, MigrationBuilder builder)
    {
        MethodInfo method = Assert.IsAssignableFrom<MethodInfo>(migration.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic));
        _ = method.Invoke(migration, [builder]);
    }
}
