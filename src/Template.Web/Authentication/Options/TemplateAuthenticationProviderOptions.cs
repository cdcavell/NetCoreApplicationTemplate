using Template.Web.Authentication.Providers.OpenIdConnect;

namespace Template.Web.Authentication.Options;

/// <summary>
/// Represents provider-specific authentication configuration.
/// </summary>
public sealed class TemplateAuthenticationProviderOptions
{
    /// <summary>
    /// Gets or sets OpenID Connect provider options.
    /// </summary>
    public TemplateOpenIdConnectAuthenticationOptions OpenIdConnect { get; set; } = new();

    /// <summary>
    /// Gets or sets SAML2 provider options.
    /// </summary>
    public TemplateExternalAuthenticationProviderOptions Saml2 { get; set; } = new();

    /// <summary>
    /// Gets or sets Microsoft provider options.
    /// </summary>
    public TemplateExternalAuthenticationProviderOptions Microsoft { get; set; } = new();

    /// <summary>
    /// Gets or sets Google provider options.
    /// </summary>
    public TemplateExternalAuthenticationProviderOptions Google { get; set; } = new();
}
