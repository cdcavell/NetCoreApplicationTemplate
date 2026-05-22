## Security Headers

The application includes configurable security header middleware that applies common HTTP response headers to help reduce browser-based attack surface. The middleware is registered through the application extension pattern so `Program.cs` can remain clean and minimal.

Security headers are registered during service configuration:

```csharp
builder.Services.AddApplicationSecurityHeaders(builder.Configuration);
```
They are applied through the standard application pipeline:
```csharp
app.UseApplicationPipeline();
```
The pipeline calls:
```csharp
app.UseApplicationSecurityHeaders();
```
### Default Headers

By default, the middleware applies the following headers when enabled:

|Header|Default Value|Purpose|
|:-----|:------------|:------|
|`X-Content-Type-Options`|`nosniff`|Prevents MIME type sniffing by browsers, reducing the risk of drive-by downloads and content injection attacks.|
|`X-Frame-Options`|`DENY`|Prevents the application from being framed by another site.|
|`Referrer-Policy`|`strict-origin-when-cross-origin`|Limits how much referrer information is sent during navigation.|
|`X-Permitted-Cross-Domain-Policies`|`none`|Blocks legacy cross-domain policy files.|
|`Cross-Origin-Opener-Policy`|`same-origin`|Helps isolate the browsing context from cross-origin documents.|
|`Cross-Origin-Resource-Policy`|`same-origin`|Restricts other origins from loading application resources.|
|`Permissions-Policy`|Configurable|Disables or limits browser features such as camera, microphone, geolocation, payment, USB, and fullscreen.|
|`Content-Security-Policy`|Configurable|Restricts where scripts, styles, images, forms, frames, and other resources may load from.|

The middleware intentionally does not add `X-XSS-Protection` because that header is obsolete and can create inconsistent behavior in modern browsers.

### Configuration

Security headers can be configured from `appsettings.json`:
```json
"ProjectTemplate": {
  "SecurityHeaders": {
    "Enabled": true,
    "EnableContentSecurityPolicy": true,
    "EnablePermissionsPolicy": true,
    "EnableCrossOriginHeaders": true,
    "ContentSecurityPolicy": "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; img-src 'self' data:; script-src 'self'; style-src 'self' 'unsafe-inline';",
    "PermissionsPolicy": "camera=(), microphone=(), geolocation=(), payment=(), usb=(), fullscreen=(self)",
    "ExcludedPathPrefixes": [
      "/health",
      "/metrics"
    ]
  }
}
```

### Configuration Options

|Option|Purpose|
|:-----|:------|
|`Enabled`|Enables or disables the security header middleware.|
|`EnableContentSecurityPolicy`|Controls whether the `Content-Security-Policy` header is applied.|
|`EnablePermissionsPolicy`|Controls whether the `Permissions-Policy` header is applied.|
|`EnableCrossOriginHeaders`|Controls whether `Cross-Origin-Opener-Policy` and `Cross-Origin-Resource-Policy` are applied.|
|`ContentSecurityPolicy`|Defines the application Content Security Policy value.|
|`PermissionsPolicy`|Defines the Permissions Policy value.|
|`ExcludedPathPrefixes`|Skips security header application for matching request path prefixes.|

Environment-Specific Behavior

The default configuration is intentionally conservative. Applications created from this application can loosen or override headers in environment-specific settings files such as `appsettings.Development.json`.

For example, a local development configuration may temporarily disable CSP while troubleshooting script or style loading:
```json
"ProjectTemplate": {
  "SecurityHeaders": {
    "EnableContentSecurityPolicy": false
  }
}
```
Production applications should use the strongest policy possible for the deployed application. In particular, production deployments should avoid broad CSP allowances such as `unsafe-inline` where practical and should only allow trusted script, style, image, frame, and connection sources.

### Excluded Paths

The default excluded paths are:
```json
[
  "/health",
  "/metrics"
]
```
These paths are commonly used by infrastructure, monitoring tools, or container orchestration systems. Additional paths can be excluded if needed.

### Testing Response Headers

Run the application and inspect the response headers from the root endpoint:
```bash
curl -k -I https://localhost:5001/
```
Expected headers include:
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: strict-origin-when-cross-origin
X-Permitted-Cross-Domain-Policies: none
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Resource-Policy: same-origin
Permissions-Policy: camera=(), microphone=(), geolocation=(), payment=(), usb=(), fullscreen=(self)
Content-Security-Policy: default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; img-src 'self' data:; script-src 'self'; style-src 'self' 'unsafe-inline';
```
The exact CSP and Permissions-Policy values may differ if overridden by configuration.
