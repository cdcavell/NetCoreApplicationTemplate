namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to configure the application's middleware pipeline
/// with a predefined ordering suitable for this template.
/// </summary>
    public static class PipelineExtensions
    {
    /// <summary>
    /// Configures the middleware pipeline for the specified <see cref="WebApplication"/>.
    /// The ordering includes forwarded headers, request logging, exception handling,
    /// security headers, HTTPS redirection, static files, routing, rate limiting, and
    /// (optionally) authentication/authorization and endpoint mapping.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
        public static WebApplication UseTemplatePipeline(this WebApplication app)
        {
            // 1. Proxy/load balancer correction must happen early.
            app.UseForwardedHeaders();

            // 2. Structured request logging should see corrected scheme, host, and client IP.
            app.UseTemplateRequestLogging();

            // 3. Centralized exception handling.
            app.UseExceptionHandler("/error/500");
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            // 4. Optional security response headers.
            app.UseTemplateSecurityHeaders();

            // 5. HTTPS enforcement.
            app.UseHttpsRedirection();

            // 6. Static files before routing if using MVC/Razor UI.
            app.UseStaticFiles();

            // 7. Routing.
            app.UseRouting();

            // 8. CORS, when needed, should be after routing and before auth.
            //app.UseCors();

            // 9. Rate limiting after routing when endpoint-specific policies are used.
            app.UseRateLimiter();

            // 10. Authentication and authorization.
            //app.UseAuthentication();
            //app.UseAuthorization();

            // 11. Endpoint mapping.
            //app.MapControllers();
            //app.MapRazorPages();

            return app;
        }
    }
}
