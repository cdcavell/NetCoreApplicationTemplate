namespace ProjectTemplate.Web.Authentication.Claims;

/// <summary>
/// Represents source claim mappings used to normalize provider-specific claims.
/// </summary>
public sealed class ApplicationClaimMappingOptions
{
    /// <summary>
    /// Gets or sets the collection of claim type identifiers used to represent the subject in security tokens.
    /// </summary>
    /// <remarks>This collection typically includes standard claim types such as 'sub', 'subject', 'nameid',
    /// and the value of ClaimTypes.NameIdentifier. The identifiers are used to extract or assign the subject value when
    /// processing security tokens.</remarks>
    public ICollection<string> Subject { get; set; } =
    [
        "sub",
        "subject",
        "nameid",
        System.Security.Claims.ClaimTypes.NameIdentifier
    ];

    /// <summary>
    /// Gets or sets the collection of claim type names used to identify a user's name.
    /// </summary>
    /// <remarks>This collection typically includes standard claim type identifiers such as "name",
    /// "display_name", and values from <see cref="System.Security.Claims.ClaimTypes"/>. The collection can be
    /// customized to support additional or alternative claim type names as needed.</remarks>
    public ICollection<string> Name { get; set; } =
    [
        "name",
        "display_name",
        System.Security.Claims.ClaimTypes.Name
    ];

    /// <summary>
    /// Gets or sets the collection of claim type identifiers used to represent an email address.
    /// </summary>
    /// <remarks>This collection typically includes standard claim type names such as "email", "emailaddress",
    /// and the value of <see cref="System.Security.Claims.ClaimTypes.Email"/>. Modify this collection to support custom
    /// or additional claim type identifiers as needed.</remarks>
    public ICollection<string> Email { get; set; } =
    [
        "email",
        "emailaddress",
        System.Security.Claims.ClaimTypes.Email
    ];

    /// <summary>
    /// Gets or sets the collection of claim type names that represent user roles.
    /// </summary>
    /// <remarks>This collection typically includes standard claim type names such as "role", "roles", and the
    /// value of <see cref="System.Security.Claims.ClaimTypes.Role"/>. Modify this collection to support custom or
    /// additional role claim types as needed.</remarks>
    public ICollection<string> Role { get; set; } =
    [
        "role",
        "roles",
        System.Security.Claims.ClaimTypes.Role
    ];

    /// <summary>
    /// Gets or sets the collection of group identifiers associated with the entity.
    /// </summary>
    public ICollection<string> Group { get; set; } =
    [
        "group",
        "groups",
        "memberOf"
    ];

    /// <summary>
    /// Gets or sets the collection of permission identifiers associated with the current context.
    /// </summary>
    public ICollection<string> Permission { get; set; } =
    [
        "permission",
        "permissions",
        "scope",
        "scp"
    ];
}
