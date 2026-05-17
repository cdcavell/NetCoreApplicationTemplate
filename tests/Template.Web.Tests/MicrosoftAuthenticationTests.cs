using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.Web.Authentication.Extensions;
using Template.Web.Authentication.Options;

namespace Template.Web.Tests;

/// <summary>
/// Provides tests for Microsoft external authentication provider registration.
/// </summary>
public sealed class MicrosoftAuthenticationTests
{
    /// <summary>
    /// Verifies that the Microsoft authentication provider is not registered when disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task DisabledMicrosoftProvider_DoesNotRegisterMicrosoftScheme()
    {
        using ServiceProvider services = CreateServiceProvider(new Dictionary<string, string?>
        {
            ["Template:Authentication:Providers:Microsoft:Enabled"] = "false",
            ["Template:Authentication:Providers:Microsoft:ClientId"] = "",
            ["Template:Authentication:Providers:Microsoft:ClientSecret"] = ""
        });

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Microsoft");

        Assert.Null(scheme);
    }

    /// <summary>
    /// Verifies that the Microsoft authentication provider is registered when enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task EnabledMicrosoftProvider_RegistersMicrosoftScheme()
    {
        using ServiceProvider services = CreateServiceProvider(CreateEnabledMicrosoftConfiguration());

        IAuthenticationSchemeProvider schemeProvider = services
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync("Microsoft");

        Assert.NotNull(scheme);
        Assert.Equal("Microsoft", scheme.Name);
        Assert.Equal("Microsoft", scheme.DisplayName);
    }

    /// <summary>
    /// Verifies that Microsoft provider options are applied to the registered Microsoft handler.
    /// </summary>
    [Fact]
    public void EnabledMicrosoftProvider_ConfiguresMicrosoftHandlerOptions()
    {
        using ServiceProvider services = CreateServiceProvider(CreateEnabledMicrosoftConfiguration());

        IOptionsMonitor<MicrosoftAccountOptions> optionsMonitor = services
            .GetRequiredService<IOptionsMonitor<MicrosoftAccountOptions>>();

        MicrosoftAccountOptions options = optionsMonitor.Get("Microsoft");

        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, options.SignInScheme);
        Assert.Equal("test-microsoft-client-id", options.ClientId);
        Assert.Equal("test-microsoft-client-secret", options.ClientSecret);
        Assert.Equal("/signin-microsoft", options.CallbackPath);
        Assert.Contains("User.Read", options.Scope);
    }

    /// <summary>
    /// Verifies that Microsoft provider scopes are bound from template authentication configuration.
    /// </summary>
    [Fact]
    public void MicrosoftProviderScopes_AreBoundFromConfiguration()
    {
        using ServiceProvider services = CreateServiceProvider(CreateEnabledMicrosoftConfiguration());

        TemplateAuthenticationOptions options = services
            .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
            .Value;

        Assert.Equal(["User.Read"], options.Providers.Microsoft.Scopes);
    }

    private static Dictionary<string, string?> CreateEnabledMicrosoftConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:DefaultScheme"] = "Cookies",
            ["Template:Authentication:DefaultChallengeScheme"] = "Microsoft",
            ["Template:Authentication:DefaultSignInScheme"] = "Cookies",
            ["Template:Authentication:Cookie:Enabled"] = "true",
            ["Template:Authentication:Cookie:Scheme"] = "Cookies",
            ["Template:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["Template:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["Template:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["Template:Authentication:Providers:Microsoft:ClientId"] = "test-microsoft-client-id",
            ["Template:Authentication:Providers:Microsoft:ClientSecret"] = "test-microsoft-client-secret",
            ["Template:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft",
            ["Template:Authentication:Providers:Microsoft:Scopes:0"] = "User.Read"
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
