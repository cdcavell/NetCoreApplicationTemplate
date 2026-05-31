using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Authentication.Options;
using ProjectTemplate.Web.Authentication.Providers.GitHub;
using ProjectTemplate.Web.Authentication.Providers.Google;
using ProjectTemplate.Web.Authentication.Providers.OpenIdConnect;
using ProjectTemplate.Web.Authentication.Providers.Saml2;
using TemplateOpenIdConnectAuthenticationOptions = ProjectTemplate.Web.Authentication.Providers.OpenIdConnect.OpenIdConnectAuthenticationOptions;
using TemplateSaml2AuthenticationOptions = ProjectTemplate.Web.Authentication.Providers.Saml2.Saml2AuthenticationOptions;

namespace ProjectTemplate.Web.Tests;

public sealed class AuthenticationProviderOptionCoverageTests
{
    [Fact]
    public void ProviderExtensions_NullBuilder_ThrowsArgumentNullException()
    {
        Assert.Equal(
            "builder",
            Assert.Throws<ArgumentNullException>(() =>
                GoogleAuthenticationServiceExtensions.AddGoogleAuthentication(
                    null!,
                    new ApplicationExternalAuthenticationProviderOptions())).ParamName);

        Assert.Equal(
            "builder",
            Assert.Throws<ArgumentNullException>(() =>
                GitHubAuthenticationServiceExtensions.AddGitHubAuthentication(
                    null!,
                    new ApplicationExternalAuthenticationProviderOptions())).ParamName);

        Assert.Equal(
            "builder",
            Assert.Throws<ArgumentNullException>(() =>
                OpenIdConnectAuthenticationServiceExtensions.AddOpenIdConnectAuthentication(
                    null!,
                    new TemplateOpenIdConnectAuthenticationOptions())).ParamName);

        Assert.Equal(
            "builder",
            Assert.Throws<ArgumentNullException>(() =>
                Saml2AuthenticationServiceExtensions.AddSaml2Authentication(
                    null!,
                    new TemplateSaml2AuthenticationOptions())).ParamName);
    }

