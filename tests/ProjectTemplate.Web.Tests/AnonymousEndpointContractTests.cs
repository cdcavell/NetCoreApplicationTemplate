using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
            .Order(StringComparer.Ordinal)];

        Assert.Equal(_expectedAnonymousRoutes, anonymousRoutes);
    }

    /// <summary>
    /// Verifies representative application routes remain protected by the fallback policy.
    /// </summary>
    /// <param name="path">The routed endpoint path to request anonymously.</param>
    [Theory]
    [InlineData("/")]
    [InlineData("/api/v1/application-information")]
    [InlineData("/api/application-information")]
    public async Task NonAllowlistedEndpoint_ChallengesAnonymousCaller(string path)
    {
        using ApplicationWebApplicationFactory factory = CreateClosedByDefaultFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            path,
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Found, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/Account/Login", response.Headers.Location.LocalPath);
    }

    /// <summary>
    /// Verifies logout declares authenticated access in addition to anti-forgery validation.
    /// </summary>
    [Fact]
    public async Task LogoutEndpoint_RequiresAuthorizationAndAntiforgery()
    {
        using ApplicationWebApplicationFactory factory = CreateClosedByDefaultFactory();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage _ = await client.GetAsync(
            "/health/live",
            TestContext.Current.CancellationToken);

        EndpointDataSource endpointDataSource = factory.Services
            .GetRequiredService<EndpointDataSource>();

        RouteEndpoint logoutEndpoint = Assert.Single(endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            , endpoint => string.Equals(
                endpoint.RoutePattern.RawText,
                "/Account/Logout",
                StringComparison.Ordinal));

        Assert.NotNull(logoutEndpoint.Metadata.GetMetadata<IAuthorizeData>());
        Assert.Null(logoutEndpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Contains(
            logoutEndpoint.Metadata,
            metadata => string.Equals(
                metadata.GetType().Name,
                "ValidateAntiForgeryTokenAttribute",
                StringComparison.Ordinal));
    }

    private static bool IsGeneratedApplicationEndpoint(RouteEndpoint endpoint)
    {
        string? route = endpoint.RoutePattern.RawText;

        return route is "/health" or "/health/live" or "/health/ready" || endpoint.Metadata
            .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
            .Any(descriptor => descriptor.ControllerTypeInfo.Namespace?
                .StartsWith("ProjectTemplate.Web.Controllers", StringComparison.Ordinal) == true);
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
