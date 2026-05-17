using Microsoft.AspNetCore.Http;

namespace Template.Infrastructure.Data;

public interface ICurrentActorAccessor
{
    string CurrentActor { get; }
}

public sealed class HttpContextCurrentActorAccessor(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentActorAccessor
{
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
