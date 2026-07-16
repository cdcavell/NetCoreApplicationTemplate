using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Web.Controllers;
using ProjectTemplate.Web.Tests.Extensions;
using ProjectTemplate.Web.Tests.Infrastructure;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Verifies the generated application's explicit anonymous endpoint contract.
/// </summary>
public sealed class AnonymousEndpointContractTests
{
    private static readonly string[] _expectedAnonymousRoutes =
    [
        "/Account/AccessDenied",
        "/Account/Login",
        "/External/Challenge",
        "/health",
        "/health/live",
        "/health/ready",
        "Home/Error/{statusCode:int?}"
    ];

    /// <summary>
    /// Verifies that only the reviewed routed endpoint allowlist carries anonymous metadata.
    /// </summary>
    [Fact]
    public async Task RoutedEndpoints_ExposeOnlyReviewedAnonymousAllowlist()
    {
        using ApplicationWebApplicationFactory factory = CreateClosedByDefaultFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage _ = await client.GetAsync(
            "/health/live",
            TestContext.Current.CancellationToken);

        EndpointDataSource endpointDataSource = factory.Services
            .GetRequiredService<EndpointDataSource>();

        string[] anonymousRoutes = [.. endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Where(IsGeneratedApplicationEndpoint)
            .Select(endpoint => endpoint.RoutePattern.RawText ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

        Assert.Equal(_expectedAnonymousRoutes, anonymousRoutes);
    }

    /// <summary>
    /// Verifies representative application routes remain protected by the fallback policy.
    /// </summary>
    /// <param name="path">The routed endpoint path to request anonymously.</param>
    /// <param name="expectedStatusCode">The expected unauthenticated response status.</param>
    [Theory]
    [InlineData("/", HttpStatusCode.Found)]
    [InlineData("/api/v1/application-information", HttpStatusCode.Unauthorized)]
    [InlineData("/api/application-information", HttpStatusCode.Unauthorized)]
    public async Task NonAllowlistedEndpoint_ChallengesAnonymousCaller(
        string path,
        HttpStatusCode expectedStatusCode)
    {
        using ApplicationWebApplicationFactory factory = CreateClosedByDefaultFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            path,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatusCode, response.StatusCode);

        if (expectedStatusCode == HttpStatusCode.Found)
        {
            Assert.NotNull(response.Headers.Location);
            Assert.Equal("/Account/Login", response.Headers.Location.LocalPath);
        }
    }

    /// <summary>
    /// Verifies logout declares authenticated access in addition to anti-forgery validation.
    /// </summary>
    [Fact]
    public void LogoutEndpoint_RequiresAuthorizationAndAntiforgery()
    {
        MethodInfo logoutMethod = typeof(AccountController).GetMethod(
            nameof(AccountController.Logout),
            BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException("AccountController.Logout was not found.");

        Assert.NotNull(logoutMethod.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(logoutMethod.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        Assert.Null(logoutMethod.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    private static bool IsGeneratedApplicationEndpoint(RouteEndpoint endpoint)
    {
        string? route = endpoint.RoutePattern.RawText;

        if (route is "/health" or "/health/live" or "/health/ready")
        {
            return true;
        }

        ControllerActionDescriptor? descriptor = endpoint.Metadata
            .GetMetadata<ControllerActionDescriptor>();

        return descriptor?.ControllerTypeInfo.Assembly == typeof(AccountController).Assembly;
    }

    private static ApplicationWebApplicationFactory CreateClosedByDefaultFactory()
    {
        return new ApplicationWebApplicationFactory(new Dictionary<string, string?>
        {
            ["ProjectTemplate:Authentication:Enabled"] = "true",
            ["ProjectTemplate:Authentication:Cookie:Enabled"] = "true",
            ["ProjectTemplate:Authorization:RequireAuthenticatedUserByDefault"] = "true"
        });
    }
}
