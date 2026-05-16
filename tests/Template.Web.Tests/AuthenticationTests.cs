using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.Web.Authentication.Options;
using Template.Web.Tests.Extensions;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration tests for the template authentication configuration and baseline cookie authentication behavior.
/// </summary>
public sealed class AuthenticationTests
{
    /// <summary>
    /// Verifies that authentication options are bound from configuration into the options model.
    /// </summary>
    [Fact]
    public void AuthenticationOptions_AreBoundFromConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:DefaultScheme"] = "Cookies",
            ["Template:Authentication:DefaultChallengeScheme"] = "Cookies",
            ["Template:Authentication:DefaultSignInScheme"] = "Cookies",
            ["Template:Authentication:Cookie:Enabled"] = "true",
            ["Template:Authentication:Cookie:Scheme"] = "Cookies",
            ["Template:Authentication:Cookie:LoginPath"] = "/Account/Login",
            ["Template:Authentication:Cookie:LogoutPath"] = "/Account/Logout",
            ["Template:Authentication:Cookie:AccessDeniedPath"] = "/Account/AccessDenied",
            ["Template:Authentication:Cookie:ExpireMinutes"] = "90",
            ["Template:Authentication:Cookie:SlidingExpiration"] = "false",
            ["Template:Authentication:Providers:OpenIdConnect:Enabled"] = "true",
            ["Template:Authentication:Providers:Saml2:Enabled"] = "true",
            ["Template:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["Template:Authentication:Providers:Google:Enabled"] = "true",
            ["Template:Authentication:Providers:GitHub:Enabled"] = "true"
        });

        TemplateAuthenticationOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.Equal("Cookies", options.DefaultScheme);
        Assert.Equal("Cookies", options.DefaultChallengeScheme);
        Assert.Equal("Cookies", options.DefaultSignInScheme);

        Assert.True(options.Cookie.Enabled);
        Assert.Equal("Cookies", options.DefaultChallengeScheme);
        Assert.Equal("Cookies", options.Cookie.Scheme);
        Assert.Equal("/Account/Login", options.Cookie.LoginPath);
        Assert.Equal("/Account/Logout", options.Cookie.LogoutPath);
        Assert.Equal("/Account/AccessDenied", options.Cookie.AccessDeniedPath);
        Assert.Equal(90, options.Cookie.ExpireMinutes);
        Assert.False(options.Cookie.SlidingExpiration);

        Assert.True(options.Providers.OpenIdConnect.Enabled);
        Assert.True(options.Providers.Saml2.Enabled);
        Assert.True(options.Providers.Microsoft.Enabled);
        Assert.True(options.Providers.Google.Enabled);
        Assert.True(options.Providers.GitHub.Enabled);
    }

    /// <summary>
    /// Verifies that template authentication and cookie authentication are enabled by default for the base template.
    /// </summary>
    [Fact]
    public void AuthenticationOptions_AreEnabledByDefault()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>());

        TemplateAuthenticationOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
            .Value;

        Assert.True(options.Enabled);
        Assert.True(options.Cookie.Enabled);
        Assert.Equal("Cookies", options.DefaultScheme);
        Assert.Equal("Cookies", options.Cookie.Scheme);
    }

    /// <summary>
    /// Verifies that anonymous endpoints remain accessible when authentication is disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledAuthentication_AllowsAnonymousEndpoint()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "false"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/anonymous",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that cookie authentication is registered when template authentication is enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledAuthentication_RegistersCookieScheme()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:DefaultScheme"] = "Cookies",
            ["Template:Authentication:DefaultChallengeScheme"] = "Cookies",
            ["Template:Authentication:DefaultSignInScheme"] = "Cookies",
            ["Template:Authentication:Cookie:Enabled"] = "true",
            ["Template:Authentication:Cookie:Scheme"] = "Cookies"
        });

        IAuthenticationSchemeProvider schemeProvider = factory.Services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Cookies");

        Assert.NotNull(scheme);
    }

    /// <summary>
    /// Verifies that a protected API endpoint returns unauthorized for unauthenticated users when cookie authentication is enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task ProtectedApiEndpoint_ReturnsUnauthorized_WhenCookieAuthenticationIsEnabled()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:DefaultScheme"] = "Cookies",
            ["Template:Authentication:DefaultChallengeScheme"] = "Cookies",
            ["Template:Authentication:DefaultSignInScheme"] = "Cookies",
            ["Template:Authentication:Cookie:Enabled"] = "true",
            ["Template:Authentication:Cookie:Scheme"] = "Cookies",
            ["Template:Authentication:Cookie:LoginPath"] = "/Account/Login"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/protected",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Found, response.StatusCode);

        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/Account/Login", response.Headers.Location.LocalPath);
        Assert.Contains("ReturnUrl=", response.Headers.Location.Query, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that startup validation fails when authentication is enabled and the cookie scheme is empty.
    /// </summary>
    [Fact]
    public void Authentication_EmptyCookieScheme_FailsStartup()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:Cookie:Enabled"] = "true",
            ["Template:Authentication:Cookie:Scheme"] = ""
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "Template:Authentication:Cookie:Scheme is required when template authentication is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that startup validation fails when authentication is enabled and cookie expiration is zero.
    /// </summary>
    [Fact]
    public void Authentication_ZeroCookieExpiration_FailsStartup()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:Cookie:ExpireMinutes"] = "0"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "Template:Authentication:Cookie:ExpireMinutes must be greater than zero when template authentication is enabled",
            exception.Message,
            StringComparison.Ordinal);
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
}
