namespace Template.Infrastructure.Data.Entities;

public class AuditRecord : DataEntity
{
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedOnUtc { get; set; } = DateTime.UtcNow;
    public string Application { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string KeyValues { get; set; } = string.Empty;
    public string OriginalValues { get; set; } = string.Empty;
    public string CurrentValues { get; set; } = string.Empty;
}
