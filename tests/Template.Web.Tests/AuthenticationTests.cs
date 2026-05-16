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
            ["Template:Authentication:Providers:OpenIdConnect:Scheme"] = "OpenIdConnect",
            ["Template:Authentication:Providers:OpenIdConnect:DisplayName"] = "OpenID Connect",
            ["Template:Authentication:Providers:OpenIdConnect:Authority"] = "https://login.example.test",
            ["Template:Authentication:Providers:OpenIdConnect:ClientId"] = "test-oidc-client-id",
            ["Template:Authentication:Providers:OpenIdConnect:CallbackPath"] = "/signin-oidc",
            ["Template:Authentication:Providers:OpenIdConnect:ResponseType"] = "code",
            ["Template:Authentication:Providers:OpenIdConnect:Scopes:0"] = "openid",
            ["Template:Authentication:Providers:OpenIdConnect:Scopes:1"] = "profile",
            ["Template:Authentication:Providers:OpenIdConnect:Scopes:2"] = "email",

            ["Template:Authentication:Providers:Saml2:Enabled"] = "true",
            ["Template:Authentication:Providers:Saml2:Scheme"] = "Saml2",
            ["Template:Authentication:Providers:Saml2:DisplayName"] = "SAML2",
            ["Template:Authentication:Providers:Saml2:EntityId"] = "https://template.example.test/saml2",
            ["Template:Authentication:Providers:Saml2:MetadataUrl"] = "https://idp.example.test/metadata",
            ["Template:Authentication:Providers:Saml2:ModulePath"] = "/custom-saml2-acs",

            ["Template:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["Template:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["Template:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["Template:Authentication:Providers:Microsoft:ClientId"] = "test-microsoft-client-id",
            ["Template:Authentication:Providers:Microsoft:ClientSecret"] = "test-microsoft-client-secret",
            ["Template:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft",

            ["Template:Authentication:Providers:Google:Enabled"] = "true",
            ["Template:Authentication:Providers:Google:Scheme"] = "Google",
            ["Template:Authentication:Providers:Google:DisplayName"] = "Google",
            ["Template:Authentication:Providers:Google:ClientId"] = "test-google-client-id",
            ["Template:Authentication:Providers:Google:ClientSecret"] = "test-google-client-secret",
            ["Template:Authentication:Providers:Google:CallbackPath"] = "/signin-google",

            ["Template:Authentication:Providers:GitHub:Enabled"] = "true",
            ["Template:Authentication:Providers:GitHub:Scheme"] = "GitHub",
            ["Template:Authentication:Providers:GitHub:DisplayName"] = "GitHub",
            ["Template:Authentication:Providers:GitHub:ClientId"] = "test-github-client-id",
            ["Template:Authentication:Providers:GitHub:ClientSecret"] = "test-github-client-secret",
            ["Template:Authentication:Providers:GitHub:CallbackPath"] = "/signin-github"
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
        Assert.Equal("/custom-saml2-acs", options.Providers.Saml2.ModulePath);
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
    /// Verifies that the OpenIdConnect authentication provider does not require configuration when it is disabled.
    /// </summary>
    /// <remarks>This test ensures that when the OpenIdConnect provider is explicitly disabled in the
    /// configuration, missing or empty configuration values for authority and client ID do not cause errors. Use this
    /// test to validate that the application can start without OpenIdConnect settings when the provider is not
    /// enabled.</remarks>
    [Fact]
    public void DisabledOpenIdConnectProvider_DoesNotRequireConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Providers:OpenIdConnect:Enabled"] = "false",
            ["Template:Authentication:Providers:OpenIdConnect:Authority"] = "",
            ["Template:Authentication:Providers:OpenIdConnect:ClientId"] = ""
        });

        TemplateAuthenticationOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
            .Value;

        Assert.False(options.Providers.OpenIdConnect.Enabled);
    }

    /// <summary>
    /// Verifies that enabling the OpenIdConnect authentication provider without specifying an authority causes
    /// application startup to fail with an options validation exception.
    /// </summary>
    /// <remarks>This test ensures that the configuration is validated and that the authority setting is
    /// required when the OpenIdConnect provider is enabled. It helps prevent misconfiguration at startup by asserting
    /// that a missing authority results in a clear validation error.</remarks>
    [Fact]
    public void EnabledOpenIdConnectProvider_MissingAuthority_FailsStartup()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Providers:OpenIdConnect:Enabled"] = "true",
            ["Template:Authentication:Providers:OpenIdConnect:Authority"] = "",
            ["Template:Authentication:Providers:OpenIdConnect:ClientId"] = "test-client-id",
            ["Template:Authentication:Providers:OpenIdConnect:CallbackPath"] = "/signin-oidc",
            ["Template:Authentication:Providers:OpenIdConnect:ResponseType"] = "code",
            ["Template:Authentication:Providers:OpenIdConnect:Scopes:0"] = "openid"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "Template:Authentication:Providers:OpenIdConnect:Authority is required when the provider is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that application startup fails with an OptionsValidationException when the SAML2 provider is enabled
    /// but the MetadataUrl configuration is missing or empty.
    /// </summary>
    /// <remarks>This test ensures that the SAML2 authentication provider enforces the requirement for a
    /// non-empty MetadataUrl when enabled. It validates that misconfiguration is detected early during application
    /// startup, preventing the application from running with incomplete authentication settings.</remarks>
    [Fact]
    public void EnabledSaml2Provider_MissingMetadataUrl_FailsStartup()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Providers:Saml2:Enabled"] = "true",
            ["Template:Authentication:Providers:Saml2:EntityId"] = "https://template.example/saml2",
            ["Template:Authentication:Providers:Saml2:MetadataUrl"] = "",
            ["Template:Authentication:Providers:Saml2:ModulePath"] = "/Saml2/Acs"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "Template:Authentication:Providers:Saml2:MetadataUrl is required when the provider is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that application startup fails with an options validation exception when the Microsoft authentication
    /// provider is enabled but the client secret is missing.
    /// </summary>
    /// <remarks>This test ensures that the configuration for the Microsoft authentication provider enforces
    /// the requirement for a client secret when the provider is enabled. It validates that an appropriate exception is
    /// thrown and that the error message clearly indicates the missing configuration.</remarks>
    [Fact]
    public void EnabledMicrosoftProvider_MissingClientSecret_FailsStartup()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["Template:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["Template:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["Template:Authentication:Providers:Microsoft:ClientId"] = "test-client-id",
            ["Template:Authentication:Providers:Microsoft:ClientSecret"] = "",
            ["Template:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "Template:Authentication:Providers:Microsoft:ClientSecret is required when the provider is enabled",
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
