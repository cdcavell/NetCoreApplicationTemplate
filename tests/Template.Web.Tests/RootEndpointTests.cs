using System.Net;
using Template.Web.Tests.Extensions;
using Template.Web.Tests.Infrastructure;

namespace Template.Web.Tests;

public sealed class RootEndpointTests
{
    [Fact]
    public async Task RootEndpoint_ReturnsHelloWorld()
    {
        using TemplateWebApplicationFactory factory = new(new Dictionary<string, string?>());
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Hello World!", body);
    }
}
