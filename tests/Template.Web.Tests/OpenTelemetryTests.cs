using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Template.Web.Extensions;
using Template.Web.Options;
using Template.Web.Tests.Extensions;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration and configuration tests for the template OpenTelemetry foundation.
/// </summary>
public sealed class OpenTelemetryTests
{
    private const string _releaseVersion = "0.2.4";

    /// <summary>
    /// Verifies that template OpenTelemetry options are bound from configuration.
    /// </summary>
    [Fact]
    public void OpenTelemetryOptions_Bind_DefaultConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:OpenTelemetry:Enabled"] = "true",
            ["Template:OpenTelemetry:ServiceName"] = "Template.Web",
            ["Template:OpenTelemetry:ServiceVersion"] = _releaseVersion,
            ["Template:OpenTelemetry:EnableTracing"] = "true",
            ["Template:OpenTelemetry:EnableMetrics"] = "true",
            ["Template:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
            ["Template:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
            ["Template:OpenTelemetry:Otlp:Enabled"] = "false",
            ["Template:OpenTelemetry:Otlp:Endpoint"] = "",
            ["Template:OpenTelemetry:Otlp:Protocol"] = "Grpc"
        });

        TemplateOpenTelemetryOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateOpenTelemetryOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.Equal("Template.Web", options.ServiceName);
        Assert.Equal(_releaseVersion, options.ServiceVersion);
        Assert.True(options.EnableTracing);
        Assert.True(options.EnableMetrics);
        Assert.True(options.EnableAspNetCoreInstrumentation);
        Assert.True(options.EnableHttpClientInstrumentation);

        Assert.False(options.Otlp.Enabled);
        Assert.Equal(string.Empty, options.Otlp.Endpoint);
        Assert.Equal("Grpc", options.Otlp.Protocol);
    }

    /// <summary>
    /// Verifies that OpenTelemetry options validation fails when OTLP export is enabled with an invalid endpoint.
    /// </summary>
    [Fact]
    public void OpenTelemetryOptions_InvalidOtlpEndpoint_FailsValidationWhenExporterEnabled()
    {
        OptionsValidationException exception =
            AssertOpenTelemetryOptionsValidationFails(
                new Dictionary<string, string?>
                {
                    ["Template:OpenTelemetry:Enabled"] = "true",
                    ["Template:OpenTelemetry:ServiceName"] = "Template.Web",
                    ["Template:OpenTelemetry:ServiceVersion"] = _releaseVersion,
                    ["Template:OpenTelemetry:EnableTracing"] = "true",
                    ["Template:OpenTelemetry:EnableMetrics"] = "true",
                    ["Template:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
                    ["Template:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
                    ["Template:OpenTelemetry:Otlp:Enabled"] = "true",
                    ["Template:OpenTelemetry:Otlp:Endpoint"] = "not-a-valid-uri",
                    ["Template:OpenTelemetry:Otlp:Protocol"] = "Grpc"
                });

        Assert.Contains(
            "Template:OpenTelemetry:Otlp:Endpoint must be a valid absolute URI when OTLP export is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the application starts when OpenTelemetry is enabled and OTLP export is disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Application_Starts_WhenOpenTelemetryEnabledWithoutOtlpExporter()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:OpenTelemetry:Enabled"] = "true",
            ["Template:OpenTelemetry:ServiceName"] = "Template.Web",
            ["Template:OpenTelemetry:ServiceVersion"] = _releaseVersion,
            ["Template:OpenTelemetry:EnableTracing"] = "true",
            ["Template:OpenTelemetry:EnableMetrics"] = "true",
            ["Template:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
            ["Template:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
            ["Template:OpenTelemetry:Otlp:Enabled"] = "false",
            ["Template:OpenTelemetry:Otlp:Endpoint"] = "",
            ["Template:OpenTelemetry:Otlp:Protocol"] = "Grpc"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the application starts when OpenTelemetry is disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Application_Starts_WhenOpenTelemetryDisabled()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:OpenTelemetry:Enabled"] = "false",
            ["Template:OpenTelemetry:ServiceName"] = "Template.Web",
            ["Template:OpenTelemetry:ServiceVersion"] = _releaseVersion,
            ["Template:OpenTelemetry:EnableTracing"] = "true",
            ["Template:OpenTelemetry:EnableMetrics"] = "true",
            ["Template:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
            ["Template:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
            ["Template:OpenTelemetry:Otlp:Enabled"] = "false",
            ["Template:OpenTelemetry:Otlp:Endpoint"] = "",
            ["Template:OpenTelemetry:Otlp:Protocol"] = "Grpc"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Creates a test application factory with the supplied in-memory configuration overrides.
    /// </summary>
    /// <param name="configurationValues">The configuration key/value pairs used to override template settings for a test.</param>
    /// <returns>A configured <see cref="TemplateWebApplicationFactory"/> instance.</returns>
    private static TemplateWebApplicationFactory CreateFactory(IReadOnlyDictionary<string, string?> configurationValues)
    {
        return new TemplateWebApplicationFactory(configurationValues);
    }

    private static OptionsValidationException AssertOpenTelemetryOptionsValidationFails(
        IReadOnlyDictionary<string, string?> configurationValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        IServiceCollection services = new ServiceCollection();

        _ = services.AddTemplateOpenTelemetry(
            configuration,
            new TestHostEnvironment());

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        return Assert.Throws<OptionsValidationException>(() =>
            provider
                .GetRequiredService<IOptions<TemplateOpenTelemetryOptions>>()
                .Value);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";

        public string ApplicationName { get; set; } = "Template.Web.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
