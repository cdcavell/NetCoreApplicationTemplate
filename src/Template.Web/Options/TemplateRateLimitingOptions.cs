namespace Template.Web.Options;

/// <summary>
/// Represents template-level rate limiting configuration.
/// </summary>
public sealed class TemplateRateLimitingOptions
{
    /// <summary>
    /// Configuration section name for rate limiting settings.
    /// </summary>
    public const string SectionName = "Template:RateLimiting";

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use the global limiter.
    /// When true, <see cref="GlobalFixedWindow"/> is used as a global limiter.
    /// </summary>
    public bool UseGlobalLimiter { get; set; } = true;

    /// <summary>
    /// Gets or sets the global fixed-window rate limiting options.
    /// </summary>
    public FixedWindowRateLimitingOptions GlobalFixedWindow { get; set; } = new()
    {
        PermitLimit = 60,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    /// <summary>
    /// Gets or sets the fixed-window rate limiting options applied at the template level.
    /// </summary>
    public FixedWindowRateLimitingOptions FixedWindowPolicy { get; set; } = new()
    {
        PermitLimit = 60,
        WindowSeconds = 60,
        QueueLimit = 0
    };

    /// <summary>
    /// Gets or sets the concurrency-based rate limiting options applied at the template level.
    /// </summary>
    public ConcurrencyRateLimitingOptions ConcurrencyPolicy { get; set; } = new()
    {
        PermitLimit = 10,
        QueueLimit = 0
    };
}
