using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Template.Web.Authentication.Claims;
using Template.Web.Authentication.Options;
using Template.Web.Tests.Extensions;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

/// <summary>
/// Provides integration tests for template authorization policy configuration and role/permission policy behavior.
/// </summary>
public sealed class AuthorizationPolicyTests
{
    private const string _testAuthenticationScheme = "TestAuthentication";
    private const string _testUserHeaderName = "X-Test-User";
    private const string _testRoleHeaderName = "X-Test-Role";
    private const string _testPermissionHeaderName = "X-Test-Permission";

    /// <summary>
    /// Verifies that template authorization options are bound from configuration.
    /// </summary>
    [Fact]
    public void AuthorizationOptions_AreBoundFromConfiguration()
    {
        using TemplateWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["Template:Authorization:RoleClaimType"] = "custom:role",
            ["Template:Authorization:PermissionClaimType"] = "custom:permission",
            ["Template:Authorization:AdministratorRoles:0"] = "Administrator",
            ["Template:Authorization:AdministratorRoles:1"] = "SecurityAdministrator",
            ["Template:Authorization:ManageApplicationPermissions:0"] = "application.manage",
            ["Template:Authorization:ManageApplicationPermissions:1"] = "application.configure"
        });

        TemplateAuthorizationOptions options = factory.Services
            .GetRequiredService<IOptions<TemplateAuthorizationOptions>>()
            .Value;

        Assert.Equal("custom:role", options.RoleClaimType);
        Assert.Equal("custom:permission", options.PermissionClaimType);
        Assert.Contains("Administrator", options.AdministratorRoles);
        Assert.Contains("SecurityAdministrator", options.AdministratorRoles);
        Assert.Contains("application.manage", options.ManageApplicationPermissions);
        Assert.Contains("application.configure", options.ManageApplicationPermissions);

    }

    /// <summary>
    /// Verifies that the authenticated-user policy rejects unauthenticated requests.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WhenUnauthenticated()
    {
        using WebApplicationFactory<Program> factory = CreateFactoryWithTestAuthentication();
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/protected",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the administrator role policy allows users with the configured administrator role.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task RolePolicy_ReturnsOk_WhenUserHasAdministratorRole()
    {
        using WebApplicationFactory<Program> factory = CreateFactoryWithTestAuthentication();
        using HttpClient client = factory.CreateHttpsClient();

        client.DefaultRequestHeaders.Add(_testUserHeaderName, "test-user");
        client.DefaultRequestHeaders.Add(_testRoleHeaderName, "Administrator");

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/admin",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the administrator role policy rejects authenticated users without the required role.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task RolePolicy_ReturnsForbidden_WhenUserLacksAdministratorRole()
    {
        using WebApplicationFactory<Program> factory = CreateFactoryWithTestAuthentication();
        using HttpClient client = factory.CreateHttpsClient();

        client.DefaultRequestHeaders.Add(_testUserHeaderName, "test-user");
        client.DefaultRequestHeaders.Add(_testRoleHeaderName, "User");

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/admin",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the manage application permission policy allows users with the configured permission.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task PermissionPolicy_ReturnsOk_WhenUserHasManagePermission()
    {
        using WebApplicationFactory<Program> factory = CreateFactoryWithTestAuthentication();
        using HttpClient client = factory.CreateHttpsClient();

        client.DefaultRequestHeaders.Add(_testUserHeaderName, "test-user");
        client.DefaultRequestHeaders.Add(_testPermissionHeaderName, "application.manage");

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/manage",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the manage application permission policy rejects authenticated users without the required permission.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task PermissionPolicy_ReturnsForbidden_WhenUserLacksManagePermission()
    {
        using WebApplicationFactory<Program> factory = CreateFactoryWithTestAuthentication();
        using HttpClient client = factory.CreateHttpsClient();

        client.DefaultRequestHeaders.Add(_testUserHeaderName, "test-user");
        client.DefaultRequestHeaders.Add(_testPermissionHeaderName, "application.read");

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/manage",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the baseline authenticated-user policy remains available and allows authenticated users.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task AuthenticatedUserPolicy_RemainsAvailable()
    {
        using WebApplicationFactory<Program> factory = CreateFactoryWithTestAuthentication();
        using HttpClient client = factory.CreateHttpsClient();

        client.DefaultRequestHeaders.Add(_testUserHeaderName, "test-user");

        using HttpResponseMessage response = await client.GetAsync(
            "/test/authentication/protected",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    /// <summary>
    /// Creates a test application factory configured to use a test-only authentication scheme.
    /// </summary>
    /// <returns>A configured <see cref="WebApplicationFactory{TEntryPoint}"/> instance.</returns>
    private static WebApplicationFactory<Program> CreateFactoryWithTestAuthentication()
    {
        return new TemplateWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Template:Authentication:Enabled"] = "true",
            ["Template:Authentication:DefaultScheme"] = _testAuthenticationScheme,
            ["Template:Authentication:DefaultChallengeScheme"] = _testAuthenticationScheme,
            ["Template:Authentication:DefaultSignInScheme"] = _testAuthenticationScheme,
            ["Template:Authorization:RoleClaimType"] = TemplateClaimTypes.Role,
            ["Template:Authorization:PermissionClaimType"] = TemplateClaimTypes.Permission,
            ["Template:Authorization:AdministratorRoles:0"] = "Administrator",
            ["Template:Authorization:ManageApplicationPermissions:0"] = "application.manage"
        })
        .WithWebHostBuilder(builder => builder.ConfigureServices(services => services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, TestAuthorizationAuthenticationHandler>(
                _testAuthenticationScheme,
                _ => { })));
    }

    /// <summary>
    /// Provides a test-only authentication handler that creates claims from request headers.
    /// </summary>
    private sealed class TestAuthorizationAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        /// <summary>
        /// Authenticates the current request by creating a test principal from configured request headers.
        /// </summary>
        /// <returns>An authentication result for the current test request.</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(_testUserHeaderName, out StringValues userValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string? userName = userValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Name, userName)
            ];

            foreach (string role in SplitHeaderValues(Request.Headers[_testRoleHeaderName]))
            {
                claims.Add(new Claim(TemplateClaimTypes.Role, role));
            }

            foreach (string permission in SplitHeaderValues(Request.Headers[_testPermissionHeaderName]))
            {
                claims.Add(new Claim(TemplateClaimTypes.Permission, permission));
            }

            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static IEnumerable<string> SplitHeaderValues(IEnumerable<string?> values)
        {
            return values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .SelectMany(value => value!.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
    }
}
