using Microsoft.Extensions.Options;
using Template.Web.Options;

namespace Template.Web.Middleware
{
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersMiddleware(
            RequestDelegate next,
            IOptions<SecurityHeadersOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled || IsExcludedPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            context.Response.OnStarting(() =>
            {
                IHeaderDictionary headers = context.Response.Headers;

                AddHeaderIfMissing(headers, "X-Content-Type-Options", "nosniff");
                AddHeaderIfMissing(headers, "X-Frame-Options", "DENY");
                AddHeaderIfMissing(headers, "Referrer-Policy", "strict-origin-when-cross-origin");
                AddHeaderIfMissing(headers, "X-Permitted-Cross-Domain-Policies", "none");

                // Modern browser isolation / cross-origin protections.
                if (_options.EnableCrossOriginHeaders)
                {
                    AddHeaderIfMissing(headers, "Cross-Origin-Opener-Policy", "same-origin");
                    AddHeaderIfMissing(headers, "Cross-Origin-Resource-Policy", "same-origin");
                }

                if (_options.EnablePermissionsPolicy)
                {
                    AddHeaderIfMissing(headers, "Permissions-Policy", _options.PermissionsPolicy);
                }

                if (_options.EnableContentSecurityPolicy)
                {
                    AddHeaderIfMissing(headers, "Content-Security-Policy", _options.ContentSecurityPolicy);
                }

                // Do not add X-XSS-Protection. It is obsolete and can cause problems in some browsers.

                return Task.CompletedTask;
            });

            await _next(context);
        }

        private bool IsExcludedPath(PathString path)
        {
            return _options.ExcludedPathPrefixes.Any(prefix =>
                path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private static void AddHeaderIfMissing(
            IHeaderDictionary headers,
            string name,
            string value)
        {
            if (!headers.ContainsKey(name))
            {
                headers[name] = value;
            }
        }
    }
}
