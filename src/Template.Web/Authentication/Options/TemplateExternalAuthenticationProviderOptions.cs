namespace Template.Web.Authentication.Options;

/// <summary>
/// Represents common enablement settings for an external authentication provider.
/// </summary>
public sealed class TemplateExternalAuthenticationProviderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the external authentication provider is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the authentication scheme used for the current operation.
    /// </summary>
    public string Scheme { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name associated with the authentication scheme.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the client application.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret used for authentication with the external service.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request path within the application's base path where the authentication middleware will
    /// receive the authentication response.
    /// </summary>
    public string CallbackPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional provider scopes requested during the external authentication challenge.
    /// </summary>
    public string[] Scopes { get; set; } = [];
}
