namespace ProjectTemplate.Web.Authentication.Providers.Saml2;

/// <summary>
/// Represents SAML2 authentication provider configuration.
/// </summary>
public sealed class Saml2AuthenticationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the authentication scheme used for the current operation.
    /// </summary>
    public string Scheme { get; set; } = "Saml2";

    /// <summary>
    /// Gets or sets the display name associated with the SAML2 entity.
    /// </summary>
    public string DisplayName { get; set; } = "SAML2";

    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the metadata endpoint associated with this instance.
    /// </summary>
    public string MetadataUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative request path that the application listens on for SAML2 authentication callbacks.
    /// </summary>
    /// <remarks>This path is used by the authentication middleware to receive SAML2 assertions from the
    /// identity provider. It should match the Assertion Consumer Service (ACS) endpoint configured with the identity
    /// provider.</remarks>
    public string ModulePath { get; set; } = "/Saml2/Acs";

    /// <summary>
    /// Gets or sets a value indicating whether metadata should be loaded.
    /// </summary>
    public bool LoadMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether assertions must be signed.
    /// </summary>
    /// <remarks>Set this property to <see langword="true"/> to require that all assertions are
    /// cryptographically signed for validation. This enhances security by ensuring the authenticity and integrity of
    /// assertions. If set to <see langword="false"/>, unsigned assertions will be accepted, which may reduce
    /// security.</remarks>
    public bool RequireSignedAssertions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to validate SSL certificates during secure connections.
    /// </summary>
    /// <remarks>Set this property to <see langword="false"/> to disable certificate validation, which may
    /// expose the connection to security risks. It is recommended to leave this property set to <see langword="true"/>
    /// in production environments.</remarks>
    public bool ValidateCertificates { get; set; } = true;
}
