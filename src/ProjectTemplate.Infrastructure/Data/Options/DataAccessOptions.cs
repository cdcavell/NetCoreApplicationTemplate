
namespace ProjectTemplate.Infrastructure.Data.Options;

public sealed class DataAccessOptions
{
    public const string SectionName = "ProjectTemplate:DataAccess";

    public string Provider { get; init; } = "Sqlite";

    public string ConnectionStringName { get; init; } = "ApplicationDatabase";

    public DataAuditingOptions Auditing { get; init; } = new();
}
