using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data.Auditing;

/// <summary>
/// Defines the template-owned storage seam for application audit records.
/// </summary>
/// <remarks>
/// The default implementation stores audit records locally through <see cref="Data.ApplicationDbContext" />.
/// Consuming applications may replace this contract with an outbox writer, external sink, or companion governance/audit adapter.
/// </remarks>
public interface IApplicationAuditStore
{
    /// <summary>
    /// Appends an audit record during the synchronous save pipeline.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="auditRecord">The audit record to append.</param>
    void Append(
        ApplicationDbContext dbContext,
        AuditRecord auditRecord);

    /// <summary>
    /// Appends an audit record during the asynchronous save pipeline.
    /// </summary>
    /// <param name="dbContext">The current application database context.</param>
    /// <param name="auditRecord">The audit record to append.</param>
    /// <param name="cancellationToken">A token that can cancel the append operation.</param>
    /// <returns>A task that completes when the audit record has been appended.</returns>
    ValueTask AppendAsync(
        ApplicationDbContext dbContext,
        AuditRecord auditRecord,
        CancellationToken cancellationToken = default);
}
