namespace ProjectTemplate.Web.Options;

/// <summary>
/// Represents template-level OpenTelemetry configuration.
/// </summary>
public sealed class ApplicationOpenTelemetryOptions
{
    /// <summary>
    /// Configuration section name for OpenTelemetry settings.
    /// </summary>
    public const string SectionName = "ProjectTemplate:OpenTelemetry";

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the logical OpenTelemetry service name.
    /// </summary>
    public string ServiceName { get; set; } = "ProjectTemplate.Web";

    /// <summary>
    /// Gets or sets the logical OpenTelemetry service version.
    /// </summary>
    public string? ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets a value indicating whether tracing is enabled.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics are enabled.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether ASP.NET Core instrumentation is enabled.
    /// </summary>
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether HttpClient instrumentation is enabled.
    /// </summary>
    public bool EnableHttpClientInstrumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets OTLP exporter options.
    /// </summary>
    public ApplicationOtlpExporterOptions Otlp { get; set; } = new();
}
