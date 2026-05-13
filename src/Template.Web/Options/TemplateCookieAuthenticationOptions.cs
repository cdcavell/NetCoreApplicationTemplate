using Microsoft.AspNetCore.Authentication.Cookies;

namespace Template.Web.Options;

/// <summary>
/// Represents cookie authentication configuration.
/// </summary>
public sealed class TemplateCookieAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the cookie authentication scheme name.
    /// </summary>
    public string Scheme { get; set; } = CookieAuthenticationDefaults.AuthenticationScheme;

    /// <summary>
    /// Gets or sets the authentication cookie name.
    /// </summary>
    public string CookieName { get; set; } = ".Template.Web.Auth";

    /// <summary>
    /// Gets or sets the login path.
    /// </summary>
    public string LoginPath { get; set; } = "/Account/Login";

    /// <summary>
    /// Gets or sets the logout path.
    /// </summary>
    public string LogoutPath { get; set; } = "/Account/Logout";

    /// <summary>
    /// Gets or sets the access denied path.
    /// </summary>
    public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";

    /// <summary>
    /// Gets or sets the authentication ticket expiration in minutes.
    /// </summary>
    public int ExpireTimeSpanMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether sliding expiration is enabled.
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;
}
