using Microsoft.AspNetCore.Authorization;
using ProjectTemplate.Web.Authentication.Claims;
using ProjectTemplate.Web.Authentication.Options;

namespace ProjectTemplate.Web.Authentication.Extensions;

/// <summary>
/// Provides extension methods for registering application authorization services.
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Adds application authorization services and baseline policies.
    /// </summary>
    /// <param name="services">The service collection to add authorization services to.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddApplicationAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        ApplicationAuthorizationOptions options = configuration
            .GetSection(ApplicationAuthorizationOptions.SectionName)
            .Get<ApplicationAuthorizationOptions>() ?? new ApplicationAuthorizationOptions();

        bool authenticationEnabled = configuration.GetValue<bool>(
            $"{ApplicationAuthenticationOptions.SectionName}:Enabled");

        string roleClaimType = string.IsNullOrWhiteSpace(options.RoleClaimType)
            ? ApplicationClaimTypes.Role
            : options.RoleClaimType;

        string permissionClaimType = string.IsNullOrWhiteSpace(options.PermissionClaimType)
            ? ApplicationClaimTypes.Permission
            : options.PermissionClaimType;

        services
            .AddOptions<ApplicationAuthorizationOptions>()
            .Bind(configuration.GetSection(ApplicationAuthorizationOptions.SectionName))
            .Validate(
                options => !options.RequireAuthenticatedUserByDefault || authenticationEnabled,
                "ProjectTemplate:Authorization:RequireAuthenticatedUserByDefault cannot be true when ProjectTemplate:Authentication:Enabled is false.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.RoleClaimType),
                "ProjectTemplate:Authorization:RoleClaimType is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.PermissionClaimType),
                "ProjectTemplate:Authorization:PermissionClaimType is required.")
            .Validate(
                options => options.AdministratorRoles.Any(role => !string.IsNullOrWhiteSpace(role)),
                "ProjectTemplate:Authorization:AdministratorRoles must contain at least one non-empty value.")
            .Validate(
                options => options.ManageApplicationPermissions.Any(permission => !string.IsNullOrWhiteSpace(permission)),
                "ProjectTemplate:Authorization:ManageApplicationPermissions must contain at least one non-empty value.")
            .ValidateOnStart();

        AuthorizationBuilder authorizationBuilder = services.AddAuthorizationBuilder()
            .AddPolicy(
                ApplicationAuthorizationPolicyNames.AuthenticatedUser,
                policy => policy.RequireAuthenticatedUser())
            .AddPolicy(
                ApplicationAuthorizationPolicyNames.AdministratorRole,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(roleClaimType, options.AdministratorRoles);
                })
            .AddPolicy(
                ApplicationAuthorizationPolicyNames.ManageApplicationPermission,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(permissionClaimType, options.ManageApplicationPermissions);
                });

        if (options.RequireAuthenticatedUserByDefault)
        {
            authorizationBuilder.SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        }

        return services;
    }
}

/// <summary>
/// Provides application authorization policy names.
/// </summary>
public static class ApplicationAuthorizationPolicyNames
{
    /// <summary>
    /// Policy requiring the current user to be authenticated.
    /// </summary>
    public const string AuthenticatedUser = "application.AuthenticatedUser";
    /// <summary>
    /// Policy defining the administrator role.
    /// </summary>
    public const string AdministratorRole = "application.Role.Administrator";
    /// <summary>
    /// Policy defining the manage application permission.
    /// </summary>
    public const string ManageApplicationPermission = "application.Permission.ManageApplication";
}
