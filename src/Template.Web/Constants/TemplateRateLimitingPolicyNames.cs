namespace Template.Web.Constants;

/// <summary>
/// Provides centralized names for template rate limiting policies.
/// </summary>
public static class TemplateRateLimitingPolicyNames
{
    /// <summary>
    /// Name of the fixed rate limiting policy.
    /// </summary>
    public const string Fixed = "fixed";

    /// <summary>
    /// Name of the concurrency rate limiting policy.
    /// </summary>
    public const string Concurrency = "concurrency";
}
