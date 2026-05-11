using Microsoft.Extensions.Options;
using Template.Web.Options;

namespace Template.Web.Authentication;

/// <summary>
/// Provides extension methods for applying template authentication middleware.
/// </summary>
public static class TemplateAuthenticationApplicationBuilderExtensions
{
    /// <summary>
    /// Applies authentication middleware when template authentication is enabled.
    /// Authorization middleware is always applied so authorization attributes work consistently.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder instance for chaining.</returns>
    public static IApplicationBuilder UseTemplateAuthentication(this IApplicationBuilder app)
    {
        TemplateAuthenticationOptions options = app.ApplicationServices
            .GetRequiredService<IOptions<TemplateAuthenticationOptions>>()
            .Value;

        if (options.Enabled)
        {
            app.UseAuthentication();
        }

        app.UseAuthorization();

        return app;
    }
}
