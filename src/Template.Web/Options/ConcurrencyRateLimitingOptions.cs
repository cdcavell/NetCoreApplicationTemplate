namespace Template.Web.Options;

/// <summary>
/// Represents concurrency rate limiting configuration.
/// </summary>
public sealed class ConcurrencyRateLimitingOptions
{
    /// <summary>
    /// The maximum number of concurrent permits allowed.
    /// </summary>
    public int PermitLimit { get; set; } = 10;

    /// <summary>
    /// The maximum number of requests allowed to wait in the queue.
    /// </summary>
    public int QueueLimit { get; set; }
}
