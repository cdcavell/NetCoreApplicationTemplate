using Template.Web.Middleware;
using Template.Web.Options;

namespace Template.Web.Extensions
{
    public static class SecurityHeadersExtensions
    {
        public static IServiceCollection AddTemplateSecurityHeaders(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SecurityHeadersOptions>(
                configuration.GetSection("SecurityHeaders"));

            return services;
        }

        public static IApplicationBuilder UseTemplateSecurityHeaders(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
