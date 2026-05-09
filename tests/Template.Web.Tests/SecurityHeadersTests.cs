using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration tests for configurable security header middleware behavior.
/// </summary>
public sealed class SecurityHeadersTests
{
    /// <summary>
    /// Verifies that default security headers are applied when security headers are enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DefaultSecurityHeaders_AreApplied()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:SecurityHeaders:Enabled"] = "true"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage response = await client.GetAsync("/test/security-headers", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AssertHeader(response, "X-Content-Type-Options", "nosniff");
        AssertHeader(response, "X-Frame-Options", "DENY");
        AssertHeader(response, "Referrer-Policy", "strict-origin-when-cross-origin");
        AssertHeader(response, "X-Permitted-Cross-Domain-Policies", "none");
        AssertHeader(response, "Cross-Origin-Opener-Policy", "same-origin");
        AssertHeader(response, "Cross-Origin-Resource-Policy", "same-origin");
        AssertHeader(response, "Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=(), fullscreen=(self)");
        AssertHeader(
            response,
            "Content-Security-Policy",
            "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; img-src 'self' data:; script-src 'self'; style-src 'self' 'unsafe-inline';");

        AssertHeaderMissing(response, "X-XSS-Protection");
    }

    /// <summary>
    /// Verifies that the Content-Security-Policy header is not emitted when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledContentSecurityPolicy_DoesNotEmitContentSecurityPolicyHeader()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:SecurityHeaders:Enabled"] = "true",
            ["Template:SecurityHeaders:EnableContentSecurityPolicy"] = "false"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage response = await client.GetAsync("/test/security-headers", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AssertHeader(response, "X-Content-Type-Options", "nosniff");
        AssertHeaderMissing(response, "Content-Security-Policy");
    }

    /// <summary>
    /// Verifies that the Permissions-Policy header is not emitted when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledPermissionsPolicy_DoesNotEmitPermissionsPolicyHeader()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:SecurityHeaders:Enabled"] = "true",
            ["Template:SecurityHeaders:EnablePermissionsPolicy"] = "false"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage response = await client.GetAsync("/test/security-headers", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AssertHeader(response, "X-Content-Type-Options", "nosniff");
        AssertHeaderMissing(response, "Permissions-Policy");
    }

    /// <summary>
    /// Verifies that cross-origin headers are not emitted when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledCrossOriginHeaders_DoNotEmitCrossOriginHeaders()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:SecurityHeaders:Enabled"] = "true",
            ["Template:SecurityHeaders:EnableCrossOriginHeaders"] = "false"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage response = await client.GetAsync("/test/security-headers", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AssertHeader(response, "X-Content-Type-Options", "nosniff");
        AssertHeaderMissing(response, "Cross-Origin-Opener-Policy");
        AssertHeaderMissing(response, "Cross-Origin-Resource-Policy");
    }

    /// <summary>
    /// Verifies that configured excluded path prefixes do not receive security headers.
    /// </summary>
    /// <param name="path">The excluded request path to verify.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [InlineData("/health")]
    [InlineData("/metrics")]
    public async Task ExcludedPathPrefixes_DoNotApplySecurityHeaders(string path)
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:SecurityHeaders:Enabled"] = "true",
            ["Template:SecurityHeaders:ExcludedPathPrefixes:0"] = "/health",
            ["Template:SecurityHeaders:ExcludedPathPrefixes:1"] = "/metrics"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AssertSecurityHeadersMissing(response);
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
    /// Creates an HTTPS test client so requests are not intercepted by HTTPS redirection middleware.
    /// </summary>
    /// <param name="factory">The web application factory used to create the client.</param>
    /// <returns>An <see cref="HttpClient"/> configured with an HTTPS base address.</returns>
    private static HttpClient CreateHttpsClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static void AssertSecurityHeadersMissing(HttpResponseMessage response)
    {
        AssertHeaderMissing(response, "X-Content-Type-Options");
        AssertHeaderMissing(response, "X-Frame-Options");
        AssertHeaderMissing(response, "Referrer-Policy");
        AssertHeaderMissing(response, "X-Permitted-Cross-Domain-Policies");
        AssertHeaderMissing(response, "Cross-Origin-Opener-Policy");
        AssertHeaderMissing(response, "Cross-Origin-Resource-Policy");
        AssertHeaderMissing(response, "Permissions-Policy");
        AssertHeaderMissing(response, "Content-Security-Policy");
        AssertHeaderMissing(response, "X-XSS-Protection");
    }

    private static void AssertHeader(HttpResponseMessage response, string name, string expectedValue)
    {
        Assert.True(response.Headers.TryGetValues(name, out IEnumerable<string>? values), $"Expected response header '{name}'.");

        string actualValue = string.Join(", ", values);

        Assert.Equal(expectedValue, actualValue);
    }

    private static void AssertHeaderMissing(HttpResponseMessage response, string name)
    {
        Assert.False(response.Headers.Contains(name), $"Did not expect response header '{name}'.");
    }
}
