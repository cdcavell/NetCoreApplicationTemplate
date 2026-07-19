# DbContext Audit State Isolation

NCAT isolates mutable SaveChanges audit state by `ApplicationDbContext` instance, even when one dependency-injection scope creates several contexts through `IDbContextFactory<ApplicationDbContext>`.

Each context receives a dedicated internal `ApplicationSaveChangesPipeline` state machine for:

- pending audit entries that contain temporary values;
- retained audit records used to build the canonical mutation manifest;
- active audit context and mutation-batch identity;
- audit record counts; and
- the most recently completed mutation receipt.

The scoped coordinator uses weak context keys, so disposing a factory-created context does not leave that context permanently retained by the scope.

## Receipt access

`IApplicationMutationAuditReceiptAccessor` is context-bound when resolved from dependency injection. It reports the receipt for the scoped `ApplicationDbContext` used by `IApplicationAuditedTransaction`.

Factory-created contexts can be queried explicitly through `IApplicationMutationAuditReceiptRegistry`:

```csharp
await using ApplicationDbContext first = await factory.CreateDbContextAsync(cancellationToken);
await using ApplicationDbContext second = await factory.CreateDbContextAsync(cancellationToken);

await first.SaveChangesAsync(cancellationToken);
await second.SaveChangesAsync(cancellationToken);

ApplicationMutationAuditReceipt? firstReceipt =
    receiptRegistry.GetLastCompletedReceipt(first);
ApplicationMutationAuditReceipt? secondReceipt =
    receiptRegistry.GetLastCompletedReceipt(second);
```

The registry returns `null` until the specified context completes an audited save. A receipt produced by another context in the same scope is never returned for the requested context.

## Concurrency boundary

Different `ApplicationDbContext` instances in one scope may save independently without sharing pending audit state or mutation receipts.

EF Core does not support concurrent operations on the same `DbContext` instance. This isolation feature does not change that rule. Await each operation on a context before starting another operation on that same context.

The configured `IApplicationAuditStore`, audit context accessor, actor accessor, value policy, manifest builder, and manifest hasher remain shared according to their registered lifetimes. Custom scoped implementations that maintain mutable state must therefore provide their own concurrency safety when multiple contexts use them simultaneously.
