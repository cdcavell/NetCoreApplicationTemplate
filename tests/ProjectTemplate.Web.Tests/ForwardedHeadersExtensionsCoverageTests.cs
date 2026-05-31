using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Extensions;
using ProjectTemplate.Web.Options;

namespace ProjectTemplate.Web.Tests;

public sealed class ForwardedHeadersExtensionsCoverageTests
{
    /// <summary>
    /// Verifies that AddApplicationForwardedHeaders correctly configures ForwardedHeadersOptions based on the provided configuration when enabled. This test covers the mapping of all relevant properties from ApplicationForwardedHeadersOptions to ForwardedHeadersOptions, as well as validation of header names and known proxies/networks.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_EnabledConfiguration_ConfiguresForwardedHeadersOptions()
    {
        ServiceCollection services = new();

        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Enabled"] = "true",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:0"] = "XForwardedFor",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:1"] = "XForwardedProto",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:2"] = "XForwardedHost",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:ForwardLimit"] = "2",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:RequireHeaderSymmetry"] = "true",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:ClearKnownNetworksAndProxies"] = "true",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:KnownProxies:0"] = "10.20.30.40",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:KnownNetworks:0"] = "10.20.0.0/16",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:AllowedHosts:0"] = "example.test"
        });

        _ = services.AddApplicationForwardedHeaders(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        ForwardedHeadersOptions options = provider
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost));
        Assert.Equal(2, options.ForwardLimit);
        Assert.True(options.RequireHeaderSymmetry);
        Assert.Contains(IPAddress.Parse("10.20.30.40"), options.KnownProxies);
        _ = Assert.Single(options.KnownIPNetworks);
        Assert.Contains("example.test", options.AllowedHosts);
    }

    /// <summary>
    /// Verifies that when AddApplicationForwardedHeaders is called with Enabled set to false, the ForwardedHeadersOptions are configured to use no forwarded headers regardless of any other configuration values provided. This ensures that the middleware will not process any forwarded headers when disabled, even if header names or other settings are specified in the configuration.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_DisabledConfiguration_UsesNoForwardedHeaders()
    {
        ServiceCollection services = new();

        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Enabled"] = "false",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:0"] = "XForwardedFor",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:1"] = "XForwardedProto",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:ForwardLimit"] = "1"
        });

        _ = services.AddApplicationForwardedHeaders(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        ForwardedHeadersOptions options = provider
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;

        Assert.Equal(ForwardedHeaders.None, options.ForwardedHeaders);
    }

    /// <summary>
    /// Verifies that AddApplicationForwardedHeaders correctly parses header names with hyphens and underscores from the configuration and maps them to the appropriate ForwardedHeaders enum values. This test ensures that common variations in header naming conventions are supported and that the middleware will recognize headers like "X-Forwarded-For" and "X_Forwarded_Proto" regardless of formatting in the configuration.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_HeaderNamesWithHyphensAndUnderscores_AreParsed()
    {
        ServiceCollection services = new();

        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Enabled"] = "true",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:0"] = " x-forwarded-for ",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:1"] = "X_Forwarded_Proto",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:ForwardLimit"] = "1"
        });

        _ = services.AddApplicationForwardedHeaders(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        ForwardedHeadersOptions options = provider
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto));
    }

    /// <summary>
    /// Verifies that when AddApplicationForwardedHeaders is called with a ForwardLimit of zero, the options validation fails with an appropriate error message. This test ensures that the ForwardLimit property is correctly validated to be either null or greater than zero, preventing misconfiguration that could lead to unexpected behavior in the middleware when processing forwarded headers.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_ForwardLimitZero_FailsOptionsValidation()
    {
        Dictionary<string, string?> values = CreateValidValues();
        values[$"{ApplicationForwardedHeadersOptions.SectionName}:ForwardLimit"] = "0";

        AssertInvalidApplicationForwardedHeadersOptions(
            values,
            "ProjectTemplate:ForwardedHeaders:ForwardLimit must be null or greater than zero.");
    }

    /// <summary>
    /// Verifies that when AddApplicationForwardedHeaders is called with an invalid header name in the configuration, the options validation fails with an appropriate error message. This test ensures that only valid header names that correspond to the ForwardedHeaders enum are accepted, preventing misconfiguration that could lead to runtime errors or unexpected behavior in the middleware when processing forwarded headers.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_InvalidHeader_FailsOptionsValidation()
    {
        Dictionary<string, string?> values = CreateValidValues();
        values[$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:0"] = "InvalidForwardedHeader";

        AssertInvalidApplicationForwardedHeadersOptions(
            values,
            "ProjectTemplate:ForwardedHeaders:Headers contains an invalid forwarded header value.");
    }

    /// <summary>
    /// Verifies that when AddApplicationForwardedHeaders is called with an invalid known proxy IP address in the configuration, the options validation fails with an appropriate error message. This test ensures that the KnownProxies property is correctly validated to contain valid IP addresses, preventing misconfiguration that could lead to security issues or incorrect behavior in the middleware when determining trusted proxies.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_InvalidKnownProxy_FailsOptionsValidation()
    {
        Dictionary<string, string?> values = CreateValidValues();
        values[$"{ApplicationForwardedHeadersOptions.SectionName}:KnownProxies:0"] = "not-an-ip-address";

        AssertInvalidApplicationForwardedHeadersOptions(
            values,
            "ProjectTemplate:ForwardedHeaders:KnownProxies must contain valid IP addresses.");
    }

    /// <summary>
    /// Verifies that when AddApplicationForwardedHeaders is called with an invalid known network CIDR range in the configuration, the options validation fails with an appropriate error message. This test ensures that the KnownNetworks property is correctly validated to contain valid CIDR notation for IP networks, preventing misconfiguration that could lead to security issues or incorrect behavior in the middleware when determining trusted networks.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_InvalidKnownNetwork_FailsOptionsValidation()
    {
        Dictionary<string, string?> values = CreateValidValues();
        values[$"{ApplicationForwardedHeadersOptions.SectionName}:KnownNetworks:0"] = "10.20.0.0";

        AssertInvalidApplicationForwardedHeadersOptions(
            values,
            "ProjectTemplate:ForwardedHeaders:KnownNetworks must contain valid CIDR ranges such as 10.0.0.0/24.");
    }

    /// <summary>
    /// Verifies that when AddApplicationForwardedHeaders is called with XForwardedHost enabled in the Headers configuration but no allowed hosts specified in the AllowedHosts configuration, the options validation fails with an appropriate error message. This test ensures that the presence of XForwardedHost in the headers to process requires at least one allowed host to be configured, preventing misconfiguration that could lead to security issues or incorrect behavior in the middleware when processing forwarded host headers.
    /// </summary>
    [Fact]
    public void AddApplicationForwardedHeaders_ForwardedHostWithoutAllowedHosts_FailsOptionsValidation()
    {
        Dictionary<string, string?> values = CreateValidValues();
        values[$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:0"] = "XForwardedHost";
        _ = values.Remove($"{ApplicationForwardedHeadersOptions.SectionName}:AllowedHosts:0");

        AssertInvalidApplicationForwardedHeadersOptions(
            values,
            "ProjectTemplate:ForwardedHeaders:AllowedHosts must contain at least one host when XForwardedHost is enabled.");
    }

    private static void AssertInvalidApplicationForwardedHeadersOptions(
        IReadOnlyDictionary<string, string?> values,
        string expectedMessage)
    {
        ServiceCollection services = new();
        IConfiguration configuration = CreateConfiguration(values);

        _ = services.AddApplicationForwardedHeaders(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<ApplicationForwardedHeadersOptions>>().Value);

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    private static Dictionary<string, string?> CreateValidValues()
    {
        return new Dictionary<string, string?>
        {
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Enabled"] = "true",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:Headers:0"] = "XForwardedFor",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:ForwardLimit"] = "1",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:RequireHeaderSymmetry"] = "false",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:ClearKnownNetworksAndProxies"] = "true",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:KnownProxies:0"] = "10.20.30.40",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:KnownNetworks:0"] = "10.20.0.0/16",
            [$"{ApplicationForwardedHeadersOptions.SectionName}:AllowedHosts:0"] = "example.test"
        };
    }

    private static IConfiguration CreateConfiguration(IReadOnlyDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
