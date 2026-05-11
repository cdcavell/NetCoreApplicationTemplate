namespace Template.Web.Options;

/// <summary>
/// Represents supported authentication provider configuration.
/// </summary>
public sealed class AuthenticationProviderOptions
{
    /// <summary>
    /// OpenIdConnect provider options.
    /// </summary>
    public ProviderOptions OpenIdConnect { get; set; } = new();

    /// <summary>
    /// SAML2 provider options.
    /// </summary>
    public ProviderOptions Saml2 { get; set; } = new();

    /// <summary>
    /// Microsoft provider options.
    /// </summary>
    public ProviderOptions Microsoft { get; set; } = new();

    /// <summary>
    /// Google provider options.
    /// </summary>
    public ProviderOptions Google { get; set; } = new();

    /// <summary>
    /// Social Media provider options.
    /// </summary>
    public ProviderOptions Social { get; set; } = new();
}

/// <summary>
/// Represents common provider enablement options.
/// </summary>
public sealed class ProviderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool Enabled { get; set; }
}
