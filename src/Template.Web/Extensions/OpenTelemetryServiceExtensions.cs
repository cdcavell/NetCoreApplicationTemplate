using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Template.Web.Options;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to register OpenTelemetry services for the application.
/// </summary>
public static class OpenTelemetryServiceExtensions
{
    /// <summary>
    /// Adds baseline OpenTelemetry tracing and metrics support.
    /// </summary>
    /// <param name="services">The service collection to add OpenTelemetry services to.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="environment">The current hosting environment.</param>
    /// <returns>The same service collection instance so calls can be chained.</returns>
    public static IServiceCollection AddTemplateOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddOptions<TemplateOpenTelemetryOptions>()
            .Bind(configuration.GetSection(TemplateOpenTelemetryOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ServiceName),
                "Template:OpenTelemetry:ServiceName must not be empty.")
            .Validate(options => !options.Otlp.Enabled || Uri.TryCreate(options.Otlp.Endpoint, UriKind.Absolute, out _),
                "Template:OpenTelemetry:Otlp:Endpoint must be a valid absolute URI when OTLP export is enabled.")
            .ValidateOnStart();

        TemplateOpenTelemetryOptions options = configuration
            .GetSection(TemplateOpenTelemetryOptions.SectionName)
            .Get<TemplateOpenTelemetryOptions>() ?? new TemplateOpenTelemetryOptions();

        if (!options.Enabled)
        {
            return services;
        }

        OpenTelemetryBuilder openTelemetryBuilder = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment.name"] = environment.EnvironmentName
                }));

        if (options.EnableTracing)
        {
            openTelemetryBuilder.WithTracing(tracing =>
            {
                if (options.EnableAspNetCoreInstrumentation)
                {
                    tracing.AddAspNetCoreInstrumentation();
                }

                if (options.EnableHttpClientInstrumentation)
                {
                    tracing.AddHttpClientInstrumentation();
                }

                if (options.Otlp.Enabled)
                {
                    tracing.AddOtlpExporter(exporterOptions => ConfigureOtlpExporter(exporterOptions, options.Otlp));
                }
            });
        }

        if (options.EnableMetrics)
        {
            openTelemetryBuilder.WithMetrics(metrics =>
            {
                if (options.EnableAspNetCoreInstrumentation)
                {
                    metrics.AddAspNetCoreInstrumentation();
                }

                if (options.EnableHttpClientInstrumentation)
                {
                    metrics.AddHttpClientInstrumentation();
                }

                if (options.Otlp.Enabled)
                {
                    metrics.AddOtlpExporter(exporterOptions => ConfigureOtlpExporter(exporterOptions, options.Otlp));
                }
            });
        }

        return services;
    }

    private static void ConfigureOtlpExporter(
        OtlpExporterOptions exporterOptions,
        TemplateOtlpExporterOptions options)
    {
        exporterOptions.Endpoint = new Uri(options.Endpoint);
        exporterOptions.Protocol = string.Equals(
            options.Protocol,
            "HttpProtobuf",
            StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;
    }
}
