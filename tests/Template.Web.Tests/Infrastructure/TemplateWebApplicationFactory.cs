using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Web.Tests.TestControllers;

namespace Template.Web.Tests.Infrastructure;

/// <summary>
/// Provides a custom WebApplicationFactory for integration testing, allowing configuration values to be injected and
/// the test environment to be set up with controllers from the specified assembly.
/// </summary>
/// <remarks>This factory sets the hosting environment to "Testing" and registers controllers from the assembly
/// containing RateLimitingTestController. Use this class to create test server instances with custom configuration for
/// integration tests.</remarks>
/// <param name="configurationValues">A read-only dictionary containing configuration key-value pairs to be applied to the test application's
/// configuration. Keys represent configuration paths; values may be null to clear a setting.</param>
internal sealed class TemplateWebApplicationFactory(IReadOnlyDictionary<string, string?> configurationValues) : WebApplicationFactory<Program>
{
    private readonly IReadOnlyDictionary<string, string?> _configurationValues = configurationValues;

    /// <summary>
    /// Configures the web host builder with test-specific settings, including environment, configuration, and services.
    /// </summary>
    /// <remarks>This method sets the environment to "Testing", adds in-memory configuration values, and
    /// registers controllers from the test assembly. Override this method to customize the web host for integration
    /// testing scenarios.</remarks>
    /// <param name="builder">The <see cref="IWebHostBuilder"/> to configure with the testing environment, in-memory configuration values, and
    /// required services.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddInMemoryCollection(_configurationValues));

        builder.ConfigureServices(services => services
                .AddControllersWithViews()
                .AddApplicationPart(typeof(RateLimitingTestController).Assembly));
    }
}
