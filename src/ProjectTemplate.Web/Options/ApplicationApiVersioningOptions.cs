namespace ProjectTemplate.Web.Options;

/// <summary>
/// Represents template-level API versioning configuration.
/// </summary>
public sealed class ApplicationApiVersioningOptions
{
    /// <summary>
    /// Configuration section name for API versioning settings.
    /// </summary>
    public const string SectionName = "ProjectTemplate:ApiVersioning";

    /// <summary>
    /// Gets or sets the default major API version.
    /// </summary>
    public int DefaultMajorVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the default minor API version.
    /// </summary>
    public int DefaultMinorVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an unspecified API version should use the default version.
    /// </summary>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether supported and deprecated API versions should be reported in response headers.
    /// </summary>
    public bool ReportApiVersions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether URL segment versioning is enabled.
    /// </summary>
    public bool EnableUrlSegmentVersioning { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether header-based versioning is enabled.
    /// </summary>
    public bool EnableHeaderVersioning { get; set; } = true;

    /// <summary>
    /// Gets or sets the request header name used for header-based API versioning.
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Version";
}
