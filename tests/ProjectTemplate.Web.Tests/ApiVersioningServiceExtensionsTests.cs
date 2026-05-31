using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Web.Extensions;
using ProjectTemplate.Web.Options;

namespace ProjectTemplate.Web.Tests;

public sealed class ApiVersioningServiceExtensionsTests
{
    [Theory]
    [InlineData(
        0,
        0,
        true,
        false,
        "X-API-Version",
        "ProjectTemplate:ApiVersioning:DefaultMajorVersion must be greater than zero.")]
    [InlineData(
        1,
        -1,
        true,
        false,
        "X-API-Version",
        "ProjectTemplate:ApiVersioning:DefaultMinorVersion must be zero or greater.")]
    [InlineData(
        1,
        0,
        false,
        false,
        "X-API-Version",
        "ProjectTemplate:ApiVersioning must enable URL segment versioning, header versioning, or both.")]
    [InlineData(
        1,
        0,
        false,
        true,
        " ",
        "ProjectTemplate:ApiVersioning:HeaderName is required when header versioning is enabled.")]
    public void AddApplicationApiVersioning_InvalidConfiguration_ThrowsInvalidOperationException(
        int defaultMajorVersion,
        int defaultMinorVersion,
        bool enableUrlSegmentVersioning,
        bool enableHeaderVersioning,
        string headerName,
        string expectedMessage)
    {
        IConfiguration configuration = CreateConfiguration(
            defaultMajorVersion,
            defaultMinorVersion,
            enableUrlSegmentVersioning,
            enableHeaderVersioning,
            headerName);

        ServiceCollection services = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => services.AddApplicationApiVersioning(configuration));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(1, 0, true, false, "X-API-Version")]
    [InlineData(1, 0, false, true, "X-API-Version")]
    [InlineData(2, 1, true, true, "X-API-Version")]
    public void AddApplicationApiVersioning_ValidConfiguration_ReturnsSameServiceCollection(
        int defaultMajorVersion,
        int defaultMinorVersion,
        bool enableUrlSegmentVersioning,
        bool enableHeaderVersioning,
        string headerName)
    {
        IConfiguration configuration = CreateConfiguration(
            defaultMajorVersion,
            defaultMinorVersion,
            enableUrlSegmentVersioning,
            enableHeaderVersioning,
            headerName);

        ServiceCollection services = new();

        IServiceCollection result = services.AddApplicationApiVersioning(configuration);

        Assert.Same(services, result);
        Assert.NotEmpty(services);
    }

    private static IConfiguration CreateConfiguration(
        int defaultMajorVersion,
        int defaultMinorVersion,
        bool enableUrlSegmentVersioning,
        bool enableHeaderVersioning,
        string headerName)
    {
        Dictionary<string, string?> values = new()
        {
            [$"{ApplicationApiVersioningOptions.SectionName}:DefaultMajorVersion"] =
                defaultMajorVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),

            [$"{ApplicationApiVersioningOptions.SectionName}:DefaultMinorVersion"] =
                defaultMinorVersion.ToString(System.Globalization.CultureInfo.InvariantCulture),

            [$"{ApplicationApiVersioningOptions.SectionName}:EnableUrlSegmentVersioning"] =
                enableUrlSegmentVersioning.ToString(),

            [$"{ApplicationApiVersioningOptions.SectionName}:EnableHeaderVersioning"] =
                enableHeaderVersioning.ToString(),

            [$"{ApplicationApiVersioningOptions.SectionName}:HeaderName"] = headerName
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
