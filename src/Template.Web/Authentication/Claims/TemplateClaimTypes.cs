namespace Template.Web.Authentication.Claims;

/// <summary>
/// Defines normalized claim types used by the template application.
/// </summary>
public static class TemplateClaimTypes
{
    /// <summary>
    /// Represents the key used to identify the subject template in configuration or metadata collections.
    /// </summary>
    public const string Subject = "template:subject";
    /// <summary>
    /// Represents the XML element name used for the template.
    /// </summary>
    public const string Name = "template:name";
    /// <summary>
    /// Represents the template identifier for email content.
    /// </summary>
    public const string Email = "template:email";
    /// <summary>
    /// Specifies the claim type for a template role.
    /// </summary>
    public const string Role = "template:role";
    /// <summary>
    /// Represents the group identifier used in template metadata.
    /// </summary>
    public const string Group = "template:group";
    /// <summary>
    /// Represents the permission identifier used for template-related authorization checks.
    /// </summary>
    public const string Permission = "template:permission";
}
