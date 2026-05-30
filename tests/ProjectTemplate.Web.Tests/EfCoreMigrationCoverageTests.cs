using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Migrations;

namespace ProjectTemplate.Web.Tests;

public sealed class EfCoreMigrationCoverageTests
{
    private const string _migrationProvider = "Microsoft.EntityFrameworkCore.Sqlite";

    private const string _auditRecordEntityName =
        "ProjectTemplate.Infrastructure.Data.Entities.AuditRecord";

    private const string _externalLoginAccountEntityName =
        "ProjectTemplate.Infrastructure.Data.Entities.ExternalLoginAccount";

    public static TheoryData<string, int, int> MigrationOperationCases => new()
    {
        { nameof(InitialCreate), 5, 2 },
        { nameof(AddDataEntityConcurrencyStamp), 2, 2 },
        { nameof(AddExternalLoginAccountNormalizedLookupColumns), 5, 5 },
        { nameof(StandardizeTimestampPersistence), 0, 0 }
    };

    public static TheoryData<string, bool, bool, int?> MigrationModelCases => new()
    {
        { nameof(InitialCreate), false, false, null },
        { nameof(AddDataEntityConcurrencyStamp), true, false, null },
        { nameof(AddExternalLoginAccountNormalizedLookupColumns), true, true, null },
        { nameof(StandardizeTimestampPersistence), true, true, 3 }
    };

    [Theory]
    [MemberData(nameof(MigrationOperationCases))]
    public void Migration_UpAndDown_DefinesExpectedOperations(
        string migrationName,
        int expectedUpOperations,
        int expectedDownOperations)
    {
        Migration migration = CreateMigration(migrationName);

        MigrationBuilder upBuilder = new(_migrationProvider);

        InvokeMigrationMethod(migration, "Up", upBuilder);

        Assert.Equal(expectedUpOperations, upBuilder.Operations.Count);

        MigrationBuilder downBuilder = new(_migrationProvider);

        InvokeMigrationMethod(migration, "Down", downBuilder);

        Assert.Equal(expectedDownOperations, downBuilder.Operations.Count);
    }

    [Theory]
    [MemberData(nameof(MigrationModelCases))]
    public void Migration_TargetModel_PreservesExpectedPersistenceShape(
        string migrationName,
        bool expectsConcurrencyStamp,
        bool expectsNormalizedLookupColumns,
        int? expectedTimestampPrecision)
    {
        Migration migration = CreateMigration(migrationName);

        IModel model = migration.TargetModel;

        IEntityType? auditRecord = model.FindEntityType(_auditRecordEntityName);
        IEntityType? externalLoginAccount = model.FindEntityType(_externalLoginAccountEntityName);

        Assert.NotNull(auditRecord);
        Assert.NotNull(externalLoginAccount);

        Assert.Equal(
            expectsConcurrencyStamp,
            auditRecord!.FindProperty("ConcurrencyStamp") is not null);

        Assert.Equal(
            expectsConcurrencyStamp,
            externalLoginAccount!.FindProperty("ConcurrencyStamp") is not null);

        Assert.Equal(
            expectsNormalizedLookupColumns,
            externalLoginAccount.FindProperty("NormalizedEmail") is not null);

        Assert.Equal(
            expectsNormalizedLookupColumns,
            externalLoginAccount.FindProperty("NormalizedProviderName") is not null);

        IProperty? createdOnUtc = externalLoginAccount.FindProperty("CreatedOnUtc");
        IProperty? modifiedOnUtc = auditRecord.FindProperty("ModifiedOnUtc");

        Assert.NotNull(createdOnUtc);
        Assert.NotNull(modifiedOnUtc);

        Assert.Equal(expectedTimestampPrecision, createdOnUtc!.GetPrecision());
        Assert.Equal(expectedTimestampPrecision, modifiedOnUtc!.GetPrecision());
    }
    [Fact]
    public void ApplicationDbContextModelSnapshot_Model_PreservesCurrentPersistenceShape()
    {
        Type snapshotType = typeof(ApplicationDbContext).Assembly.GetType(
            "ProjectTemplate.Infrastructure.Data.Migrations.ApplicationDbContextModelSnapshot",
            throwOnError: true)!;

        var snapshot = (ModelSnapshot)Activator.CreateInstance(
            snapshotType,
            nonPublic: true)!;

        IModel model = snapshot.Model;

        IEntityType? auditRecord = model.FindEntityType(_auditRecordEntityName);
        IEntityType? externalLoginAccount = model.FindEntityType(_externalLoginAccountEntityName);

        Assert.NotNull(auditRecord);
        Assert.NotNull(externalLoginAccount);

        Assert.NotNull(auditRecord!.FindProperty("ConcurrencyStamp"));
        Assert.NotNull(externalLoginAccount!.FindProperty("ConcurrencyStamp"));
        Assert.NotNull(externalLoginAccount.FindProperty("NormalizedEmail"));
        Assert.NotNull(externalLoginAccount.FindProperty("NormalizedProviderName"));

        Assert.Equal(3, auditRecord.FindProperty("ModifiedOnUtc")!.GetPrecision());
        Assert.Equal(3, externalLoginAccount.FindProperty("CreatedOnUtc")!.GetPrecision());
        Assert.Equal(3, externalLoginAccount.FindProperty("UpdatedOnUtc")!.GetPrecision());
        Assert.Equal(3, externalLoginAccount.FindProperty("LastLoginOnUtc")!.GetPrecision());

        IIndex? normalizedProviderIndex = externalLoginAccount
            .GetIndexes()
            .SingleOrDefault(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(["NormalizedProviderName", "ProviderUserId"]));

        Assert.NotNull(normalizedProviderIndex);
        Assert.True(normalizedProviderIndex!.IsUnique);
    }

    private static void InvokeMigrationMethod(
        Migration migration,
        string methodName,
        MigrationBuilder migrationBuilder)
    {
        MethodInfo? method = migration.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);

        _ = method.Invoke(migration, [migrationBuilder]);
    }

    private static Migration CreateMigration(string migrationName)
    {
        return migrationName switch
        {
            nameof(InitialCreate) => new InitialCreate(),
            nameof(AddDataEntityConcurrencyStamp) => new AddDataEntityConcurrencyStamp(),
            nameof(AddExternalLoginAccountNormalizedLookupColumns) => new AddExternalLoginAccountNormalizedLookupColumns(),
            nameof(StandardizeTimestampPersistence) => new StandardizeTimestampPersistence(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(migrationName),
                migrationName,
                "Unknown migration name.")
        };
    }
}
