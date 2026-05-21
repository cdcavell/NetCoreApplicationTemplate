namespace ProjectTemplate.Web.Options;

/// <summary>
/// Represents OTLP exporter configuration.
/// </summary>
public sealed class ApplicationOtlpExporterOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether OTLP export is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the OTLP endpoint.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OTLP protocol. Supported values are Grpc and HttpProtobuf.
    /// </summary>
    public string Protocol { get; set; } = "Grpc";
}
