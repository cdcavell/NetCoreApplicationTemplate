namespace ProjectTemplate.Web.Authentication.Options;

/// <summary>
/// Represents cookie authentication configuration.
/// </summary>
public sealed class ApplicationCookieAuthenticationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether cookie authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the cookie authentication scheme name.
    /// </summary>
    public string Scheme { get; set; } = "Cookies";

    /// <summary>
    /// Gets or sets the login path used when an unauthenticated request requires authentication.
    /// </summary>
    public string LoginPath { get; set; } = "/Account/Login";

    /// <summary>
    /// Gets or sets the logout path used by the application.
    /// </summary>
    public string LogoutPath { get; set; } = "/Account/Logout";

    /// <summary>
    /// Gets or sets the access denied path used when an authenticated user lacks permission.
    /// </summary>
    public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";

    /// <summary>
    /// Gets or sets the cookie expiration time in minutes.
    /// </summary>
    public int ExpireMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether the cookie expiration should be renewed during active use.
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether local Development environments may issue the authentication cookie for
    /// plain HTTP requests.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false" />. This override is rejected outside the Development environment.
    /// </remarks>
    public bool AllowInsecureHttp { get; set; }
}
