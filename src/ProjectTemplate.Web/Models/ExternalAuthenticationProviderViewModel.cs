namespace ProjectTemplate.Web.Models;

/// <summary>
/// Represents an available external authentication provider.
/// </summary>
public sealed class ExternalAuthenticationProviderViewModel
{
    /// <summary>
    /// Gets or sets the authentication scheme name.
    /// </summary>
    public string Scheme { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
