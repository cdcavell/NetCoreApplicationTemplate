using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Authentication.Extensions;
using ProjectTemplate.Web.Authentication.Options;

namespace ProjectTemplate.Web.Tests;

public sealed class AuthenticationCookieSecurePolicyTests
{
    [Fact]
    public void CookieSecurePolicy_DefaultsToAlways()
    {
        using ServiceProvider serviceProvider = CreateServiceProvider(
            Environments.Production,
            new Dictionary<string, string?>());

        CookieAuthenticationOptions options = serviceProvider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
    }

    [Fact]
    public void CookieSecurePolicy_DevelopmentOverride_UsesSameAsRequest()
    {
        using ServiceProvider serviceProvider = CreateServiceProvider(
            Environments.Development,
            new Dictionary<string, string?>
            {
                ["ProjectTemplate:Authentication:Cookie:AllowInsecureHttp"] = "true"
            });

        CookieAuthenticationOptions options = serviceProvider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
    }

    [Fact]
    public void CookieSecurePolicy_InsecureOverrideOutsideDevelopment_FailsValidation()
    {
        using ServiceProvider serviceProvider = CreateServiceProvider(
            Environments.Production,
            new Dictionary<string, string?>
            {
                ["ProjectTemplate:Authentication:Cookie:AllowInsecureHttp"] = "true"
            });

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            serviceProvider
                .GetRequiredService<IOptions<ApplicationAuthenticationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authentication:Cookie:AllowInsecureHttp may only be enabled in the Development environment",
            exception.Message,
            StringComparison.Ordinal);
    }

    private static ServiceProvider CreateServiceProvider(
        string environmentName,
        IReadOnlyDictionary<string, string?> configurationValues)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        TestHostEnvironment environment = new(environmentName);
        ServiceCollection services = new();
        services.AddLogging();
        services.AddApplicationAuthentication(configuration, environment);

        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "ProjectTemplate.Web.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
