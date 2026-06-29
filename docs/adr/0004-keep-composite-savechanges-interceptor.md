# ADR 0004: Keep the Composite EF Core SaveChanges Interceptor

## Status

Accepted

## Context

The EF Core save pipeline has been moved out of the heavy `ApplicationDbContext` save overrides and into a composite `ApplicationSaveChangesInterceptor` backed by `IApplicationSaveChangesPipeline`.

The pipeline currently coordinates several ordered persistence concerns:

1. persisted string canonicalization
2. lookup string normalization
3. timestamp normalization
4. application-managed concurrency stamp generation
5. audit entry creation and after-save audit completion

Issue #322 evaluated whether this composite interceptor should immediately be split into specialized interceptors such as string canonicalization, lookup normalization, timestamp normalization, concurrency stamp, and audit interceptors.

## Decision

Keep the composite `ApplicationSaveChangesInterceptor` as the default implementation for now.

Do not split the save behavior into multiple specialized interceptors until there is a concrete consumer or maintenance need that outweighs the ordering and audit-safety risks.

## Rationale

A composite interceptor keeps save behavior easier to reason about because the pipeline has strict ordering requirements:

- display values must be canonicalized before lookup values are normalized
- timestamps must be normalized before audit records are created
- concurrency stamps must be updated before audit records capture current values
- audit records with temporary values must be completed after EF Core persists the primary entities

Splitting the behavior into multiple interceptors would make that ordering less obvious and could introduce hidden dependency between interceptor registration order and persisted data correctness.

Auditing is the most sensitive part of the pipeline. Keeping audit preparation and after-save completion behind one pipeline preserves the current guardrails:

- audit records are not recursively audited
- local audit records remain part of the same EF Core save flow
- generated keys can be captured after the primary save
- SQLite development and SQL Server deployment behavior remain aligned

## Extension Model

Applications should prefer replacing or decorating `IApplicationSaveChangesPipeline` rather than splitting the interceptor by default.

Replacement is appropriate when a consuming application wants to fully own save preparation behavior.

Decoration is appropriate when a consuming application wants to add behavior before or after the template pipeline while preserving the baseline order.

Audit storage extension should continue to use `IApplicationAuditStore` and `ProjectTemplate:DataAccess:Auditing:StorageMode` when the goal is to change where audit records are written without changing the rest of the save pipeline.

## Consequences

Positive consequences:

- save behavior remains centralized and deterministic
- audit behavior remains guarded against recursion and lifecycle-order bugs
- consumer extension has a clear seam through `IApplicationSaveChangesPipeline`
- the template avoids architectural splitting for its own sake

Tradeoffs:

- individual concerns are not independently replaceable as separate EF Core interceptors yet
- consumers that need fine-grained replacement must replace or decorate the pipeline rather than swapping one small interceptor
- a future split may still be worthwhile if real-world consumers need independent ordering, disabling, or replacement of specific save concerns

## Revisit Criteria

Revisit this decision if one or more of the following becomes true:

- consumers need to disable one save concern without replacing the full pipeline
- audit storage or audit lifecycle logic becomes too complex for the composite pipeline
- new providers require provider-specific save behavior that cannot remain cleanly isolated
- tests become difficult to maintain because too many unrelated concerns are coupled inside the pipeline
- a real extension scenario proves that separate interceptors are safer than a composite pipeline
