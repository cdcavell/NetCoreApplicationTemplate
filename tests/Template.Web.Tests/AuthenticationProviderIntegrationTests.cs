using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Web.Authentication.Extensions;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration tests for authentication provider registration through the template authentication module.
/// </summary>
public sealed class AuthenticationProviderIntegrationTests
{
    /// <summary>
    /// Verifies that disabled external providers do not register provider authentication schemes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledExternalProviders_DoNotRegisterProviderSchemes()
    {
        using ServiceProvider services = CreateServiceProvider(CreateDisabledExternalProvidersConfiguration());

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        string[] providerSchemes =
        [
            "OpenIdConnect",
            "Saml2",
            "Microsoft",
            "Google",
            "GitHub"
        ];

        foreach (string providerScheme in providerSchemes)
        {
            AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync(providerScheme);

            Assert.Null(scheme);
        }
    }

    /// <summary>
    /// Verifies that the OpenID Connect provider registers the configured scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledOpenIdConnectProvider_RegistersConfiguredScheme()
    {
        Dictionary<string, string?> configurationValues = CreateBaseAuthenticationConfiguration();

        configurationValues["Template:Authentication:Providers:OpenIdConnect:Enabled"] = "true";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:Scheme"] = "OpenIdConnect";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:DisplayName"] = "OpenID Connect";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:Authority"] = "https://login.example.test";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:ClientId"] = "test-oidc-client-id";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:ClientSecret"] = "test-oidc-client-secret";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:CallbackPath"] = "/signin-oidc";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:ResponseType"] = "code";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:Scopes:0"] = "openid";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:Scopes:1"] = "profile";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:Scopes:2"] = "email";

        using ServiceProvider services = CreateServiceProvider(configurationValues);

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("OpenIdConnect");

