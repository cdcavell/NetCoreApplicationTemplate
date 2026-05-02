namespace Template.Web.Options;

/// <summary>
/// Configuration settings for forwarded headers support.
/// </summary>
public sealed class TemplateForwardedHeadersOptions
{
    /// <summary>
    /// Configuration section name for forwarded headers settings.
    /// </summary>
    public const string SectionName = "Template:ForwardedHeaders";

    /// <summary>
    /// Enables or disables forwarded headers middleware.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Forwarded header values to process.
    /// Valid values include XForwardedFor, XForwardedProto, XForwardedHost, and XForwardedPrefix.
    /// </summary>
    public string[] Headers { get; set; } =
    [
        "XForwardedFor",
        "XForwardedProto"
    ];

    /// <summary>
    /// Limits the number of forwarded header entries processed.
    /// Keep this low unless the proxy chain is well understood.
    /// </summary>
    public int? ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Requires the number of values to match across forwarded headers.
    /// </summary>
    public bool RequireHeaderSymmetry { get; set; }

    /// <summary>
    /// Clears default known proxies/networks before applying configured values.
    /// For production, set this to true and explicitly configure trusted proxies/networks.
    /// </summary>
    public bool ClearKnownNetworksAndProxies { get; set; }

    /// <summary>
    /// Exact proxy IP addresses trusted to send forwarded headers.
    /// </summary>
    public string[] KnownProxies { get; set; } = [];

    /// <summary>
    /// Trusted proxy networks in CIDR notation.
    /// Example: 10.0.0.0/24
    /// </summary>
    public string[] KnownNetworks { get; set; } = [];

    /// <summary>
    /// Allowed host values from X-Forwarded-Host.
    /// Only needed when XForwardedHost is enabled.
    /// </summary>
    public string[] AllowedHosts { get; set; } = [];
}
