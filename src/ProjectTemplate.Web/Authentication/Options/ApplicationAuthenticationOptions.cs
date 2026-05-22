using ProjectTemplate.Web.Authentication.Claims;

namespace ProjectTemplate.Web.Authentication.Options;

/// <summary>
/// Represents application-level authentication configuration.
/// </summary>
public sealed class ApplicationAuthenticationOptions
{
    /// <summary>
    /// Configuration section name for authentication settings.
    /// </summary>
    public const string SectionName = "ProjectTemplate:Authentication";

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
    public ApplicationCookieAuthenticationOptions Cookie { get; set; } = new();

    /// <summary>
    /// Gets or sets provider-specific authentication options.
    /// </summary>
    public ApplicationAuthenticationProviderOptions Providers { get; set; } = new();

    /// <summary>
    /// Gets or sets claims transformation and normalization options.
    /// </summary>
    public ApplicationClaimsTransformationOptions ClaimsTransformation { get; set; } = new();
}
