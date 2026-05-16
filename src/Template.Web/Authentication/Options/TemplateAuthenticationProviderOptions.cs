using Template.Web.Authentication.Providers.OpenIdConnect;
using Template.Web.Authentication.Providers.Saml2;

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
    public TemplateSaml2AuthenticationOptions Saml2 { get; set; } = new();

    /// <summary>
    /// Gets or sets Microsoft provider options.
    /// </summary>
    public TemplateExternalAuthenticationProviderOptions Microsoft { get; set; } = new();

    /// <summary>
    /// Gets or sets Google provider options.
    /// </summary>
    public TemplateExternalAuthenticationProviderOptions Google { get; set; } = new();

    /// <summary>
    /// Gets or sets the options used to configure GitHub as an external authentication provider.
    /// </summary>
    public TemplateExternalAuthenticationProviderOptions GitHub { get; set; } = new();
}
