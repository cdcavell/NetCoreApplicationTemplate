namespace ProjectTemplate.Infrastructure.Data.Entities;

public class AuditRecord : DataEntity
{
    /// <summary>
    /// The user, system or remote IP address that made the change. It can be a username, an email address, or a system identifier. It should be unique enough to identify the source of the change, but it does not have to be globally unique.
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;
    /// <summary>
    /// The date and time when the change was made, in Coordinated Universal Time (UTC). It should be stored in UTC to avoid issues with time zones and daylight saving time. When displaying the date and time to users, it can be converted to their local time zone as needed.
    /// </summary>
    public DateTime ModifiedOnUtc { get; set; } = PersistenceTimestamp.UtcNow();
    /// <summary>
    /// The name of the application or service that made the change. It can be used to group changes by application or service, and to identify which application or service is responsible for a change. It should be unique enough to identify the source of the change, but it does not have to be globally unique.
    /// </summary>
    public string Application { get; set; } = string.Empty;
    /// <summary>
    /// The name of the entity that was changed. It can be used to group changes by entity type, and to identify which entity was changed. It should be unique enough to identify the type of entity that was changed, but it does not have to be globally unique.
    /// </summary>
    public string Entity { get; set; } = string.Empty;
    /// <summary>
    /// The state of the entity after the change. It can be used to identify whether the entity was created, updated, or deleted. It should be a string that represents the state of the entity, such as "Created", "Updated", or "Deleted".
    /// </summary>
    public string State { get; set; } = string.Empty;
    /// <summary>
    /// The key values of the entity that was changed. It can be used to identify which specific entity was changed, and to link the audit record to the entity. It should be a string that represents the key values of the entity, such as a JSON object or a comma-separated list of key-value pairs.
    /// </summary>
    public string KeyValues { get; set; } = string.Empty;
    /// <summary>
    /// The original values of the entity before the change. It can be used to identify what the values were before the change, and to compare them with the current values after the change. It should be a string that represents the original values of the entity, such as a JSON object or a comma-separated list of key-value pairs.
    /// </summary>
    public string OriginalValues { get; set; } = string.Empty;
    /// <summary>
    /// The current values of the entity after the change. It can be used to identify what the values are after the change, and to compare them with the original values before the change. It should be a string that represents the current values of the entity, such as a JSON object or a comma-separated list of key-value pairs.
    /// </summary>
    public string CurrentValues { get; set; } = string.Empty;
}
