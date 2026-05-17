using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Template.Infrastructure.Data.Entities;

namespace Template.Infrastructure.Data;

/// <summary>
/// Represents the EF Core database context for the template application.
/// </summary>
public sealed partial class TemplateDbContext(DbContextOptions<TemplateDbContext> options, ILogger<TemplateDbContext> logger)
    : DbContext(options)
{
    private readonly ILogger<TemplateDbContext> _logger = logger;

    /// <summary>
    /// Gets the audit records for the application.
    /// </summary>
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    [LoggerMessage(
        EventId = 19000,
        Level = LogLevel.Trace,
        Message = "{EfCoreMessage}")]
    private static partial void LogEfCoreMessage(
        ILogger logger,
        string efCoreMessage);

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder
            .LogTo(message => LogEfCoreMessage(_logger, message), LogLevel.Trace)
            .EnableDetailedErrors();

        base.OnConfiguring(optionsBuilder);
    }
}
