using Asp.Versioning;
using ProjectTemplate.Web.Options;

namespace ProjectTemplate.Web.Extensions;

/// <summary>
/// Provides extension methods to register API versioning services for the application.
/// </summary>
public static class ApiVersioningServiceExtensions
{
    /// <summary>
    /// Adds API versioning services and conventions for controller-based APIs.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same service collection instance so calls can be chained.</returns>
    public static IServiceCollection AddApplicationApiVersioning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ApplicationApiVersioningOptions apiVersioningOptions = new();

        configuration
            .GetSection(ApplicationApiVersioningOptions.SectionName)
            .Bind(apiVersioningOptions);

        ValidateApiVersioningOptions(apiVersioningOptions);

        services
            .AddOptions<ApplicationApiVersioningOptions>()
            .Bind(configuration.GetSection(ApplicationApiVersioningOptions.SectionName))
            .Validate(options => options.DefaultMajorVersion > 0,
                "ProjectTemplate:ApiVersioning:DefaultMajorVersion must be greater than zero.")
            .Validate(options => options.DefaultMinorVersion >= 0,
                "ProjectTemplate:ApiVersioning:DefaultMinorVersion must be zero or greater.")
            .Validate(options => options.EnableUrlSegmentVersioning || options.EnableHeaderVersioning,
                "ProjectTemplate:ApiVersioning must enable URL segment versioning, header versioning, or both.")
            .Validate(options => !options.EnableHeaderVersioning || !string.IsNullOrWhiteSpace(options.HeaderName),
                "ProjectTemplate:ApiVersioning:HeaderName is required when header versioning is enabled.")
            .ValidateOnStart();

        IApiVersionReader[] readers = CreateApiVersionReaders(apiVersioningOptions);

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(
                    apiVersioningOptions.DefaultMajorVersion,
                    apiVersioningOptions.DefaultMinorVersion);

                options.AssumeDefaultVersionWhenUnspecified =
                    apiVersioningOptions.AssumeDefaultVersionWhenUnspecified;

                options.ReportApiVersions = apiVersioningOptions.ReportApiVersions;
                options.ApiVersionReader = ApiVersionReader.Combine(readers);
            })
            .AddMvc();

        return services;
    }

    private static IApiVersionReader[] CreateApiVersionReaders(
        ApplicationApiVersioningOptions options)
    {
        List<IApiVersionReader> readers = [];

        if (options.EnableUrlSegmentVersioning)
        {
            readers.Add(new UrlSegmentApiVersionReader());
        }

        if (options.EnableHeaderVersioning)
        {
            readers.Add(new HeaderApiVersionReader(options.HeaderName));
        }

        return [.. readers];
    }

    private static void ValidateApiVersioningOptions(ApplicationApiVersioningOptions options)
    {
        if (options.DefaultMajorVersion <= 0)
        {
            throw new InvalidOperationException(
                "ProjectTemplate:ApiVersioning:DefaultMajorVersion must be greater than zero.");
        }

        if (options.DefaultMinorVersion < 0)
        {
            throw new InvalidOperationException(
                "ProjectTemplate:ApiVersioning:DefaultMinorVersion must be zero or greater.");
        }

        if (!options.EnableUrlSegmentVersioning && !options.EnableHeaderVersioning)
        {
            throw new InvalidOperationException(
                "ProjectTemplate:ApiVersioning must enable URL segment versioning, header versioning, or both.");
        }

        if (options.EnableHeaderVersioning && string.IsNullOrWhiteSpace(options.HeaderName))
        {
            throw new InvalidOperationException(
                "ProjectTemplate:ApiVersioning:HeaderName is required when header versioning is enabled.");
        }
    }
}
