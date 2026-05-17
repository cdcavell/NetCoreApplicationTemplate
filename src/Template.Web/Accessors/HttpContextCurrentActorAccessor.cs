using Template.Infrastructure.Data;

namespace Template.Web.Accessors;

/// <summary>
/// An implementation of <see cref="ICurrentActorAccessor"/> that retrieves the current actor information from the HTTP context.
/// </summary>
/// <param name="httpContextAccessor"></param>
public sealed class HttpContextCurrentActorAccessor(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentActorAccessor
{
    /// <summary>
    /// Accesses the current actor information from the HTTP context. It first attempts to retrieve the "sub" claim from the user's claims, and if that is not available, it falls back to using the remote IP address. If neither is available, it returns "Unknown".
    /// </summary>
    public string CurrentActor
    {
        get
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;

            string? subject = httpContext?.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrWhiteSpace(subject))
            {
                return $"Subject: {subject}";
            }

            string? remoteIpAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

            return !string.IsNullOrWhiteSpace(remoteIpAddress) ? $"Remote IP: {remoteIpAddress}" : "Unknown";
        }
    }
}
