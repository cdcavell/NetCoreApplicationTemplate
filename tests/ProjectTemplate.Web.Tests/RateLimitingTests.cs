using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Extensions;
using ProjectTemplate.Web.Options;
using ProjectTemplate.Web.Tests.Extensions;
using ProjectTemplate.Web.Tests.Infrastructure;
using ProjectTemplate.Web.Tests.TestControllers;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides integration tests for the application rate limiting configuration and policies.
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:RateLimiting:Enabled"] = "true",
            ["ProjectTemplate:RateLimiting:UseGlobalLimiter"] = "true",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:PermitLimit"] = "1",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "60",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:QueueLimit"] = "0"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage firstResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);
        using HttpResponseMessage secondResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);

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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:RateLimiting:Enabled"] = "true",
            ["ProjectTemplate:RateLimiting:UseGlobalLimiter"] = "false",
            ["ProjectTemplate:RateLimiting:FixedWindowPolicy:PermitLimit"] = "1",
            ["ProjectTemplate:RateLimiting:FixedWindowPolicy:WindowSeconds"] = "60",
            ["ProjectTemplate:RateLimiting:FixedWindowPolicy:QueueLimit"] = "0"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage firstResponse = await client.GetAsync("/test/rate-limiting/fixed", TestContext.Current.CancellationToken);
        using HttpResponseMessage secondResponse = await client.GetAsync("/test/rate-limiting/fixed", TestContext.Current.CancellationToken);

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

        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:RateLimiting:Enabled"] = "true",
            ["ProjectTemplate:RateLimiting:UseGlobalLimiter"] = "false",
            ["ProjectTemplate:RateLimiting:ConcurrencyPolicy:PermitLimit"] = "1",
            ["ProjectTemplate:RateLimiting:ConcurrencyPolicy:QueueLimit"] = "0"
        });

        using HttpClient client = factory.CreateHttpsClient();

        Task<HttpResponseMessage> firstRequest = client.GetAsync("/test/rate-limiting/concurrency", TestContext.Current.CancellationToken);

        await RateLimitingTestController.WaitForConcurrencyRequestStartedAsync();

        using HttpResponseMessage secondResponse = await client.GetAsync("/test/rate-limiting/concurrency", TestContext.Current.CancellationToken);
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:RateLimiting:Enabled"] = "true",
            ["ProjectTemplate:RateLimiting:UseGlobalLimiter"] = "true",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:PermitLimit"] = "1",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "60",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:QueueLimit"] = "0"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage firstResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);
        using HttpResponseMessage rejectedResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
        Assert.Equal("application/json", rejectedResponse.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await rejectedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        Assert.Equal("Too many requests.", document.RootElement.GetProperty("error").GetString());
        Assert.Equal(429, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("traceId").GetString()));
    }

    /// <summary>
    /// Verifies that repeated requests are allowed when application rate limiting is disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledRateLimiting_DoesNotRejectRepeatedRequests()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:RateLimiting:Enabled"] = "false",
            ["ProjectTemplate:RateLimiting:UseGlobalLimiter"] = "true",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:PermitLimit"] = "1",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "60",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:QueueLimit"] = "0"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage firstResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);
        using HttpResponseMessage secondResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that application rate limiting options are bound from configuration into the options model.
    /// </summary>
    [Fact]
    public void RateLimitingOptions_AreBoundFromConfiguration()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:RateLimiting:Enabled"] = "true",
            ["ProjectTemplate:RateLimiting:UseGlobalLimiter"] = "true",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:PermitLimit"] = "7",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "30",
            ["ProjectTemplate:RateLimiting:GlobalFixedWindow:QueueLimit"] = "2",
            ["ProjectTemplate:RateLimiting:FixedWindowPolicy:PermitLimit"] = "5",
            ["ProjectTemplate:RateLimiting:FixedWindowPolicy:WindowSeconds"] = "20",
            ["ProjectTemplate:RateLimiting:FixedWindowPolicy:QueueLimit"] = "1",
            ["ProjectTemplate:RateLimiting:ConcurrencyPolicy:PermitLimit"] = "3",
            ["ProjectTemplate:RateLimiting:ConcurrencyPolicy:QueueLimit"] = "1"
        });

        ApplicationRateLimitingOptions options = factory.Services
            .GetRequiredService<IOptions<ApplicationRateLimitingOptions>>()
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
    /// Verifies that startup validation fails when a fixed-window permit limit is zero.
    /// </summary>
    [Fact]
    public void RateLimiting_ZeroPermitLimit_FailsStartup()
    {
        OptionsValidationException exception =
            AssertRateLimitingOptionsValidationFails(
                new Dictionary<string, string?>
                {
                    ["ProjectTemplate:RateLimiting:Enabled"] = "true",
                    ["ProjectTemplate:RateLimiting:GlobalFixedWindow:PermitLimit"] = "0"
                });

        Assert.Contains(
            "ProjectTemplate:RateLimiting:GlobalFixedWindow:PermitLimit must be greater than zero",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that startup validation fails when a rate-limiting queue limit is negative.
    /// </summary>
    [Fact]
    public void RateLimiting_NegativeQueueLimit_FailsStartup()
    {
        OptionsValidationException exception =
            AssertRateLimitingOptionsValidationFails(
                new Dictionary<string, string?>
                {
                    ["ProjectTemplate:RateLimiting:Enabled"] = "true",
                    ["ProjectTemplate:RateLimiting:GlobalFixedWindow:QueueLimit"] = "-1"
                });

        Assert.Contains(
            "ProjectTemplate:RateLimiting:GlobalFixedWindow:QueueLimit must be zero or greater",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that startup validation fails when a fixed-window duration is zero seconds.
    /// </summary>
    [Fact]
    public void RateLimiting_ZeroWindowSeconds_FailsStartup()
    {
        OptionsValidationException exception =
            AssertRateLimitingOptionsValidationFails(
                new Dictionary<string, string?>
                {
                    ["ProjectTemplate:RateLimiting:Enabled"] = "true",
                    ["ProjectTemplate:RateLimiting:GlobalFixedWindow:WindowSeconds"] = "0"
                });

        Assert.Contains(
            "ProjectTemplate:RateLimiting:GlobalFixedWindow:WindowSeconds must be greater than zero",
            exception.Message,
            StringComparison.Ordinal);
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

    private static OptionsValidationException AssertRateLimitingOptionsValidationFails(
        IReadOnlyDictionary<string, string?> configurationValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        IServiceCollection services = new ServiceCollection();

        _ = services.AddApplicationRateLimiting(
            configuration,
            new TestHostEnvironment());

        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        return Assert.Throws<OptionsValidationException>(() =>
            provider
                .GetRequiredService<IOptions<ApplicationRateLimitingOptions>>()
                .Value);
    }

    private static OptionsValidationException? FindOptionsValidationException(Exception exception)
    {
        if (exception is OptionsValidationException optionsValidationException)
        {
            return optionsValidationException;
        }

        if (exception is AggregateException aggregateException)
        {
            foreach (Exception innerException in aggregateException.Flatten().InnerExceptions)
            {
                OptionsValidationException? foundException =
                    FindOptionsValidationException(innerException);

                if (foundException is not null)
                {
                    return foundException;
                }
            }
        }

        return exception.InnerException is null
            ? null
            : FindOptionsValidationException(exception.InnerException);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";

        public string ApplicationName { get; set; } = "ProjectTemplate.Web.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
