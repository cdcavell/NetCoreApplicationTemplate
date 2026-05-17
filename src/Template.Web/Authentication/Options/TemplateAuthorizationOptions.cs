namespace Template.Web.Authentication.Options;

/// <summary>
/// Provides configurable authorization policy options for the template.
/// </summary>
public sealed class TemplateAuthorizationOptions
{
    /// <summary>
    /// Gets the configuration section name used to bind template authorization settings.
    /// </summary>
    public const string SectionName = "Template:Authorization";

    /// <summary>
    /// Gets or sets the claim type used to evaluate role-based authorization policies.
    /// </summary>
    public string RoleClaimType { get; set; } = "template:role";

    /// <summary>
    /// Gets or sets the claim type used to evaluate permission-based authorization policies.
    /// </summary>
    public string PermissionClaimType { get; set; } = "template:permission";

    /// <summary>
    /// Gets or sets the role values that satisfy the administrator authorization policy.
    /// </summary>
    public string[] AdministratorRoles { get; set; } = ["Administrator"];

    /// <summary>
    /// Gets or sets the permission values that satisfy the manage application authorization policy.
    /// </summary>
    public string[] ManageApplicationPermissions { get; set; } = ["application.manage"];
}
