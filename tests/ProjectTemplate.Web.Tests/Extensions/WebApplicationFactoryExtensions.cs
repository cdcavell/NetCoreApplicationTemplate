using Microsoft.AspNetCore.Mvc.Testing;

namespace ProjectTemplate.Web.Tests.Extensions;

/// <summary>
/// Provides extension methods for the WebApplicationFactory class to facilitate testing of web applications. 
/// </summary>
internal static class WebApplicationFactoryExtensions
{
    /// <summary>
    /// Creates an HttpClient with a base address of https://localhost and disables automatic redirection.
    /// This is useful for testing scenarios where you want to ensure that the application is correctly
    /// handling HTTPS requests and not automatically redirecting to HTTP.
    /// </summary>
    /// <param name="factory">
    /// The WebApplicationFactory instance used to create the HttpClient. This factory is typically configured
    /// </param>
    /// <returns>
    /// An <see cref="HttpClient"/> instance configured to use HTTPS and not follow redirects, allowing for more accurate
    /// testing of secure endpoints and redirection behavior.
    /// </returns>
    public static HttpClient CreateHttpsClient(this WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }
}