    [Fact]
    public void ProviderExtensions_NullOptions_ThrowsArgumentNullException()
    {
        ServiceCollection services = new();
        AuthenticationBuilder builder = services.AddAuthentication();

        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddGoogleAuthentication(null!)).ParamName);

        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddGitHubAuthentication(null!)).ParamName);

        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddOpenIdConnectAuthentication(null!)).ParamName);

        Assert.Equal(
            "options",
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddSaml2Authentication(null!)).ParamName);
    }

    [Fact]
    public async Task EnabledGoogleProvider_ConfiguresOAuthOptionsAndFiltersScopes()
    {
        ServiceCollection services = new();
        services.AddLogging();
        AuthenticationBuilder builder = services.AddAuthentication();

        AuthenticationBuilder result = builder.AddGoogleAuthentication(new ApplicationExternalAuthenticationProviderOptions
        {
            Enabled = true,
            Scheme = "GoogleCoverage",
            DisplayName = "Google Coverage",
            ClientId = "google-client-id",
            ClientSecret = "google-client-secret",
            CallbackPath = "/signin-google-coverage",
            Scopes =
            [
                "profile",
                " ",
                "calendar.readonly",
                "calendar.readonly"
            ]
        });

        Assert.Same(builder, result);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AuthenticationScheme scheme = await GetRequiredSchemeAsync(
            serviceProvider,
            "GoogleCoverage");

        Assert.Equal("GoogleCoverage", scheme.Name);
        Assert.Equal("Google Coverage", scheme.DisplayName);

        object options = GetNamedOptions(
            serviceProvider,
            "GoogleCoverage",
            "GoogleOptions");

        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, GetPropertyAsString(options, "SignInScheme"));
        Assert.Equal("google-client-id", GetPropertyAsString(options, "ClientId"));
        Assert.Equal("google-client-secret", GetPropertyAsString(options, "ClientSecret"));
        Assert.Equal("/signin-google-coverage", GetPropertyAsString(options, "CallbackPath"));

        string[] scopes = GetStringCollectionProperty(options, "Scope");

        Assert.Contains("profile", scopes);
        Assert.Contains("calendar.readonly", scopes);
        Assert.Single(scopes, scope => string.Equals(scope, "calendar.readonly", StringComparison.Ordinal));
        Assert.DoesNotContain(scopes, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public async Task EnabledGitHubProvider_ConfiguresOAuthOptionsAndFiltersScopes()
    {
        ServiceCollection services = new();
        services.AddLogging();
        AuthenticationBuilder builder = services.AddAuthentication();

        AuthenticationBuilder result = builder.AddGitHubAuthentication(new ApplicationExternalAuthenticationProviderOptions
        {
            Enabled = true,
            Scheme = "GitHubCoverage",
            DisplayName = "GitHub Coverage",
            ClientId = "github-client-id",
            ClientSecret = "github-client-secret",
            CallbackPath = "/signin-github-coverage",
            Scopes =
            [
                "read:user",
                string.Empty,
                " ",
                "user:email"
            ]
        });

        Assert.Same(builder, result);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AuthenticationScheme scheme = await GetRequiredSchemeAsync(
            serviceProvider,
            "GitHubCoverage");

        Assert.Equal("GitHubCoverage", scheme.Name);
        Assert.Equal("GitHub Coverage", scheme.DisplayName);

        object options = GetNamedOptions(
            serviceProvider,
            "GitHubCoverage",
            "GitHubAuthenticationOptions");

        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, GetPropertyAsString(options, "SignInScheme"));
        Assert.Equal("github-client-id", GetPropertyAsString(options, "ClientId"));
        Assert.Equal("github-client-secret", GetPropertyAsString(options, "ClientSecret"));
        Assert.Equal("/signin-github-coverage", GetPropertyAsString(options, "CallbackPath"));

        string[] scopes = GetStringCollectionProperty(options, "Scope");

        Assert.Contains("read:user", scopes);
        Assert.Contains("user:email", scopes);
        Assert.DoesNotContain(scopes, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public async Task EnabledOpenIdConnectProvider_ConfiguresOptionsAndFiltersScopes()
    {
        ServiceCollection services = new();
        services.AddLogging();
        AuthenticationBuilder builder = services.AddAuthentication();

        AuthenticationBuilder result = builder.AddOpenIdConnectAuthentication(new TemplateOpenIdConnectAuthenticationOptions
        {
            Enabled = true,
            Scheme = "OpenIdConnectCoverage",
            DisplayName = "OpenID Connect Coverage",
            Authority = "https://login.example.test",
            ClientId = "oidc-client-id",
            ClientSecret = "oidc-client-secret",
            CallbackPath = "/signin-oidc-coverage",
            ResponseType = "code",
            SaveTokens = true,
            Scopes =
            [
                "openid",
                " ",
                "profile",
                "api.read"
            ]
        });

        Assert.Same(builder, result);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AuthenticationScheme scheme = await GetRequiredSchemeAsync(
            serviceProvider,
            "OpenIdConnectCoverage");

        Assert.Equal("OpenIdConnectCoverage", scheme.Name);
        Assert.Equal("OpenID Connect Coverage", scheme.DisplayName);

        object options = GetNamedOptions(
            serviceProvider,
            "OpenIdConnectCoverage",
            "OpenIdConnectOptions");

        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, GetPropertyAsString(options, "SignInScheme"));
        Assert.Equal("https://login.example.test", GetPropertyAsString(options, "Authority"));
        Assert.Equal("oidc-client-id", GetPropertyAsString(options, "ClientId"));
        Assert.Equal("oidc-client-secret", GetPropertyAsString(options, "ClientSecret"));
        Assert.Equal("/signin-oidc-coverage", GetPropertyAsString(options, "CallbackPath"));
        Assert.Equal("code", GetPropertyAsString(options, "ResponseType"));
        Assert.True(GetPropertyValue<bool>(options, "SaveTokens"));

        string[] scopes = GetStringCollectionProperty(options, "Scope");

        Assert.Contains("openid", scopes);
        Assert.Contains("profile", scopes);
        Assert.Contains("api.read", scopes);
        Assert.DoesNotContain(scopes, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public async Task EnabledSaml2Provider_ConfiguresServiceProviderAndIdentityProvider()
    {
        ServiceCollection services = new();
        services.AddLogging();
        AuthenticationBuilder builder = services.AddAuthentication();

        AuthenticationBuilder result = builder.AddSaml2Authentication(new TemplateSaml2AuthenticationOptions
        {
            Enabled = true,
            Scheme = "Saml2Coverage",
            DisplayName = "SAML2 Coverage",
            EntityId = "https://sp.example.test/saml2",
            MetadataUrl = "https://idp.example.test/metadata",
            ModulePath = "/saml2/coverage",
            RequireSignedAssertions = true,
            ValidateCertificates = false,
            LoadMetadata = false
        });

        Assert.Same(builder, result);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AuthenticationScheme scheme = await GetRequiredSchemeAsync(
            serviceProvider,
            "Saml2Coverage");

        Assert.Equal("Saml2Coverage", scheme.Name);
        Assert.Equal("SAML2 Coverage", scheme.DisplayName);

        object options = GetNamedOptions(
            serviceProvider,
            "Saml2Coverage",
            "Saml2Options");

        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, GetPropertyAsString(options, "SignInScheme"));

        object serviceProviderOptions = GetRequiredPropertyValue(options, "SPOptions");

        Assert.Equal("https://sp.example.test/saml2", GetPropertyAsString(serviceProviderOptions, "EntityId"));
        Assert.Equal("/saml2/coverage", GetPropertyAsString(serviceProviderOptions, "ModulePath"));
        Assert.True(GetPropertyValue<bool>(serviceProviderOptions, "WantAssertionsSigned"));
        Assert.False(GetPropertyValue<bool>(serviceProviderOptions, "ValidateCertificates"));

        object identityProviders = GetRequiredPropertyValue(options, "IdentityProviders");
        object identityProvider = Assert.Single(GetEnumerableValues(identityProviders));

        if (identityProvider.GetType().Name.StartsWith("KeyValuePair", StringComparison.Ordinal))
        {
            identityProvider = GetRequiredPropertyValue(identityProvider, "Value");
        }

        Assert.Equal("https://idp.example.test/metadata", GetPropertyAsString(identityProvider, "MetadataLocation"));
        Assert.False(GetPropertyValue<bool>(identityProvider, "LoadMetadata"));
        Assert.False(GetPropertyValue<bool>(identityProvider, "AllowUnsolicitedAuthnResponse"));
    }

    private static async Task<AuthenticationScheme> GetRequiredSchemeAsync(
        IServiceProvider serviceProvider,
        string schemeName)
    {
        IAuthenticationSchemeProvider schemeProvider = serviceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync(schemeName);

        return Assert.IsType<AuthenticationScheme>(scheme);
    }

    private static object GetNamedOptions(
        IServiceProvider serviceProvider,
        string schemeName,
        string optionsTypeName)
    {
        Type optionsType = FindLoadedType(optionsTypeName);
        Type monitorType = typeof(IOptionsMonitor<>).MakeGenericType(optionsType);
        object monitor = serviceProvider.GetRequiredService(monitorType);
        MethodInfo? getMethod = monitorType.GetMethod(
            nameof(IOptionsMonitor<>.Get),
            [typeof(string)]);

        Assert.True(getMethod is not null);

        object? options = getMethod.Invoke(monitor, [schemeName]);

        Assert.NotNull(options);
        Assert.IsType(optionsType, options);

        return options;
    }

    private static Type FindLoadedType(string typeName)
    {
        List<Type> matches =
        [
            .. AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal))
        ];

        Assert.NotEmpty(matches);

        if (matches.Count == 1)
        {
            return matches[0];
        }

        var authenticationMatch = matches.SingleOrDefault(type =>
            type.FullName?.Contains("Authentication", StringComparison.Ordinal) == true);

        return authenticationMatch ?? matches[0];
    }

    private static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return [.. exception.Types.Where(type => type is not null).Cast<Type>()];
        }
    }

    private static string[] GetStringCollectionProperty(
        object instance,
        string propertyName)
    {
        object value = GetRequiredPropertyValue(instance, propertyName);
        IEnumerable<string> values = Assert.IsAssignableFrom<IEnumerable<string>>(value);

        return [.. values];
    }

    private static object[] GetEnumerableValues(object instance)
    {
        IEnumerable values = Assert.IsAssignableFrom<IEnumerable>(instance);

        return [.. values.Cast<object>()];
    }

    private static T GetPropertyValue<T>(object instance, string propertyName)
    {
        object value = GetRequiredPropertyValue(instance, propertyName);

        return Assert.IsType<T>(value);
    }

    private static string GetPropertyAsString(object instance, string propertyName)
    {
        object value = GetRequiredPropertyValue(instance, propertyName);

        return value.ToString() ?? string.Empty;
    }

    private static object GetRequiredPropertyValue(object instance, string propertyName)
    {
        PropertyInfo? property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        Assert.True(property is not null);

        object? value = property.GetValue(instance);

        Assert.NotNull(value);

        return value;
    }
}
