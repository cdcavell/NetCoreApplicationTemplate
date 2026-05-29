using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Extensions;
using ProjectTemplate.Web.Options;
using ProjectTemplate.Web.Tests.Extensions;
using ProjectTemplate.Web.Tests.Infrastructure;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides integration and configuration tests for the application OpenTelemetry foundation.
/// </summary>
public sealed class OpenTelemetryTests
{
    private const string _releaseVersion = "0.4.0";

    /// <summary>
    /// Verifies that application OpenTelemetry options are bound from configuration.
    /// </summary>
    [Fact]
    public void OpenTelemetryOptions_Bind_DefaultConfiguration()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:OpenTelemetry:Enabled"] = "true",
            ["ProjectTemplate:OpenTelemetry:ServiceName"] = "ProjectTemplate.Web",
            ["ProjectTemplate:OpenTelemetry:ServiceVersion"] = _releaseVersion,
            ["ProjectTemplate:OpenTelemetry:EnableTracing"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableMetrics"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
            ["ProjectTemplate:OpenTelemetry:Otlp:Enabled"] = "false",
            ["ProjectTemplate:OpenTelemetry:Otlp:Endpoint"] = "",
            ["ProjectTemplate:OpenTelemetry:Otlp:Protocol"] = "Grpc"
        });

        ApplicationOpenTelemetryOptions options = factory.Services
            .GetRequiredService<IOptions<ApplicationOpenTelemetryOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.Equal("ProjectTemplate.Web", options.ServiceName);
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
                    ["ProjectTemplate:OpenTelemetry:Enabled"] = "true",
                    ["ProjectTemplate:OpenTelemetry:ServiceName"] = "ProjectTemplate.Web",
                    ["ProjectTemplate:OpenTelemetry:ServiceVersion"] = _releaseVersion,
                    ["ProjectTemplate:OpenTelemetry:EnableTracing"] = "true",
                    ["ProjectTemplate:OpenTelemetry:EnableMetrics"] = "true",
                    ["ProjectTemplate:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
                    ["ProjectTemplate:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
                    ["ProjectTemplate:OpenTelemetry:Otlp:Enabled"] = "true",
                    ["ProjectTemplate:OpenTelemetry:Otlp:Endpoint"] = "not-a-valid-uri",
                    ["ProjectTemplate:OpenTelemetry:Otlp:Protocol"] = "Grpc"
                });

        Assert.Contains(
            "ProjectTemplate:OpenTelemetry:Otlp:Endpoint must be a valid absolute URI when OTLP export is enabled",
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:OpenTelemetry:Enabled"] = "true",
            ["ProjectTemplate:OpenTelemetry:ServiceName"] = "ProjectTemplate.Web",
            ["ProjectTemplate:OpenTelemetry:ServiceVersion"] = _releaseVersion,
            ["ProjectTemplate:OpenTelemetry:EnableTracing"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableMetrics"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
            ["ProjectTemplate:OpenTelemetry:Otlp:Enabled"] = "false",
            ["ProjectTemplate:OpenTelemetry:Otlp:Endpoint"] = "",
            ["ProjectTemplate:OpenTelemetry:Otlp:Protocol"] = "Grpc"
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:OpenTelemetry:Enabled"] = "false",
            ["ProjectTemplate:OpenTelemetry:ServiceName"] = "ProjectTemplate.Web",
            ["ProjectTemplate:OpenTelemetry:ServiceVersion"] = _releaseVersion,
            ["ProjectTemplate:OpenTelemetry:EnableTracing"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableMetrics"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableAspNetCoreInstrumentation"] = "true",
            ["ProjectTemplate:OpenTelemetry:EnableHttpClientInstrumentation"] = "true",
            ["ProjectTemplate:OpenTelemetry:Otlp:Enabled"] = "false",
            ["ProjectTemplate:OpenTelemetry:Otlp:Endpoint"] = "",
            ["ProjectTemplate:OpenTelemetry:Otlp:Protocol"] = "Grpc"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that no appsettings.json files in the repository contain a stale OpenTelemetry service version value.
    /// </summary>
    [Fact]
    public void Appsettings_DoNotContainStaleOpenTelemetryServiceVersion()
    {
        string solutionRoot = GetSolutionRoot();

        List<string> appsettingsPaths = [];

        string srcDirectory = Path.Combine(solutionRoot, "src");

        if (Directory.Exists(srcDirectory))
        {
            appsettingsPaths.AddRange(
                Directory.EnumerateFiles(srcDirectory, "appsettings.json", SearchOption.AllDirectories));
        }

        string templateContentAppsettings = Path.Combine(
            solutionRoot,
            ".template.content",
            "src",
            "ProjectTemplate.Web",
            "appsettings.json");

        if (File.Exists(templateContentAppsettings))
        {
            appsettingsPaths.Add(templateContentAppsettings);
        }

        Assert.NotEmpty(appsettingsPaths);

        foreach (string appsettingsPath in appsettingsPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string appsettings = File.ReadAllText(appsettingsPath);

            Assert.DoesNotContain(
                "\"ServiceVersion\": \"0.3.1\"",
                appsettings,
                StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Gets the root directory of the solution.
    /// </summary>
    /// <returns>The solution root directory.</returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    private static string GetSolutionRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.EnumerateFiles(directory.FullName, "*.slnx").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate solution root from '{AppContext.BaseDirectory}'.");
    }

    /// <summary>
    /// Creates a test application factory with the supplied in-memory configuration overrides.
    /// </summary>
    /// <param name="configurationValues">The configuration key/value pairs used to override application settings for a test.</param>
    /// <returns>A configured <see cref="ApplicationWebApplicationFactory"/> instance.</returns>
    private static ApplicationWebApplicationFactory CreateFactory(IReadOnlyDictionary<string, string?> configurationValues)
    {
        return new ApplicationWebApplicationFactory(configurationValues);
    }

    private static OptionsValidationException AssertOpenTelemetryOptionsValidationFails(
        IReadOnlyDictionary<string, string?> configurationValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        IServiceCollection services = new ServiceCollection();

        _ = services.AddApplicationOpenTelemetry(
            configuration,
            new TestHostEnvironment());

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        return Assert.Throws<OptionsValidationException>(() =>
            provider
                .GetRequiredService<IOptions<ApplicationOpenTelemetryOptions>>()
                .Value);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";

        public string ApplicationName { get; set; } = "ProjectTemplate.Web.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
