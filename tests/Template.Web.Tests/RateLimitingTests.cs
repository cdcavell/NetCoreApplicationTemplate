using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.Web.Options;
using Template.Web.Tests.Infrastructure;
using Template.Web.Tests.TestControllers;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration tests for the template rate limiting configuration and policies.
/// </summary>
public sealed class RateLimitingTests
{
    /// <summary>
    /// Verifies that the global fixed-window limiter rejects requests after the configured permit limit is exceeded.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GlobalFixedWindowLimiter_ReturnsTooManyRequests_WhenPermitLimitIsExceeded()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:RateLimiting:Enabled"] = "true",
            ["Template:RateLimiting:UseGlobalLimiter"] = "true",
            ["Template:RateLimiting:GlobalFixedWindow:PermitLimit"] = "1",
            ["Template:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "60",
            ["Template:RateLimiting:GlobalFixedWindow:QueueLimit"] = "0"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage firstResponse = await client.GetAsync("/");
        using HttpResponseMessage secondResponse = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that the named fixed-window policy rejects endpoint requests after the configured permit limit is exceeded.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task NamedFixedWindowPolicy_ReturnsTooManyRequests_WhenPermitLimitIsExceeded()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:RateLimiting:Enabled"] = "true",
            ["Template:RateLimiting:UseGlobalLimiter"] = "false",
            ["Template:RateLimiting:FixedWindowPolicy:PermitLimit"] = "1",
            ["Template:RateLimiting:FixedWindowPolicy:WindowSeconds"] = "60",
            ["Template:RateLimiting:FixedWindowPolicy:QueueLimit"] = "0"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage firstResponse = await client.GetAsync("/test/rate-limiting/fixed");
        using HttpResponseMessage secondResponse = await client.GetAsync("/test/rate-limiting/fixed");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that the named concurrency policy rejects a second request while the configured permit is already in use.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task NamedConcurrencyPolicy_ReturnsTooManyRequests_WhenConcurrentLimitIsExceeded()
    {
        RateLimitingTestController.ResetConcurrencySignal();

        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:RateLimiting:Enabled"] = "true",
            ["Template:RateLimiting:UseGlobalLimiter"] = "false",
            ["Template:RateLimiting:ConcurrencyPolicy:PermitLimit"] = "1",
            ["Template:RateLimiting:ConcurrencyPolicy:QueueLimit"] = "0"
        });

        using HttpClient client = CreateHttpsClient(factory);

        Task<HttpResponseMessage> firstRequest = client.GetAsync("/test/rate-limiting/concurrency");

        await RateLimitingTestController.WaitForConcurrencyRequestStartedAsync();

        using HttpResponseMessage secondResponse = await client.GetAsync("/test/rate-limiting/concurrency");
        using HttpResponseMessage firstResponse = await firstRequest;

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that rejected requests return the expected JSON 429 Too Many Requests response payload.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task RejectedRequest_ReturnsJsonTooManyRequestsResponseShape()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:RateLimiting:Enabled"] = "true",
            ["Template:RateLimiting:UseGlobalLimiter"] = "true",
            ["Template:RateLimiting:GlobalFixedWindow:PermitLimit"] = "1",
            ["Template:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "60",
            ["Template:RateLimiting:GlobalFixedWindow:QueueLimit"] = "0"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage firstResponse = await client.GetAsync("/");
        using HttpResponseMessage rejectedResponse = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
        Assert.Equal("application/json", rejectedResponse.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await rejectedResponse.Content.ReadAsStringAsync());

        Assert.Equal("Too many requests.", document.RootElement.GetProperty("error").GetString());
        Assert.Equal(429, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("traceId").GetString()));
    }

    /// <summary>
    /// Verifies that repeated requests are allowed when template rate limiting is disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledRateLimiting_DoesNotRejectRepeatedRequests()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:RateLimiting:Enabled"] = "false",
            ["Template:RateLimiting:UseGlobalLimiter"] = "true",
            ["Template:RateLimiting:GlobalFixedWindow:PermitLimit"] = "1",
            ["Template:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "60",
            ["Template:RateLimiting:GlobalFixedWindow:QueueLimit"] = "0"
        });

        using HttpClient client = CreateHttpsClient(factory);

        using HttpResponseMessage firstResponse = await client.GetAsync("/");
        using HttpResponseMessage secondResponse = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that template rate limiting options are bound from configuration into the options model.
    /// </summary>
    [Fact]
    public void RateLimitingOptions_AreBoundFromConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:RateLimiting:Enabled"] = "true",
            ["Template:RateLimiting:UseGlobalLimiter"] = "true",
            ["Template:RateLimiting:GlobalFixedWindow:PermitLimit"] = "7",
            ["Template:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "30",
            ["Template:RateLimiting:GlobalFixedWindow:QueueLimit"] = "2",
            ["Template:RateLimiting:FixedWindowPolicy:PermitLimit"] = "5",
            ["Template:RateLimiting:FixedWindowPolicy:WindowSeconds"] = "20",
            ["Template:RateLimiting:FixedWindowPolicy:QueueLimit"] = "1",
            ["Template:RateLimiting:ConcurrencyPolicy:PermitLimit"] = "3",
            ["Template:RateLimiting:ConcurrencyPolicy:QueueLimit"] = "1"
        });

        TemplateRateLimitingOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateRateLimitingOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.True(options.UseGlobalLimiter);

        Assert.Equal(7, options.GlobalFixedWindow.PermitLimit);
        Assert.Equal(30, options.GlobalFixedWindow.WindowSeconds);
        Assert.Equal(2, options.GlobalFixedWindow.QueueLimit);

        Assert.Equal(5, options.FixedWindowPolicy.PermitLimit);
        Assert.Equal(20, options.FixedWindowPolicy.WindowSeconds);
        Assert.Equal(1, options.FixedWindowPolicy.QueueLimit);

        Assert.Equal(3, options.ConcurrencyPolicy.PermitLimit);
        Assert.Equal(1, options.ConcurrencyPolicy.QueueLimit);
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
}
