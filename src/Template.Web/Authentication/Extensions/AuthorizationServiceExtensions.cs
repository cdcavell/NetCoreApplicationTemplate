using Template.Web.Authentication.Claims;
using Template.Web.Authentication.Options;

namespace Template.Web.Authentication.Extensions;

/// <summary>
/// Provides extension methods for registering template authorization services.
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Adds template authorization services and baseline policies.
    /// </summary>
    /// <param name="services">The service collection to add authorization services to.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddTemplateAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        TemplateAuthorizationOptions options = configuration
            .GetSection(TemplateAuthorizationOptions.SectionName)
            .Get<TemplateAuthorizationOptions>() ?? new TemplateAuthorizationOptions();

        string roleClaimType = string.IsNullOrWhiteSpace(options.RoleClaimType)
            ? TemplateClaimTypes.Role
            : options.RoleClaimType;

        string permissionClaimType = string.IsNullOrWhiteSpace(options.PermissionClaimType)
            ? TemplateClaimTypes.Permission
            : options.PermissionClaimType;

        services
            .AddOptions<TemplateAuthorizationOptions>()
            .Bind(configuration.GetSection(TemplateAuthorizationOptions.SectionName));

        services.AddAuthorizationBuilder()
            .AddPolicy(
                TemplateAuthorizationPolicyNames.AuthenticatedUser,
                policy => policy.RequireAuthenticatedUser())
            .AddPolicy(
                TemplateAuthorizationPolicyNames.AdministratorRole,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(roleClaimType, options.AdministratorRoles);
                })
            .AddPolicy(
                TemplateAuthorizationPolicyNames.ManageApplicationPermission,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(permissionClaimType, options.ManageApplicationPermissions);
                });

        return services;
    }
}

/// <summary>
/// Provides template authorization policy names.
/// </summary>
public static class TemplateAuthorizationPolicyNames
{
    /// <summary>
    /// Policy requiring the current user to be authenticated.
    /// </summary>
    public const string AuthenticatedUser = "Template.AuthenticatedUser";
    /// <summary>
    /// Policy defining the administrator role.
    /// </summary>
    public const string AdministratorRole = "Template.Role.Administrator";
    /// <summary>
    /// Policy defining the manage application permission.
    /// </summary>
    public const string ManageApplicationPermission = "Template.Permission.ManageApplication";
}
