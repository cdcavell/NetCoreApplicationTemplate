using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Authentication.Extensions;
using ProjectTemplate.Web.Authentication.Options;

namespace ProjectTemplate.Web.Tests;

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
            ["ProjectTemplate:Authentication:Providers:Microsoft:Enabled"] = "false",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientId"] = "",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientSecret"] = ""
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
    /// Verifies that Microsoft provider scopes are bound from application authentication configuration.
    /// </summary>
    [Fact]
    public void MicrosoftProviderScopes_AreBoundFromConfiguration()
    {
        using ServiceProvider services = CreateServiceProvider(CreateEnabledMicrosoftConfiguration());

        ApplicationAuthenticationOptions options = services
            .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
            .Value;

        Assert.Equal(["User.Read"], options.Providers.Microsoft.Scopes);
    }

    private static Dictionary<string, string?> CreateEnabledMicrosoftConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:DefaultScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:DefaultChallengeScheme"] = "Microsoft",
            ["ProjectTemplate:Authentication:DefaultSignInScheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Scheme"] = "Cookies",
            ["ProjectTemplate:Authentication:Providers:Microsoft:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Providers:Microsoft:Scheme"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:DisplayName"] = "Microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientId"] = "test-microsoft-client-id",
            ["ProjectTemplate:Authentication:Providers:Microsoft:ClientSecret"] = "test-microsoft-client-secret",
            ["ProjectTemplate:Authentication:Providers:Microsoft:CallbackPath"] = "/signin-microsoft",
            ["ProjectTemplate:Authentication:Providers:Microsoft:Scopes:0"] = "User.Read"
        };
    }

    private static ServiceProvider CreateServiceProvider(IReadOnlyDictionary<string, string?> configurationValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        ServiceCollection services = [];
        services.AddLogging();
        services.AddApplicationAuthentication(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
