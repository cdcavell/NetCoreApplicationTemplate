using ProjectTemplate.Web.Authentication.Providers.OpenIdConnect;
using ProjectTemplate.Web.Authentication.Providers.Saml2;

namespace ProjectTemplate.Web.Authentication.Options;

/// <summary>
/// Represents provider-specific authentication configuration.
/// </summary>
public sealed class ApplicationAuthenticationProviderOptions
{
    /// <summary>
    /// Gets or sets OpenID Connect provider options.
    /// </summary>
    public OpenIdConnectAuthenticationOptions OpenIdConnect { get; set; } = new();

    /// <summary>
    /// Gets or sets SAML2 provider options.
    /// </summary>
    public Saml2AuthenticationOptions Saml2 { get; set; } = new();

    /// <summary>
    /// Gets or sets Microsoft provider options.
    /// </summary>
    public ApplicationExternalAuthenticationProviderOptions Microsoft { get; set; } = new();

    /// <summary>
    /// Gets or sets Google provider options.
    /// </summary>
    public ApplicationExternalAuthenticationProviderOptions Google { get; set; } = new();

    /// <summary>
    /// Gets or sets the options used to configure GitHub as an external authentication provider.
    /// </summary>
    public ApplicationExternalAuthenticationProviderOptions GitHub { get; set; } = new();
}
