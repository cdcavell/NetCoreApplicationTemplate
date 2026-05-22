namespace ProjectTemplate.Web.Authentication.Options;

/// <summary>
/// Provides configurable authorization policy options for the application.
/// </summary>
public sealed class ApplicationAuthorizationOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind application authorization settings.
    /// </summary>
    public const string SectionName = "ProjectTemplate:Authorization";

    /// <summary>
    /// Gets or sets the claim type used to evaluate role-based authorization policies.
    /// </summary>
    public string RoleClaimType { get; set; } = "application:role";

    /// <summary>
    /// Gets or sets the claim type used to evaluate permission-based authorization policies.
    /// </summary>
    public string PermissionClaimType { get; set; } = "application:permission";

    /// <summary>
    /// Gets or sets the role values that satisfy the administrator authorization policy.
    /// </summary>
    public string[] AdministratorRoles { get; set; } = ["administrator"];

    /// <summary>
    /// Gets or sets the permission values that satisfy the manage application authorization policy.
    /// </summary>
    public string[] ManageApplicationPermissions { get; set; } = ["application.manage"];
}
