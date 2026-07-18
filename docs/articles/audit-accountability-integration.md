# Audit Accountability Integration

NCAT records application data mutations. A companion archive, SIEM, governance service, or verification tool may record why an operation occurred, how it progressed, or whether the retained mutation records still correspond to the receipt produced when the save completed.

These records should remain distinct and be joined through shared opaque identifiers rather than copied into one another.

## Record ownership

| Record | Authoritative question |
| --- | --- |
| NCAT `AuditRecord` | What persisted application state changed? |
| Host decision or workflow record | Why was the operation allowed or initiated? |
| Host execution lifecycle record | Did the operation execute, and which NCAT mutation batch resulted? |
| Canonical mutation manifest digest | Do the retained protected audit records correspond to the completed NCAT receipt? |

NCAT does not require a governance product, SIEM vendor, archive provider, or external audit framework. The integration contracts remain host-owned and framework-neutral.

## Shared audit context

`IApplicationAuditContextAccessor` supplies structured identity and correlation data to the EF Core save pipeline.

```csharp
ApplicationAuditContext context = new(
    actorId: "user-123",
    actorType: ApplicationAuditActorTypes.Human,
    actorDisplayName: "User 123",
    operationExecutionId: "operation-456",
    executionAttemptId: "attempt-1",
    correlationId: "correlation-789",
    traceId: Activity.Current?.TraceId.ToString(),
    spanId: Activity.Current?.SpanId.ToString(),
    decisionAuditRecordId: "decision-abc");
```

The web host registers `HttpContextApplicationAuditContextAccessor`. It resolves authenticated subject or name-identifier claims, request correlation, and current trace identifiers. Infrastructure-only consumers receive a system context by default.

Applications may replace the accessor for background jobs, message handlers, deployment gates, scheduled tasks, or governed operations.

## Mutation batches and receipts

Every logical `SaveChanges` or `SaveChangesAsync` mutation set receives one `MutationBatchId`. All NCAT audit records produced by that save share the same identifier.

After the save completes, `IApplicationMutationAuditReceiptAccessor.LastCompletedReceipt` exposes a minimized receipt containing:

- mutation batch ID;
- audit record count;
- persistence outcome;
- completion timestamp;
- canonical manifest schema version;
- manifest hash algorithm and digest;
- operation execution and attempt identifiers;
- decision audit record identifier;
- correlation and trace identifiers.

The receipt intentionally excludes entity keys and original/current values. A host lifecycle record can safely retain the opaque identifiers, count, algorithm, and digest without copying audited entity values outside NCAT.

## Canonical privacy-safe mutation manifest

`IApplicationMutationManifestBuilder` creates a versioned, provider-neutral canonical JSON representation of one retained mutation batch.

Version `1.0` includes:

- manifest schema version;
- mutation batch ID;
- audit record count;
- each record's audit schema version, entity, and state;
- key, original, and current value objects;
- operation, attempt, decision, correlation, trace, and span identifiers.

Records and object properties are ordered using ordinal comparison rules. Empty audit value payloads are represented as empty JSON objects. This makes the canonical output stable across repeated runs, database return order, and JSON property insertion order.

The manifest is built from the `AuditRecord` values created after `IApplicationAuditValuePolicy` has applied inclusion, masking, hashing, omission, or truncation. Raw pre-protection entity values are not copied into the manifest merely to support hashing.

`IApplicationMutationManifestHasher` is replaceable through dependency injection. NCAT registers `Sha256ApplicationMutationManifestHasher` by default and writes `SHA-256` plus the uppercase hexadecimal digest to the receipt.

## Verification

An archive reader, SIEM adapter, governance service, or independent integrity check can load the retained `AuditRecord` batch and verify it against the receipt:

```csharp
IReadOnlyCollection<AuditRecord> retainedBatch = await dbContext.AuditRecords
    .Where(record => record.MutationBatchId == receipt.MutationBatchId)
    .ToListAsync(cancellationToken);

bool corresponds = manifestVerifier.Verify(receipt, retainedBatch);
```

Verification rebuilds the versioned canonical manifest, confirms the mutation batch and record count, recomputes the configured digest, and compares it using a fixed-time byte comparison. Verification succeeds only when the retained protected batch still corresponds to the receipt.

A successful digest comparison does **not** prove:

- that the business transaction and every audit write were durably committed atomically;
- that the records are stored in immutable or tamper-proof storage;
- that a legal, regulatory, or records-retention requirement has been satisfied;
- that the actor identity or upstream policy decision was independently authenticated;
- that a later archive transfer was complete unless that transfer retained and verified the full canonical batch.

Those guarantees require host-owned transaction strategy, access controls, retention policy, immutable storage, signing or timestamping where appropriate, and operational evidence.

## Value minimization

`IApplicationAuditValuePolicy` controls how each audited property value is represented before persistence, manifest construction, or external delivery.

Supported dispositions are:

- `Include`
- `Mask`
- `Hash`
- `Omit`
- `Truncate`

The default policy preserves the previous NCAT behavior and includes values unchanged. Production applications should replace it when entities may contain credentials, tokens, protected document bodies, personal information, or other regulated data.

```csharp
services.AddScoped<IApplicationAuditValuePolicy, ApplicationAuditValuePolicy>();
```

Redaction occurs before values are serialized into `KeyValues`, `OriginalValues`, or `CurrentValues`, and before the canonical manifest is hashed. Do not copy raw NCAT mutation values wholesale into telemetry, an external archive, a governance outbox, or a SIEM.

## Local persistence and generated values

Audit records whose values are available before the primary save participate in the normal EF Core save operation. Records that depend on database-generated temporary values are completed through the existing after-save audit flush.

Applications requiring strict atomicity between a business mutation and an after-save audit flush should place the complete operation inside an explicit host-owned database transaction and test the selected provider's failure behavior. NCAT remains provider-portable and does not impose a SQL Server- or SQLite-specific transaction strategy.

## Generic integration sequence

```text
Host decision or workflow record
        -> shared operation execution identity
        -> NCAT SaveChanges mutation batch
        -> protected canonical mutation manifest
        -> NCAT mutation audit receipt with digest
        -> archive, SIEM, governance, or lifecycle record
        -> later retained-batch verification
```

Only opaque identifiers, counts, algorithms, hashes, and minimized metadata need to cross system boundaries. NCAT remains authoritative for mutation details, while each external system remains authoritative for its own decision, lifecycle, alerting, or retention responsibilities.
