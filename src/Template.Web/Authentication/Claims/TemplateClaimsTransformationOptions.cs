namespace Template.Web.Authentication.Claims;

/// <summary>
/// Represents template claims transformation options.
/// </summary>
public sealed class TemplateClaimsTransformationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the original claims should be removed after processing.
    /// </summary>
    public bool RemoveOriginalClaims { get; set; }

    /// <summary>
    /// Gets or sets the default claim mappings used when no provider-specific mapping is configured.
    /// </summary>
    public TemplateClaimMappingOptions DefaultMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets provider-specific claim mappings keyed by authentication provider scheme or authentication type.
    /// </summary>
    public Dictionary<string, TemplateClaimMappingOptions> ProviderMappings { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}
