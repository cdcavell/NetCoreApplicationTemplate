using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Infrastructure.Data.Entities;

[Table("AuditRecords")]
public class AuditRecord : DataEntity
{
    [DataType(DataType.Text)]
    public string ModifiedBy { get; set; } = string.Empty;
    [DataType(DataType.DateTime)]
    public DateTime ModifiedOnUtc { get; set; } = DateTime.UtcNow;
    [DataType(DataType.Text)]
    public string Application { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    public string Entity { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    public string State { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    public string KeyValues { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    public string OriginalValues { get; set; } = string.Empty;
    [DataType(DataType.Text)]
    public string CurrentValues { get; set; } = string.Empty;
}
