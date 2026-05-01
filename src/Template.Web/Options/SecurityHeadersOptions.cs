namespace Template.Web.Options
{
    public sealed class SecurityHeadersOptions
    {
        public bool Enabled { get; set; } = true;

        public bool EnableContentSecurityPolicy { get; set; } = true;

        public bool EnablePermissionsPolicy { get; set; } = true;

        public bool EnableCrossOriginHeaders { get; set; } = true;

        public string ContentSecurityPolicy { get; set; } =
            "default-src 'self'; " +
            "base-uri 'self'; " +
            "object-src 'none'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self'; " +
            "img-src 'self' data:; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline';";

        public string PermissionsPolicy { get; set; } =
            "camera=(), microphone=(), geolocation=(), payment=(), usb=(), fullscreen=(self)";

        public List<string> ExcludedPathPrefixes { get; set; } = new()
        {
            "/health",
            "/metrics"
        };
    }
}
