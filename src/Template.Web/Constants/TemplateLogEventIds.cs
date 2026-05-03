namespace Template.Web.Constants;

/// <summary>
/// Defines application log event identifiers.
/// </summary>
internal static class TemplateLogEventIds
{
    internal const int UnhandledExceptionRoutedToErrorPage = 6000;
    internal const int StatusCodePageRoutedToErrorPage = 6001;

    internal const int RateLimitRejectedRequest = 6100;
}
