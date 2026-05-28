using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Authentication.Options;
using ProjectTemplate.Web.Authentication.Providers.GitHub;
using ProjectTemplate.Web.Authentication.Providers.Google;
using ProjectTemplate.Web.Tests.Extensions;
using ProjectTemplate.Web.Tests.Infrastructure;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides integration tests for the application authentication configuration and baseline cookie authentication behavior.
/// </summary>
public sealed class AuthenticationTests
{
    /// <summary>
    /// Verifies that authentication options are bound from configuration into the options model.
    /// </summary>
    [Fact]
    public void AuthenticationOptions_AreBoundFromConfiguration()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:DefaultScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultChallengeScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultSignInScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Scheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Cookie:LoginPath"] = "/Account/Login",
            ["ProjectTemplate:Authentication:Cookie:LogoutPath"] = "/Account/Logout",
            ["ProjectTemplate:Authentication:Cookie:AccessDeniedPath"] = "/Account/AccessDenied",
            ["ProjectTemplate:Authentication:Cookie:ExpireMinutes"] = "90",
            ["ProjectTemplate:Authentication:Cookie:SlidingExpiration"] = "false",

            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scheme"] = "OpenIdConnect",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:DisplayName"] = "OpenID Connect",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Authority"] = "https://login.example.test",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ClientId"] = "test-oidc-client-id",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:CallbackPath"] = "/signin-oidc",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ResponseType"] = "code",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scopes:0"] = "openid",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scopes:1"] = "profile",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scopes:2"] = "email",

            ["ProjectTemplate:Authentication:Providers:Saml2:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Saml2:Scheme"] = "Saml2",
            ["ProjectTemplate:Authentication:Providers:Saml2:DisplayName"] = "SAML2",
            ["ProjectTemplate:Authentication:Providers:Saml2:EntityId"] = "https://ProjectTemplate.example.test/saml2",
            ["ProjectTemplate:Authentication:Providers:Saml2:MetadataUrl"] = "https://idp.example.test/metadata",
            ["ProjectTemplate:Authentication:Providers:Saml2:ModulePath"] = "/custom-saml2-acs",

            ["ProjectTemplate:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientId"] = "test-microsoft-client-id",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientSecret"] = "test-microsoft-client-secret",
            ["ProjectTemplate:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft",

            ["ProjectTemplate:Authentication:Providers:Google:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Google:Scheme"] = "Google",
            ["ProjectTemplate:Authentication:Providers:Google:DisplayName"] = "Google",
            ["ProjectTemplate:Authentication:Providers:Google:ClientId"] = "test-google-client-id",
            ["ProjectTemplate:Authentication:Providers:Google:ClientSecret"] = "test-google-client-secret",
            ["ProjectTemplate:Authentication:Providers:Google:CallbackPath"] = "/signin-google",

            ["ProjectTemplate:Authentication:Providers:GitHub:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:GitHub:Scheme"] = "GitHub",
            ["ProjectTemplate:Authentication:Providers:GitHub:DisplayName"] = "GitHub",
            ["ProjectTemplate:Authentication:Providers:GitHub:ClientId"] = "test-github-client-id",
            ["ProjectTemplate:Authentication:Providers:GitHub:ClientSecret"] = "test-github-client-secret",
            ["ProjectTemplate:Authentication:Providers:GitHub:CallbackPath"] = "/signin-github"
        });

        ApplicationAuthenticationOptions options = factory.Services
            .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
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
    /// Verifies that the authentication options match the expected values based on the provided configuration.
    /// </summary>
    [Fact]
    public void AuthenticationOptions_MatchGeneratedConfiguration()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>());

        IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();

        ApplicationAuthenticationOptions options = factory.Services
            .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
            .Value;

        bool expectedAuthenticationEnabled = bool.TryParse(
            configuration["ProjectTemplate:Authentication:Enabled"],
            out bool authenticationEnabled) && authenticationEnabled;

        bool expectedCookieEnabled = bool.TryParse(
            configuration["ProjectTemplate:Authentication:Cookie:Enabled"],
            out bool cookieEnabled) && cookieEnabled;

        Assert.Equal(expectedAuthenticationEnabled, options.Enabled);
        Assert.Equal(expectedCookieEnabled, options.Cookie.Enabled);
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "false"
        });

        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/anonymous",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that cookie authentication is registered when application authentication is enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledAuthentication_RegistersCookieScheme()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:DefaultScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultChallengeScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultSignInScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Scheme"] = "Cookies"
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:DefaultScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultChallengeScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultSignInScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Scheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Cookie:LoginPath"] = "/Account/Login"
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Scheme"] = ""
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Cookie:Scheme is required when application authentication is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that startup validation fails when authentication is enabled and cookie expiration is zero.
    /// </summary>
    [Fact]
    public void Authentication_ZeroCookieExpiration_FailsStartup()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:ExpireMinutes"] = "0"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Cookie:ExpireMinutes must be greater than zero when application authentication is enabled",
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Enabled"] = "false",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Authority"] = "",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ClientId"] = ""
        });

        ApplicationAuthenticationOptions options = factory.Services
            .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
            .Value;

        Assert.False(options.Providers.OpenIdConnect.Enabled);
    }

    /// <summary>
    /// Verifies that the OpenIdConnect provider does not register a scheme when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledOpenIdConnectProvider_DoesNotRegisterScheme()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Enabled"] = "false",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scheme"] = "OpenIdConnect",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:DisplayName"] = "OpenId Connect",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Authority"] = "",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ClientId"] = "test-client-id",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:CallbackPath"] = "/signin-oidc",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ResponseType"] = "code",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scopes:0"] = "openid"
        });

        IAuthenticationSchemeProvider schemeProvider = factory.Services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("OpenIdConnect");

        Assert.Null(scheme);
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Authority"] = "",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ClientId"] = "test-client-id",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:CallbackPath"] = "/signin-oidc",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:ResponseType"] = "code",
            ["ProjectTemplate:Authentication:Providers:OpenIdConnect:Scopes:0"] = "openid"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Providers:OpenIdConnect:Authority is required when the provider is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the SAML2 provider does not register a scheme when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledSaml2Provider_DoesNotRegisterScheme()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:Saml2:Enabled"] = "false",
            ["ProjectTemplate:Authentication:Providers:Saml2:Scheme"] = "Saml2",
            ["ProjectTemplate:Authentication:Providers:Saml2:DisplayName"] = "SAML2",
            ["ProjectTemplate:Authentication:Providers:Saml2:EntityId"] = "https://ProjectTemplate.example/saml2",
            ["ProjectTemplate:Authentication:Providers:Saml2:MetadataUrl"] = "",
            ["ProjectTemplate:Authentication:Providers:Saml2:ModulePath"] = "/Saml2/Acs"
        });

        IAuthenticationSchemeProvider schemeProvider = factory.Services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Saml2");

        Assert.Null(scheme);
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:Saml2:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Saml2:EntityId"] = "https://ProjectTemplate.example/saml2",
            ["ProjectTemplate:Authentication:Providers:Saml2:MetadataUrl"] = "",
            ["ProjectTemplate:Authentication:Providers:Saml2:ModulePath"] = "/Saml2/Acs"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Providers:Saml2:MetadataUrl is required when the provider is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the Microsoft authentication provider does not register a scheme when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledMicrosoftProvider_DoesNotRegisterScheme()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:Microsoft:Enabled"] = "false",
            ["ProjectTemplate:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientId"] = "",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientSecret"] = "",
            ["ProjectTemplate:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft"
        });

        IAuthenticationSchemeProvider schemeProvider = factory.Services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Microsoft");

        Assert.Null(scheme);
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
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientId"] = "test-client-id",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientSecret"] = "",
            ["ProjectTemplate:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Providers:Microsoft:ClientSecret is required when the provider is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the Google authentication provider does not register a scheme when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledGoogleProvider_DoesNotRegisterGoogleScheme()
    {
        ServiceCollection services = new();

        AuthenticationBuilder builder = services.AddAuthentication();

        builder.AddGoogleAuthentication(new ApplicationExternalAuthenticationProviderOptions
        {
            Enabled = false,
            Scheme = "Google",
            DisplayName = "Google",
            ClientId = "",
            ClientSecret = "",
            CallbackPath = "/signin-google"
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IAuthenticationSchemeProvider schemeProvider = serviceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Google");

        Assert.Null(scheme);
    }

    /// <summary>
    /// Verifies that the Google authentication provider registers a scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledGoogleProvider_RegistersGoogleScheme()
    {
        ServiceCollection services = new();

        AuthenticationBuilder builder = services.AddAuthentication();

        builder.AddGoogleAuthentication(new ApplicationExternalAuthenticationProviderOptions
        {
            Enabled = true,
            Scheme = "Google",
            DisplayName = "Google",
            ClientId = "test-google-client-id",
            ClientSecret = "test-google-client-secret",
            CallbackPath = "/signin-google",
            Scopes =
            [
                "profile",
            "email"
            ]
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IAuthenticationSchemeProvider schemeProvider = serviceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Google");

        Assert.NotNull(scheme);
        Assert.Equal("Google", scheme.Name);
        Assert.Equal("Google", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that application startup fails with an options validation exception when the Google authentication
    /// provider is enabled but the client secret is missing.
    /// </summary>
    [Fact]
    public void EnabledGoogleProvider_MissingClientSecret_FailsStartup()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:Google:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Google:Scheme"] = "Google",
            ["ProjectTemplate:Authentication:Providers:Google:DisplayName"] = "Google",
            ["ProjectTemplate:Authentication:Providers:Google:ClientId"] = "test-client-id",
            ["ProjectTemplate:Authentication:Providers:Google:ClientSecret"] = "",
            ["ProjectTemplate:Authentication:Providers:Google:CallbackPath"] = "/signin-google"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Providers:Google:ClientSecret is required when the provider is enabled",
            exception.Message,
            StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies that the GitHub authentication provider does not register a scheme when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledGitHubProvider_DoesNotRegisterGitHubScheme()
    {
        ServiceCollection services = new();

        AuthenticationBuilder builder = services.AddAuthentication();

        builder.AddGitHubAuthentication(new ApplicationExternalAuthenticationProviderOptions
        {
            Enabled = false,
            Scheme = "GitHub",
            DisplayName = "GitHub",
            ClientId = "",
            ClientSecret = "",
            CallbackPath = "/signin-github"
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IAuthenticationSchemeProvider schemeProvider = serviceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("GitHub");

        Assert.Null(scheme);
    }

    /// <summary>
    /// Verifies that the GitHub authentication provider registers a scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledGitHubProvider_RegistersGitHubScheme()
    {
        ServiceCollection services = new();

        AuthenticationBuilder builder = services.AddAuthentication();

        builder.AddGitHubAuthentication(new ApplicationExternalAuthenticationProviderOptions
        {
            Enabled = true,
            Scheme = "GitHub",
            DisplayName = "GitHub",
            ClientId = "test-github-client-id",
            ClientSecret = "test-github-client-secret",
            CallbackPath = "/signin-github",
            Scopes =
            [
                "profile",
                "email"
            ]
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IAuthenticationSchemeProvider schemeProvider = serviceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("GitHub");

        Assert.NotNull(scheme);
        Assert.Equal("GitHub", scheme.Name);
        Assert.Equal("GitHub", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that application startup fails with an options validation exception when the GitHub authentication
    /// provider is enabled but the client secret is missing.
    /// </summary>
    [Fact]
    public void EnabledGitHubProvider_MissingClientSecret_FailsStartup()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Providers:GitHub:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:GitHub:Scheme"] = "GitHub",
            ["ProjectTemplate:Authentication:Providers:GitHub:DisplayName"] = "GitHub",
            ["ProjectTemplate:Authentication:Providers:GitHub:ClientId"] = "test-client-id",
            ["ProjectTemplate:Authentication:Providers:GitHub:ClientSecret"] = "",
            ["ProjectTemplate:Authentication:Providers:GitHub:CallbackPath"] = "/signin-github"
        });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Providers:GitHub:ClientSecret is required when the provider is enabled",
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
}
