using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Auditing;
using ProjectTemplate.Infrastructure.Data.Entities;
using ProjectTemplate.Infrastructure.Data.Extensions;

namespace ProjectTemplate.Web.Tests;

public sealed class ApplicationSaveChangesPipelineContextIsolationTests
{
    [Fact]
    public async Task SaveChangesAsync_TwoFactoryContextsInOneScope_KeepAuditStateAndReceiptsIsolated()
    {
        string databasePath = Path.Combine(
            Path.GetTempPath(),
            $"ncat-audit-context-isolation-{Guid.NewGuid():N}.db");

        try
        {
            IConfiguration configuration = CreateConfiguration(databasePath);
            ServiceCollection services = new();
            services.AddLogging();
            services.AddScoped<BlockingApplicationAuditStore>();
            services.AddScoped<IApplicationAuditStore>(serviceProvider =>
                serviceProvider.GetRequiredService<BlockingApplicationAuditStore>());
            services.AddApplicationInfrastructureDataAccess(configuration);

            await using ServiceProvider serviceProvider = services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

            IDbContextFactory<ApplicationDbContext> factory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            IApplicationMutationAuditReceiptRegistry receiptRegistry = scope.ServiceProvider
                .GetRequiredService<IApplicationMutationAuditReceiptRegistry>();
            BlockingApplicationAuditStore auditStore = scope.ServiceProvider
                .GetRequiredService<BlockingApplicationAuditStore>();

            await using ApplicationDbContext firstContext = await factory.CreateDbContextAsync(
                TestContext.Current.CancellationToken);
            await using ApplicationDbContext secondContext = await factory.CreateDbContextAsync(
                TestContext.Current.CancellationToken);

            _ = await firstContext.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
            _ = await firstContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            firstContext.ExternalLoginAccounts.Add(CreateAccount("first-context-user"));
            secondContext.ExternalLoginAccounts.Add(CreateAccount("second-context-user"));

            Task<int> firstSave = firstContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            await auditStore.WaitUntilFirstAppendIsBlockedAsync(TestContext.Current.CancellationToken);

            int secondResult = await secondContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            auditStore.ReleaseFirstAppend();
            int firstResult = await firstSave;

            AuditRecord firstAuditRecord = Assert.Single(
                firstContext.ChangeTracker.Entries<AuditRecord>()).Entity;
            AuditRecord secondAuditRecord = Assert.Single(
                secondContext.ChangeTracker.Entries<AuditRecord>()).Entity;
            ApplicationMutationAuditReceipt firstReceipt = Assert.IsType<ApplicationMutationAuditReceipt>(
                receiptRegistry.GetLastCompletedReceipt(firstContext));
            ApplicationMutationAuditReceipt secondReceipt = Assert.IsType<ApplicationMutationAuditReceipt>(
                receiptRegistry.GetLastCompletedReceipt(secondContext));

            Assert.Equal(2, firstResult);
            Assert.Equal(2, secondResult);
            Assert.NotEqual(firstAuditRecord.MutationBatchId, secondAuditRecord.MutationBatchId);
            Assert.Equal(firstAuditRecord.MutationBatchId, firstReceipt.MutationBatchId);
            Assert.Equal(secondAuditRecord.MutationBatchId, secondReceipt.MutationBatchId);
            Assert.Contains("first-context-user", firstAuditRecord.CurrentValues, StringComparison.Ordinal);
            Assert.DoesNotContain("second-context-user", firstAuditRecord.CurrentValues, StringComparison.Ordinal);
            Assert.Contains("second-context-user", secondAuditRecord.CurrentValues, StringComparison.Ordinal);
            Assert.DoesNotContain("first-context-user", secondAuditRecord.CurrentValues, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    private static IConfiguration CreateConfiguration(string databasePath)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ApplicationDatabase"] = $"Data Source={databasePath}",
                    ["ProjectTemplate:DataAccess:Provider"] = "Sqlite",
                    ["ProjectTemplate:DataAccess:ConnectionStringName"] = "ApplicationDatabase",
                    ["ProjectTemplate:DataAccess:Auditing:Enabled"] = "true",
                    ["ProjectTemplate:DataAccess:Auditing:StorageMode"] = "Local"
                })
            .Build();
    }

    private static ExternalLoginAccount CreateAccount(string providerUserId)
    {
        return new ExternalLoginAccount
        {
            LocalUserId = Guid.NewGuid(),
            ProviderName = "GitHub",
            ProviderUserId = providerUserId,
            DisplayName = providerUserId,
            Email = $"{providerUserId}@example.com",
            CreatedOnUtc = new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    private sealed class BlockingApplicationAuditStore : IApplicationAuditStore
    {
        private readonly TaskCompletionSource _firstAppendBlocked = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _releaseFirstAppend = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private int _appendCount;

        public void Append(ApplicationDbContext dbContext, AuditRecord auditRecord)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            ArgumentNullException.ThrowIfNull(auditRecord);
            dbContext.AuditRecords.Add(auditRecord);
        }

        public async ValueTask AppendAsync(
            ApplicationDbContext dbContext,
            AuditRecord auditRecord,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            ArgumentNullException.ThrowIfNull(auditRecord);

            if (Interlocked.Increment(ref _appendCount) == 1)
            {
                _firstAppendBlocked.SetResult();
                await _releaseFirstAppend.Task.WaitAsync(cancellationToken);
            }

            dbContext.AuditRecords.Add(auditRecord);
        }

        public Task WaitUntilFirstAppendIsBlockedAsync(CancellationToken cancellationToken)
        {
            return _firstAppendBlocked.Task.WaitAsync(cancellationToken);
        }

        public void ReleaseFirstAppend()
        {
            _releaseFirstAppend.TrySetResult();
        }
    }
}
