using Serilog;
using Serilog.Events;

namespace Template.Web.Extensions
{
    public static class PipelineExtensions
    {
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
