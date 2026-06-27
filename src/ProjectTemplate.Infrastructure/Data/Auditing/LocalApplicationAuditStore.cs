using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Stores application audit records in the local EF Core audit record table.
/// </summary>
public sealed class LocalApplicationAuditStore : IApplicationAuditStore
{
    /// <inheritdoc />
    public void Append(
        ApplicationDbContext dbContext,
        AuditRecord auditRecord)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(auditRecord);

        _ = dbContext.AuditRecords.Add(auditRecord);
    }

    /// <inheritdoc />
    public async ValueTask AppendAsync(
        ApplicationDbContext dbContext,
        AuditRecord auditRecord,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(auditRecord);
        cancellationToken.ThrowIfCancellationRequested();

        _ = await dbContext.AuditRecords.AddAsync(auditRecord, cancellationToken).ConfigureAwait(false);
    }
}
