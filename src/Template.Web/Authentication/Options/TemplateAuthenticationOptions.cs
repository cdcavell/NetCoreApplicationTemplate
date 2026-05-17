using Template.Web.Authentication.Claims;

namespace Template.Web.Authentication.Options;

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
    /// Gets or sets the default challenge scheme.
    /// </summary>
    public string DefaultChallengeScheme { get; set; } = "Cookies";

    /// <summary>
    /// Gets or sets the default sign-in scheme used by external authentication providers.
    /// </summary>
    public string DefaultSignInScheme { get; set; } = "Cookies";

    /// <summary>
    /// Gets or sets cookie authentication options.
    /// </summary>
    public TemplateCookieAuthenticationOptions Cookie { get; set; } = new();

    /// <summary>
    /// Gets or sets provider-specific authentication options.
    /// </summary>
    public TemplateAuthenticationProviderOptions Providers { get; set; } = new();

    /// <summary>
    /// Gets or sets claims transformation and normalization options.
    /// </summary>
    public TemplateClaimsTransformationOptions ClaimsTransformation { get; set; } = new();
}
