namespace ProjectTemplate.Infrastructure.Data.Entities;

/// <summary>
/// Represents a persisted link between a local application user and an external authentication provider identity.
/// </summary>
public sealed class ExternalLoginAccount : DataEntity
{
    /// <summary>
    /// Local application user ID to which this external login account is linked.
    /// </summary>
    public Guid LocalUserId { get; set; }

    /// <summary>
    /// Provider name of the external authentication service (e.g., "Google", "Facebook", "Microsoft").
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Provider-specific unique identifier for the user (e.g., the "sub" claim from an OpenID Connect token).
    /// </summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    /// Display name or email associated with the external account, for informational purposes. This is not used for authentication but can be helpful for administrators to identify linked accounts.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Email address associated with the external account, if available. This is not used for authentication but can be helpful for administrators to identify linked accounts and for potential communication purposes.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Creation timestamp in UTC when this external login account was linked to the local user. This can be useful for auditing and tracking purposes.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Update timestamp in UTC when this external login account was last modified. This can be useful for auditing and tracking purposes, especially if the display name or email associated with the external account changes over time.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Last login timestamp in UTC when the user authenticated using this external login account. This can be useful for auditing and tracking purposes, as well as for identifying inactive linked accounts.
    /// </summary>
    public DateTime? LastLoginOnUtc { get; set; }
}
