namespace ProjectTemplate.Web.Options;

/// <summary>
/// Options controlling structured HTTP request logging behavior.
/// </summary>
public sealed class ApplicationRequestLoggingOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind request logging settings.
    /// </summary>
    public static string SectionName { get; internal set; } = "ProjectTemplate:RequestLogging";

    /// <summary>
    /// Gets or sets a value indicating whether structured request logging is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the request/response header used for request correlation.
    /// </summary>
    public string CorrelationHeaderName { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets a value indicating whether the query string should be logged.
    /// Disabled by default because query strings may contain sensitive values.
    /// </summary>
    public bool IncludeQueryString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether authenticated user names should be logged.
    /// </summary>
    public bool IncludeUserName { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the remote IP address should be logged.
    /// </summary>
    public bool IncludeRemoteIpAddress { get; set; } = true;

    /// <summary>
    /// Gets or sets path prefixes that should be excluded from normal request logging.
    /// Matching requests are logged at Verbose level so the default sinks suppress them.
    /// </summary>
    public List<string> ExcludedPathPrefixes { get; set; } =
    [
        "/health",
        "/metrics",
        "/favicon.ico",
        "/css",
        "/js",
        "/lib",
        "/_framework"
    ];
}
