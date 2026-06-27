using ProjectTemplate.Infrastructure.Data.Auditing;

namespace ProjectTemplate.Infrastructure.Data.Options;

public sealed class DataAuditingOptions
{
    public bool Enabled { get; init; } = true;

    public string StorageMode { get; init; } = AuditStorageModes.Local;
}
