# EF Core Save Pipeline

The template uses a composite EF Core `SaveChangesInterceptor` to invoke the application-owned save pipeline.

The default interceptor is `ApplicationSaveChangesInterceptor`. It delegates save preparation to `IApplicationSaveChangesPipeline` during the EF Core save lifecycle instead of placing the cross-cutting mutation logic directly inside `ApplicationDbContext` save overrides.

`ApplicationDbContext` requires `IApplicationSaveChangesPipeline` as a non-null dependency. Missing registration should fail during service resolution instead of falling back to an internally constructed pipeline.

## Default Pipeline Order

The default `ApplicationSaveChangesPipeline` preserves the following order:

1. persisted string canonicalization
2. lookup string normalization
3. timestamp normalization
4. application-managed concurrency stamp generation
5. audit entry creation
6. after-save audit completion for database-generated values

This order is intentional. Lookup values depend on canonicalized display values. Audit records should capture normalized timestamps and updated concurrency stamps. Audit records that depend on database-generated values must be completed after the primary save succeeds.

## Why the Interceptor Stays Composite

The template intentionally keeps one composite save interceptor rather than separate interceptors for each concern.

Separate interceptors could look cleaner in isolation, but they would make save behavior depend more heavily on registration order. That would make auditing and current-value capture easier to break accidentally.

The composite interceptor keeps the lifecycle easy to reason about:

- `SavingChanges` and `SavingChangesAsync` run the before-save pipeline
- EF Core performs the primary save
- `SavedChanges` and `SavedChangesAsync` complete any pending audit records that require generated values
- audit records are guarded from recursive auditing by the pipeline

## Extension Seams

Applications that need different save behavior should usually replace or decorate `IApplicationSaveChangesPipeline`.

Use a replacement pipeline when the consuming application wants to fully own the save-preparation behavior.

Use a decorator when the consuming application wants to add behavior before or after the default template pipeline while preserving the baseline order.

For audit destination changes, prefer `IApplicationAuditStore` rather than replacing the full save pipeline. The audit store seam is intended for local, outbox, or external sink audit delivery while preserving the rest of the save lifecycle.

## Recommended Extension Pattern

A consuming application can register its own pipeline implementation before constructing `ApplicationDbContext` instances:

```csharp
services.AddScoped<IApplicationSaveChangesPipeline, MyApplicationSaveChangesPipeline>();
```

For audit storage changes, register a custom audit store:

```csharp
services.AddScoped<IApplicationAuditStore, MyApplicationAuditStore>();
```

Then configure the desired audit storage mode:

```json
"ProjectTemplate": {
  "DataAccess": {
    "Auditing": {
      "Enabled": true,
      "StorageMode": "ExternalSink"
    }
  }
}
```

## Concurrency Token Portability

The template uses an application-managed `ConcurrencyStamp` by default. This keeps the model portable across SQLite development and SQL Server deployments.

SQL Server-only applications may choose to replace this pattern with a provider-native `rowversion` column, but that is not the default because the template is intended to remain provider-portable.

## When to Split Later

The composite interceptor should only be split into specialized interceptors if a real maintenance or consumer-extension need emerges.

Reasonable revisit signals include:

- consumers need to disable one save concern without replacing the full pipeline
- audit behavior grows complex enough to require its own isolated lifecycle
- provider-specific behavior cannot remain cleanly isolated inside the pipeline
- test maintenance becomes harder because the pipeline has too many unrelated responsibilities
- a real consuming application needs independent ordering or replacement of a specific save concern
