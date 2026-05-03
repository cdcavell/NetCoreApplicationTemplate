namespace Template.Web.Options;

/// <summary>
/// Represents fixed-window rate limiting configuration.
/// </summary>
public sealed class FixedWindowRateLimitingOptions
{
    /// <summary>
    /// Maximum number of permits allowed per window.
    /// </summary>
    public int PermitLimit { get; set; } = 60;

    /// <summary>
    /// Length of the fixed window in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of requests allowed to queue when the permit limit is reached.
    /// </summary>
    public int QueueLimit { get; set; }
}
