using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Infrastructure.Data.Entities;

namespace ProjectTemplate.Infrastructure.Data;

public sealed partial class ApplicationDbContext
{
    public DbSet<ApplicationAuditReconciliationFinding> ApplicationAuditReconciliationFindings =>
        Set<ApplicationAuditReconciliationFinding>();

    public DbSet<ApplicationAuditReconciliationRemediation> ApplicationAuditReconciliationRemediations =>
        Set<ApplicationAuditReconciliationRemediation>();
}
