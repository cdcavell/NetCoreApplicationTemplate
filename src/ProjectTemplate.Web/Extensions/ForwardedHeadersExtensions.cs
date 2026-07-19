using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Diagnostics;
using ProjectTemplate.Web.Options;
using NetIPNetwork = System.Net.IPNetwork;

namespace ProjectTemplate.Web.Extensions;

/// <summary>
/// Provides service and middleware registration for configurable forwarded headers support.
/// </summary>
public static class ForwardedHeadersExtensions
{
    /// <summary>
    /// Registers forwarded headers configuration from appsettings.json.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplicationForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ApplicationForwardedHeadersOptions>()
            .Bind(configuration.GetSection(ApplicationForwardedHeadersOptions.SectionName))
            .Validate(
                options => options.ForwardLimit is null or > 0,
                "ProjectTemplate:ForwardedHeaders:ForwardLimit must be null or greater than zero.")
            .Validate(
                options => options.Headers.All(IsValidForwardedHeader),
                "ProjectTemplate:ForwardedHeaders:Headers contains an invalid forwarded header value.")
            .Validate(
                options => options.KnownProxies.All(proxy => IPAddress.TryParse(proxy, out _)),
                "ProjectTemplate:ForwardedHeaders:KnownProxies must contain valid IP addresses.")
            .Validate(
                options => options.KnownNetworks.All(IsValidKnownNetwork),
                "ProjectTemplate:ForwardedHeaders:KnownNetworks must contain valid CIDR ranges such as 10.0.0.0/24.")
            .Validate(
                options =>
                    !options.Headers.Any(IsForwardedHost) ||
                    options.AllowedHosts.Any(host => !string.IsNullOrWhiteSpace(host)),
                "ProjectTemplate:ForwardedHeaders:AllowedHosts must contain at least one host when XForwardedHost is enabled.")
            .ValidateOnStart();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            ApplicationForwardedHeadersOptions settings =
                configuration
                    .GetSection(ApplicationForwardedHeadersOptions.SectionName)
                    .Get<ApplicationForwardedHeadersOptions>()
                ?? new ApplicationForwardedHeadersOptions();

            options.ForwardedHeaders = settings.Enabled
                ? BuildForwardedHeaders(settings.Headers)
                : ForwardedHeaders.None;

            options.ForwardLimit = settings.ForwardLimit;
            options.RequireHeaderSymmetry = settings.RequireHeaderSymmetry;

            if (settings.ClearKnownNetworksAndProxies)
            {
                options.KnownProxies.Clear();
                options.KnownIPNetworks.Clear();
            }

            foreach (string proxy in settings.KnownProxies.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }

            foreach (string network in settings.KnownNetworks.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                options.KnownIPNetworks.Add(ParseIPNetwork(network));
            }

            if (settings.AllowedHosts.Length > 0)
            {
                options.AllowedHosts.Clear();

                foreach (string host in settings.AllowedHosts.Where(value => !string.IsNullOrWhiteSpace(value)))
                {
                    options.AllowedHosts.Add(host);
                }
            }
        });

        services.AddHostedService<ForwardedHeadersTrustDiagnosticsHostedService>();

        return services;
    }

    /// <summary>
    /// Adds forwarded headers middleware only when enabled by configuration.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static IApplicationBuilder UseApplicationForwardedHeaders(this IApplicationBuilder app)
    {
        ApplicationForwardedHeadersOptions settings =
            app.ApplicationServices
                .GetRequiredService<IOptions<ApplicationForwardedHeadersOptions>>()
                .Value;

        return settings.Enabled ? app.UseForwardedHeaders() : app;
    }

    private static ForwardedHeaders BuildForwardedHeaders(IEnumerable<string> headers)
    {
        ForwardedHeaders forwardedHeaders = ForwardedHeaders.None;

        foreach (string header in headers)
        {
            if (TryParseForwardedHeader(header, out ForwardedHeaders parsedHeader))
            {
                forwardedHeaders |= parsedHeader;
            }
        }

        return forwardedHeaders;
    }

    private static bool IsValidForwardedHeader(string value)
    {
        return TryParseForwardedHeader(value, out _);
    }

    private static bool TryParseForwardedHeader(string? value, out ForwardedHeaders forwardedHeader)
    {
        forwardedHeader = ForwardedHeaders.None;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalizedValue = value
            .Trim()
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal);

        return Enum.TryParse(normalizedValue, ignoreCase: true, out forwardedHeader);
    }

    private static bool IsValidKnownNetwork(string value)
    {
        try
        {
            _ = ParseIPNetwork(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static NetIPNetwork ParseIPNetwork(string value)
    {
        string[] parts = value.Split('/', StringSplitOptions.TrimEntries);

        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid CIDR notation: {value}");
        }

        var prefix = IPAddress.Parse(parts[0]);
        int prefixLength = int.Parse(parts[1], CultureInfo.InvariantCulture);

        return new NetIPNetwork(prefix, prefixLength);
    }

    private static bool IsForwardedHost(string value)
    {
        return TryParseForwardedHeader(value, out ForwardedHeaders forwardedHeader) &&
               forwardedHeader.HasFlag(ForwardedHeaders.XForwardedHost);
    }
}
