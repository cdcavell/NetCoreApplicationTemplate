using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Authentication.Options;
using ProjectTemplate.Web.Tests.Extensions;
using ProjectTemplate.Web.Tests.Infrastructure;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides integration coverage for the closed-by-default fallback authorization posture.
/// </summary>
public sealed class FallbackAuthorizationPolicyTests
{
    /// <summary>
    /// Verifies that the configured authorization option is bound and the authenticated fallback policy is registered.
    /// </summary>
    [Fact]
    public void FallbackPolicy_IsRegistered_WhenRequiredByConfiguration()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(
            authenticationEnabled: true,
            requireAuthenticatedUserByDefault: true);

        ApplicationAuthorizationOptions applicationOptions = factory.Services
            .GetRequiredService<IOptions<ApplicationAuthorizationOptions>>()
            .Value;

        AuthorizationOptions authorizationOptions = factory.Services
            .GetRequiredService<IOptions<AuthorizationOptions>>()
            .Value;

        Assert.True(applicationOptions.RequireAuthenticatedUserByDefault);
        Assert.NotNull(authorizationOptions.FallbackPolicy);
        Assert.Contains(
            authorizationOptions.FallbackPolicy.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    /// <summary>
    /// Verifies that an unannotated controller action rejects an unauthenticated request when fallback authorization is enabled.
    /// </summary>
    [Fact]
    public async Task UnannotatedControllerEndpoint_ReturnsUnauthorized_WhenFallbackPolicyIsEnabled()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(
            authenticationEnabled: true,
            requireAuthenticatedUserByDefault: true);
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/fallback",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that explicit anonymous metadata bypasses fallback authorization only for that endpoint.
    /// </summary>
    [Fact]
    public async Task ExplicitAnonymousEndpoint_ReturnsOk_WhenFallbackPolicyIsEnabled()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(
            authenticationEnabled: true,
            requireAuthenticatedUserByDefault: true);
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/anonymous",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that an unannotated Razor Page is protected by the fallback policy.
    /// </summary>
    [Fact]
    public async Task UnannotatedRazorPage_ChallengesAnonymousCaller_WhenFallbackPolicyIsEnabled()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(
            authenticationEnabled: true,
            requireAuthenticatedUserByDefault: true);
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/Account/Login", response.Headers.Location.LocalPath);
    }

    /// <summary>
    /// Verifies that applications can deliberately opt out of fallback authorization.
    /// </summary>
    [Fact]
    public async Task UnannotatedControllerEndpoint_ReturnsOk_WhenFallbackPolicyIsDisabled()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(
            authenticationEnabled: true,
            requireAuthenticatedUserByDefault: false);
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/fallback",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that authentication-disabled applications cannot accidentally retain an authenticated fallback policy.
    /// </summary>
    [Fact]
    public void ApplicationStartup_Fails_WhenAuthenticationIsDisabledAndFallbackRequiresAuthentication()
    {
        using ApplicationWebApplicationFactory factory = CreateFactory(
            authenticationEnabled: false,
            requireAuthenticatedUserByDefault: true);

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
            factory.Services
                .GetRequiredService<IOptions<ApplicationAuthorizationOptions>>()
                .Value);

        Assert.Contains(
            "ProjectTemplate:Authorization:RequireAuthenticatedUserByDefault cannot be true when ProjectTemplate:Authentication:Enabled is false.",
            exception.Message,
            StringComparison.Ordinal);
    }

    private static ApplicationWebApplicationFactory CreateFactory(
        bool authenticationEnabled,
        bool requireAuthenticatedUserByDefault)
    {
        return new ApplicationWebApplicationFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = authenticationEnabled.ToString(),
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = authenticationEnabled.ToString(),
            ["ProjectTemplate:Authorization:RequireAuthenticatedUserByDefault"] = requireAuthenticatedUserByDefault.ToString()
        });
    }
}
