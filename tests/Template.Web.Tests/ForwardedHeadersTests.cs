using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.Web.Options;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

/// <summary>
/// Provides tests for forwarded headers configuration and binding behavior.
/// </summary>
public sealed class ForwardedHeadersTests
{
    /// <summary>
    /// Verifies that X-Forwarded-For and X-Forwarded-Proto are enabled by default,
    /// while X-Forwarded-Host remains disabled by default.
    /// </summary>
    [Fact]
    public void ForwardedHeadersOptions_EnableExpectedHeadersByDefault()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>());

        ForwardedHeadersOptions options = GetForwardedHeadersOptions(factory);

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto));
        Assert.False(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost));
    }

    /// <summary>
    /// Verifies that forwarded headers processing is disabled when template forwarded headers are disabled.
    /// </summary>
    [Fact]
    public void ForwardedHeadersOptions_DisablesForwardedHeaders_WhenConfigurationIsDisabled()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:ForwardedHeaders:Enabled"] = "false"
        });

        ForwardedHeadersOptions options = GetForwardedHeadersOptions(factory);

        Assert.Equal(ForwardedHeaders.None, options.ForwardedHeaders);
    }

    /// <summary>
    /// Verifies that configured known proxy IP addresses are bound into ASP.NET Core forwarded headers options.
    /// </summary>
    [Fact]
    public void ForwardedHeadersOptions_BindsKnownProxiesFromConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:ForwardedHeaders:ClearKnownNetworksAndProxies"] = "true",
            ["Template:ForwardedHeaders:KnownProxies:0"] = "203.0.113.10",
            ["Template:ForwardedHeaders:KnownProxies:1"] = "2001:db8::1"
        });

        ForwardedHeadersOptions options = GetForwardedHeadersOptions(factory);

        Assert.Equal(2, options.KnownProxies.Count);
        Assert.Contains(IPAddress.Parse("203.0.113.10"), options.KnownProxies);
        Assert.Contains(IPAddress.Parse("2001:db8::1"), options.KnownProxies);
    }

    /// <summary>
    /// Verifies that configured known proxy networks are bound into ASP.NET Core forwarded headers options.
    /// </summary>
    [Fact]
    public void ForwardedHeadersOptions_BindsKnownNetworksFromConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:ForwardedHeaders:ClearKnownNetworksAndProxies"] = "true",
            ["Template:ForwardedHeaders:KnownNetworks:0"] = "10.10.0.0/16",
            ["Template:ForwardedHeaders:KnownNetworks:1"] = "2001:db8::/64"
        });

        ForwardedHeadersOptions options = GetForwardedHeadersOptions(factory);

        Assert.Equal(2, options.KnownIPNetworks.Count);
        Assert.Contains(options.KnownIPNetworks, network => network.Contains(IPAddress.Parse("10.10.1.25")));
        Assert.Contains(options.KnownIPNetworks, network => network.Contains(IPAddress.Parse("2001:db8::1234")));
    }

    /// <summary>
    /// Verifies that forwarded host support and allowed hosts are explicitly configured together.
    /// </summary>
    [Fact]
    public void ForwardedHeadersOptions_BindsAllowedHosts_WhenForwardedHostIsEnabled()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:ForwardedHeaders:Headers:0"] = "XForwardedFor",
            ["Template:ForwardedHeaders:Headers:1"] = "XForwardedProto",
            ["Template:ForwardedHeaders:Headers:2"] = "XForwardedHost",
            ["Template:ForwardedHeaders:AllowedHosts:0"] = "app.example.com",
            ["Template:ForwardedHeaders:AllowedHosts:1"] = "www.example.com"
        });

        ForwardedHeadersOptions options = GetForwardedHeadersOptions(factory);

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost));
        Assert.Equal(["app.example.com", "www.example.com"], options.AllowedHosts);
    }

    /// <summary>
    /// Verifies that the template forwarded headers options model is bound from configuration.
    /// </summary>
    [Fact]
    public void TemplateForwardedHeadersOptions_AreBoundFromConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:ForwardedHeaders:Enabled"] = "true",
            ["Template:ForwardedHeaders:Headers:0"] = "XForwardedFor",
            ["Template:ForwardedHeaders:Headers:1"] = "XForwardedProto",
            ["Template:ForwardedHeaders:ForwardLimit"] = "2",
            ["Template:ForwardedHeaders:RequireHeaderSymmetry"] = "true",
            ["Template:ForwardedHeaders:ClearKnownNetworksAndProxies"] = "true",
            ["Template:ForwardedHeaders:KnownProxies:0"] = "203.0.113.10",
            ["Template:ForwardedHeaders:KnownNetworks:0"] = "10.10.0.0/16"
        });

        TemplateForwardedHeadersOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateForwardedHeadersOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.Contains("XForwardedFor", options.Headers);
        Assert.Contains("XForwardedProto", options.Headers);
        Assert.Equal(2, options.ForwardLimit);
        Assert.True(options.RequireHeaderSymmetry);
        Assert.True(options.ClearKnownNetworksAndProxies);
        Assert.Equal(["203.0.113.10"], options.KnownProxies);
        Assert.Equal(["10.10.0.0/16"], options.KnownNetworks);
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

    /// <summary>
    /// Gets the configured ASP.NET Core forwarded headers options from the test application factory.
    /// </summary>
    /// <param name="factory">The test application factory.</param>
    /// <returns>The configured <see cref="ForwardedHeadersOptions"/> instance.</returns>
    private static ForwardedHeadersOptions GetForwardedHeadersOptions(TemplateWebApplicationFactory factory)
    {
        return factory.Services
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;
    }
}
