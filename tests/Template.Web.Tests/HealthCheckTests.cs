using System.Net;
using Template.Web.Tests.Extensions;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration tests for baseline health check endpoints.
/// </summary>
public sealed class HealthCheckTests
{
    /// <summary>
    /// Verifies that the baseline health endpoint returns a healthy response.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        using TemplateWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Healthy", body, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that the readiness health endpoint returns a healthy response.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task HealthReadyEndpoint_ReturnsHealthy()
    {
        using TemplateWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Healthy", body);
    }

    /// <summary>
    /// Verifies that the liveness health endpoint returns a healthy response.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task HealthLiveEndpoint_ReturnsHealthy()
    {
        using TemplateWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Healthy", body);
    }

    /// <summary>
    /// Verifies that health endpoints do not receive configured security headers.
    /// </summary>
    /// <param name="path">The health check path to test.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    public async Task HealthEndpoints_DoNotApplySecurityHeaders(string path)
    {
        using TemplateWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.False(response.Headers.Contains("X-Content-Type-Options"));
        Assert.False(response.Headers.Contains("X-Frame-Options"));
        Assert.False(response.Headers.Contains("Referrer-Policy"));
        Assert.False(response.Headers.Contains("X-Permitted-Cross-Domain-Policies"));
        Assert.False(response.Headers.Contains("Cross-Origin-Opener-Policy"));
        Assert.False(response.Headers.Contains("Cross-Origin-Resource-Policy"));
        Assert.False(response.Headers.Contains("Permissions-Policy"));
        Assert.False(response.Headers.Contains("Content-Security-Policy"));
    }

    private static TemplateWebApplicationFactory CreateFactory()
    {
        return new TemplateWebApplicationFactory(new Dictionary<string, string?>());
    }
}
