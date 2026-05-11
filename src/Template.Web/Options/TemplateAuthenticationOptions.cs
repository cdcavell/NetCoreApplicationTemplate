namespace Template.Web.Options;

/// <summary>
/// Represents template-level authentication configuration.
/// </summary>
public sealed class TemplateAuthenticationOptions
{
    /// <summary>
    /// Configuration section name for authentication settings.
    /// </summary>
    public const string SectionName = "Template:Authentication";

    /// <summary>
    /// Gets or sets a value indicating whether authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the default authentication scheme.
    /// </summary>
    public string DefaultScheme { get; set; } = "Cookies";

    /// <summary>
    /// Gets or sets provider-specific authentication options.
    /// </summary>
    public AuthenticationProviderOptions Providers { get; set; } = new();
}