        Assert.NotNull(scheme);
        Assert.Equal("OpenIdConnect", scheme.Name);
        Assert.Equal("OpenID Connect", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that the SAML2 provider registers the configured scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledSaml2Provider_RegistersConfiguredScheme()
    {
        Dictionary<string, string?> configurationValues = CreateBaseAuthenticationConfiguration();

        configurationValues["Template:Authentication:Providers:Saml2:Enabled"] = "true";
        configurationValues["Template:Authentication:Providers:Saml2:Scheme"] = "Saml2";
        configurationValues["Template:Authentication:Providers:Saml2:DisplayName"] = "SAML2";
        configurationValues["Template:Authentication:Providers:Saml2:EntityId"] = "https://template.example.test/saml2";
        configurationValues["Template:Authentication:Providers:Saml2:MetadataUrl"] = "https://idp.example.test/metadata";
        configurationValues["Template:Authentication:Providers:Saml2:ModulePath"] = "/Saml2/Acs";
        configurationValues["Template:Authentication:Providers:Saml2:LoadMetadata"] = "false";

        using ServiceProvider services = CreateServiceProvider(configurationValues);

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Saml2");

        Assert.NotNull(scheme);
        Assert.Equal("Saml2", scheme.Name);
        Assert.Equal("SAML2", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that the Microsoft provider registers the configured scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledMicrosoftProvider_RegistersConfiguredScheme()
    {
        Dictionary<string, string?> configurationValues = CreateBaseAuthenticationConfiguration();

        configurationValues["Template:Authentication:Providers:Microsoft:Enabled"] = "true";
        configurationValues["Template:Authentication:Providers:Microsoft:Scheme"] = "Microsoft";
        configurationValues["Template:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft";
        configurationValues["Template:Authentication:Providers:Microsoft:ClientId"] = "test-microsoft-client-id";
        configurationValues["Template:Authentication:Providers:Microsoft:ClientSecret"] = "test-microsoft-client-secret";
        configurationValues["Template:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft";

        using ServiceProvider services = CreateServiceProvider(configurationValues);

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Microsoft");

        Assert.NotNull(scheme);
        Assert.Equal("Microsoft", scheme.Name);
        Assert.Equal("Microsoft", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that the Google provider registers the configured scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledGoogleProvider_RegistersConfiguredScheme()
    {
        Dictionary<string, string?> configurationValues = CreateBaseAuthenticationConfiguration();

        configurationValues["Template:Authentication:Providers:Google:Enabled"] = "true";
        configurationValues["Template:Authentication:Providers:Google:Scheme"] = "Google";
        configurationValues["Template:Authentication:Providers:Google:DisplayName"] = "Google";
        configurationValues["Template:Authentication:Providers:Google:ClientId"] = "test-google-client-id";
        configurationValues["Template:Authentication:Providers:Google:ClientSecret"] = "test-google-client-secret";
        configurationValues["Template:Authentication:Providers:Google:CallbackPath"] = "/signin-google";
        configurationValues["Template:Authentication:Providers:Google:Scopes:0"] = "profile";
        configurationValues["Template:Authentication:Providers:Google:Scopes:1"] = "email";

        using ServiceProvider services = CreateServiceProvider(configurationValues);

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Google");

        Assert.NotNull(scheme);
        Assert.Equal("Google", scheme.Name);
        Assert.Equal("Google", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that the GitHub provider registers the configured scheme when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledGitHubProvider_RegistersConfiguredScheme()
    {
        Dictionary<string, string?> configurationValues = CreateBaseAuthenticationConfiguration();

        configurationValues["Template:Authentication:Providers:GitHub:Enabled"] = "true";
        configurationValues["Template:Authentication:Providers:GitHub:Scheme"] = "GitHub";
        configurationValues["Template:Authentication:Providers:GitHub:DisplayName"] = "GitHub";
        configurationValues["Template:Authentication:Providers:GitHub:ClientId"] = "test-github-client-id";
        configurationValues["Template:Authentication:Providers:GitHub:ClientSecret"] = "test-github-client-secret";
        configurationValues["Template:Authentication:Providers:GitHub:CallbackPath"] = "/signin-github";
        configurationValues["Template:Authentication:Providers:GitHub:Scopes:0"] = "user:email";

        using ServiceProvider services = CreateServiceProvider(configurationValues);

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("GitHub");

        Assert.NotNull(scheme);
        Assert.Equal("GitHub", scheme.Name);
        Assert.Equal("GitHub", scheme.DisplayName);
    }

    private static Dictionary<string, string?> CreateDisabledExternalProvidersConfiguration()
    {
        Dictionary<string, string?> configurationValues = CreateBaseAuthenticationConfiguration();

        configurationValues["Template:Authentication:Providers:OpenIdConnect:Enabled"] = "false";
        configurationValues["Template:Authentication:Providers:OpenIdConnect:Scheme"] = "OpenIdConnect";

        configurationValues["Template:Authentication:Providers:Saml2:Enabled"] = "false";
        configurationValues["Template:Authentication:Providers:Saml2:Scheme"] = "Saml2";

        configurationValues["Template:Authentication:Providers:Microsoft:Enabled"] = "false";
        configurationValues["Template:Authentication:Providers:Microsoft:Scheme"] = "Microsoft";

        configurationValues["Template:Authentication:Providers:Google:Enabled"] = "false";
        configurationValues["Template:Authentication:Providers:Google:Scheme"] = "Google";

        configurationValues["Template:Authentication:Providers:GitHub:Enabled"] = "false";
        configurationValues["Template:Authentication:Providers:GitHub:Scheme"] = "GitHub";

        return configurationValues;
    }

    private static Dictionary<string, string?> CreateBaseAuthenticationConfiguration()
    {
        return new Dictionary<string, string?>
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
            ["Template:Authentication:Cookie:ExpireMinutes"] = "60",
            ["Template:Authentication:Cookie:SlidingExpiration"] = "true"
        };
    }

    private static ServiceProvider CreateServiceProvider(IReadOnlyDictionary<string, string?> configurationValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        ServiceCollection services = [];
        services.AddLogging();
        services.AddTemplateAuthentication(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
