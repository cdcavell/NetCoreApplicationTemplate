namespace ProjectTemplate.Web.Authentication.Claims;

/// <summary>
/// Defines normalized claim types used by the application.
/// </summary>
public static class ApplicationClaimTypes
{
    /// <summary>
    /// Represents the key used to identify the subject in configuration or metadata collections.
    /// </summary>
    public const string Subject = "application:subject";
    /// <summary>
    /// Represents the XML name used for the application.
    /// </summary>
    public const string Name = "application:name";
    /// <summary>
    /// Represents the identifier for email content.
    /// </summary>
    public const string Email = "application:email";
    /// <summary>
    /// Specifies the claim type for an application role.
    /// </summary>
    public const string Role = "application:role";
    /// <summary>
    /// Represents the group identifier used in application metadata.
    /// </summary>
    public const string Group = "application:group";
    /// <summary>
    /// Represents the permission identifier used for application-related authorization checks.
    /// </summary>
    public const string Permission = "application:permission";
}
