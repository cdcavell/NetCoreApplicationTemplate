using System.Net;
using ProjectTemplate.Web.Tests.Extensions;
using ProjectTemplate.Web.Tests.Infrastructure;

namespace ProjectTemplate.Web.Tests;

public sealed class RootEndpointTests
{
    [Fact]
    public async Task RootEndpoint_ReturnsHelloWorld()
    {
        using ApplicationWebApplicationFactory factory = new(new Dictionary<string, string?>());
        using HttpClient client = factory.CreateHttpsClient();

        using HttpResponseMessage response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Hello World!", body);
    }
}
