# Audit Accountability Integration

NCAT records application data mutations. A companion governance or policy system records why a protected operation was permitted or denied and how that governed execution progressed.

The two records should remain distinct and be joined through shared identifiers rather than copied into one another.

## Record ownership

| Record | Authoritative question |
| --- | --- |
| NCAT `AuditRecord` | What persisted application state changed? |
| Companion decision audit record | Why was the operation allowed or denied? |
| Companion execution lifecycle receipt | Did the permitted operation execute, and which NCAT mutation batch resulted? |

NCAT does not require AsiBackbone or another governance package. The integration contracts are host-owned and framework-neutral.

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

## Mutation batches

Every logical `SaveChanges` or `SaveChangesAsync` mutation set receives one `MutationBatchId`. All NCAT audit records produced by that save share the same identifier.

After the save completes, `IApplicationMutationAuditReceiptAccessor.LastCompletedReceipt` exposes a minimized receipt containing:

- mutation batch ID;
- audit record count;
- persistence outcome;
- operation execution and attempt identifiers;
- decision audit record identifier;
- correlation and trace identifiers.

The receipt intentionally excludes entity keys and original/current values. A companion lifecycle system can use the receipt to bind its execution-completed event to the NCAT mutation batch.

## Value minimization

`IApplicationAuditValuePolicy` controls how each audited property value is represented before persistence or external delivery.

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

Redaction occurs before values are serialized into `KeyValues`, `OriginalValues`, or `CurrentValues`. Do not copy raw NCAT mutation values wholesale into telemetry, a governance outbox, or an external SIEM.

## Local persistence and generated values

Audit records whose values are available before the primary save participate in the normal EF Core save operation. Records that depend on database-generated temporary values are completed through the existing after-save audit flush.

Applications requiring strict atomicity between a business mutation and an after-save audit flush should place the complete operation inside an explicit host-owned database transaction and test the selected provider's failure behavior. NCAT remains provider-portable and does not impose a SQL Server- or SQLite-specific transaction strategy.

## AsiBackbone integration pattern

A host using AsiBackbone can follow this sequence:

```text
AsiBackbone decision audit record
        -> shared operation execution identity
        -> NCAT SaveChanges mutation batch
        -> NCAT mutation audit receipt
        -> AsiBackbone execution-completed lifecycle event
```

The host should pass the AsiBackbone decision audit identifier, operation execution identifier, attempt identifier, correlation identifier, and trace identifiers through `ApplicationAuditContext`.

Only opaque identifiers, counts, hashes, and minimized metadata should cross into the governance lifecycle record. NCAT remains authoritative for mutation details; AsiBackbone remains authoritative for policy decisions and governed execution lifecycle.
