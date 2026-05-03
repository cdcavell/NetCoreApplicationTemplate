# .NET Core Application Template &nbsp;&nbsp;&nbsp;&nbsp;[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)&nbsp;&nbsp;![GitHub top language](https://img.shields.io/github/languages/top/cdcavell/NetCoreApplicationTemplate)&nbsp;&nbsp;![GitHub language count](https://img.shields.io/github/languages/count/cdcavell/NetCoreApplicationTemplate)

A reusable, production-oriented .NET application template designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

This project is intended to serve as a starting point for applications that need a consistent foundation for middleware ordering, logging, security headers, proxy support, rate limiting, centralized error handling, authentication integration, data access, and future template packaging.

## Project Goals

The purpose of this template is to provide a clean enterprise-ready baseline that can be reused across multiple applications while keeping common infrastructure concerns consistent and easy to maintain.

Primary goals include:

- Production-ready ASP.NET Core middleware configuration.
- Centralized application startup and pipeline organization.
- Serilog-based structured logging.
- Forwarded headers and reverse proxy support.
- Security header middleware.
- Rate limiting policies.
- Centralized exception and status code handling.
- EF Core data access patterns.
- SQLite support for development.
- SQL Server support through pluggable configuration.
- Authentication-ready architecture.
- Support for OIDC, SAML2, Microsoft, Google, and other external providers.
- Future packaging as a reusable .NET project template.
- Local Git development with GitHub remote repository support.

## Current Status

Initial repository and folder structure are in place.

Development has not yet started. This README will evolve as each area of the project is implemented.

## Repository Structure

```text
/
├── src/
│   └── Application source projects
│
├── tests/
│   └── Automated tests
│
├── docs/
│   └── Project documentation
│
├── templates/
│   └── Future .NET template packaging files
│
├── scripts/
│   └── Utility scripts for setup, build, or maintenance
│
├── .github/
│   └── GitHub workflows, issue templates, and repository metadata
│
├── .gitattributes
├── .gitignore
├── README.md
├── LICENSE.txt
└── ASSETS-LICENSES.md
```

## Application Architecture

This section will describe the high-level architecture of the template application.

Planned areas:

- Application startup structure.
- Dependency injection organization.
- Extension method conventions.
- Middleware composition.
- Environment-specific behavior.
- Configuration layering.
- Modular service registration.
- Separation of infrastructure, application, and web concerns.

## Middleware Pipeline

This section will document the standard middleware order used by the template.

Planned areas:

- Forwarded headers.
- Exception handling.
- HTTPS redirection.
- Static files.
- Routing.
- Security headers.
- CORS, if applicable.
- Authentication.
- Authorization.
- Rate limiting.
- Response caching, if applicable.
- Endpoint mapping.
- Status code handling.

## Logging

This template uses Serilog for structured application logging.

Serilog is configured as the primary logging provider so that application events, startup events, errors, and HTTP request activity are written using a consistent structured format.

#### Bootstrap Logging
The application logs a bootstrap message when the web application begins provider configuration:
```csharp
Log.Information("Bootstrapping Template.Web application");
```

#### Startup Logging
The application logs a startup message when the web application begins initialization:
```csharp
Log.Information("Starting Template.Web application");
```

#### Pipline Logging 
The application logs a startup message when the web application begins configuring the middleware pipeline:
```csharp
Log.Information("Configuring pipline for Template.Web application");
```

#### Runtime Logging
The application logs a startup message when the web application begins running:
```csharp
Log.Information("Running Template.Web application");
```

#### Ongoing Application Logging
While the application is running, structured logs are written for normal application activity, warnings, errors, and HTTP request processing. These logs help provide visibility into the current behavior of the application without requiring a debugger to be attached.

Runtime logging may include:

- Application lifecycle events
- Controller or endpoint activity
- HTTP request completion details
- Warnings from expected but noteworthy conditions
- Exceptions and unexpected failures
- Framework or infrastructure messages based on configured log levels

The default logging configuration is intended to capture useful operational information while avoiding sensitive data such as passwords, authentication tokens, cookies, request bodies, and response bodies.

Additional logging can be added throughout the application by injecting ILogger<T> into services, controllers, middleware, or other application components.

#### Bootstrap Exception Logging
The application logs any exceptions that occur during the bootstrapping process or while configuring the middleware pipeline:
```csharp
Log.Fatal(ex, "Template.Web application terminated unexpectedly");
```


## Error Handling

This section will document centralized exception handling and status code behavior.

Planned areas:

- Global exception handling.
- Status code pages.
- Error controller or endpoint strategy.
- Problem Details support.
- User-safe error responses.
- Developer exception pages for local development.
- Logging behavior for handled and unhandled errors.

## Security Headers

The template includes configurable security header middleware that applies common HTTP response headers to help reduce browser-based attack surface. The middleware is registered through the template extension pattern so `Program.cs` can remain clean and minimal.

Security headers are registered during service configuration:

```csharp
builder.Services.AddTemplateSecurityHeaders(builder.Configuration);
```
They are applied through the standard template pipeline:
```csharp
app.UseTemplatePipeline();
```
The pipeline calls:
```csharp
app.UseTemplateSecurityHeaders();
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
"Template": {
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

The default configuration is intentionally conservative. Applications created from this template can loosen or override headers in environment-specific settings files such as `appsettings.Development.json`.

For example, a local development configuration may temporarily disable CSP while troubleshooting script or style loading:
```json
"Template": {
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


## Forwarded Headers and Proxy Support

The template includes optional forwarded headers support for deployments behind reverse proxies,
load balancers, ingress controllers, and hosted infrastructure.

Forwarded headers allow the application to correctly resolve the original client IP address,
request scheme, and host when traffic is forwarded through another server before reaching Kestrel.

Configuration is controlled through `appsettings.json`:

```json
"Template": {
  "ForwardedHeaders": {
    "Enabled": true,
    "Headers": [
      "XForwardedFor",
      "XForwardedProto"
    ],
    "ForwardLimit": 1,
    "RequireHeaderSymmetry": false,
    "ClearKnownNetworksAndProxies": false,
    "KnownProxies": [],
    "KnownNetworks": [],
    "AllowedHosts": []
  }
}
```
By default, the template processes:

- `X-Forwarded-For`
- `X-Forwarded-Proto`

Production deployments should explicitly configure trusted proxy IP addresses or trusted proxy
networks using `KnownProxies` or `KnownNetworks`.

`XForwardedHost` is intentionally not enabled by default. If enabled, configure `AllowedHosts`
to reduce the risk of host header spoofing.

## Rate Limiting

The template includes baseline ASP.NET Core rate limiting support to help protect applications from accidental request floods, scraping, repeated automated requests, and concurrency-heavy operations.

Rate limiting is registered through the template service extension:

```csharp
builder.Services.AddTemplateRateLimiting(builder.Configuration);
```
The middleware is applied in the standard template pipeline:
```csharp
app.UseRateLimiter();
```
`UseRateLimiter()` is intentionally placed after routing so endpoint-specific rate limiting policies can be applied, and before endpoint execution so requests can be rejected before reaching controllers, Razor Pages, or minimal API handlers.

### Default Behavior

The template supports:

- A global fixed-window limiter for baseline request protection.
- A named fixed-window policy for endpoint-specific use.
- A named concurrency policy for sensitive or resource-heavy operations.
- JSON rejection responses.
- `429 Too Many Requests` responses when limits are exceeded.
- Logging for rejected requests.

Rejected requests return a response similar to:
```json
{
  "error": "Too many requests.",
  "statusCode": 429
}
```
Configuration

Rate limiting values can be configured from `appsettings.json`:
```json
"Template": {
  "RateLimiting": {
    "Enabled": true,
    "GlobalFixedWindow": {
      "PermitLimit": 60,
      "WindowSeconds": 60,
      "QueueLimit": 0
    },
    "FixedWindowPolicy": {
      "PermitLimit": 60,
      "WindowSeconds": 60,
      "QueueLimit": 0
    },
    "ConcurrencyPolicy": {
      "PermitLimit": 10,
      "QueueLimit": 0
    }
  }
}
```
These defaults are intentionally conservative and should be reviewed before production use.

### Endpoint-Specific Policies

Named policies can be applied to specific endpoints when stricter or specialized protection is needed.

Minimal API example:
```csharp
app.MapGet("/api/data", () => "Limited endpoint")
    .RequireRateLimiting("fixed");
```
Concurrency-sensitive endpoint example:
```csharp
app.MapPost("/admin/export", () => "Export started")
    .RequireRateLimiting("concurrency");
```
Controller or Razor Page handlers can also use rate limiting attributes:
```csharp
using Microsoft.AspNetCore.RateLimiting;

[EnableRateLimiting("fixed")]
public class ReportsController : Controller
{
}
```
### Middleware Order

The template pipeline applies rate limiting after routing:
```csharp
app.UseRouting();

app.UseCors();

app.UseRateLimiter();
```
If a future policy depends on the authenticated user identity, rate limiting may need to move after authentication so user-specific partitioning can be applied.

## Authentication and Authorization

This section will document authentication and authorization architecture.

Planned areas:

- Cookie authentication.
- OpenID Connect.
- SAML2.
- Microsoft authentication.
- Google authentication.
- Social provider modules.
- External login configuration.
- Claims transformation.
- Authorization policies.
- Role and permission patterns.
- Pluggable authentication module design.

## Data Access

This section will document EF Core and database patterns.

Planned areas:

- DbContext registration.
- SQLite development configuration.
- SQL Server production configuration.
- Connection string management.
- Migrations.
- Repository or service patterns, if used.
- Transaction handling.
- Retry strategies.
- Concurrency handling.
- Auditing fields.
- Soft delete patterns, if applicable.

## Configuration

This section will document application configuration strategy.

Planned areas:

- appsettings.json.
- appsettings.Development.json.
- User secrets.
- Environment variables.
- Provider-specific configuration.
- Options pattern.
- Strongly typed settings.
- Validation on startup.
- Sensitive configuration handling.

## Template Packaging

This section will document how the project will eventually be packaged as a reusable template.

Planned areas:

- .template.config setup.
- Template parameters.
- Project naming replacements.
- Optional modules.
- Authentication provider variants.
- Database provider variants.
- Local installation.
- Template testing.
- Publishing strategy.

## Development Environment

This section will document local development requirements.

Planned tools:

- Visual Studio 2026.
- .NET SDK.
- Git.
- GitHub remote repository.
- SQLite for local development.
- SQL Server support for production-style configuration.

## Getting Started

Development instructions will be added as the project implementation begins.

Planned setup steps:

- Clone the repository.
- Restore NuGet packages.
- Configure local settings.
- Apply database migrations.
- Run the application.
- Execute tests.

## Build and Test

Build and test instructions will be added once the solution structure is finalized

Planned commands:

```
dotnet restore
dotnet build
dotnet test
```

## Git Workflow

This project uses Git for local source control with a remote repository hosted on GitHub.

Recommended branch naming:

```
main
feature/issue-<issue-number>
fix/issue-<issue-number>
docs/issue-<issue-number>
refactor/issue-<issue-number>
```
_example: feature/issue-42 will cause Issue #42 status to change to `In Progress` when branch is created._

Recommended commit style:

```
Add initial repository attributes #<issue-number>
Add template README scaffold #<issue-number>
Implement security header middleware #<issue-number>
Configure Serilog request logging #<issue-number>
Add EF Core SQLite provider #<issue-number>
```

#### Required Secret

This workflow requires a classic GitHub personal access token stored as:

`PROJECT_TOKEN`

Required classic PAT scopes:

- `project`
- `repo` if the repository is private
- `public_repo` may be sufficient if the repository is public

_A fine-grained PAT may not work for user-owned GitHub Projects._

## Documentation

Additional documentation will be added under the docs/ folder as the project grows.

Planned documentation:

- Architecture overview.
- Middleware pipeline notes.
- Security configuration.
- Authentication provider setup.
- Database provider setup.
- Template packaging guide.
- Deployment notes.

## Roadmap

Initial planned milestones:

- [x]  Add repository metadata files.
- [x]  Create initial solution structure.
- [x]  Add baseline ASP.NET Core web application.
- [x]  Add centralized service registration.
- [x]  Add production middleware pipeline.
- [x]  Add Serilog logging.
- [x]  Add security headers.
- [x]  Add forwarded headers support.
- [x]  Add rate limiting policies.
- [ ]  Add centralized error handling.
- [ ]  Add EF Core with SQLite.
- [ ]  Add SQL Server provider option.
- [ ]  Add authentication module structure.
- [ ]  Add OIDC support.
- [ ]  Add SAML2 support.
- [ ]  Add external provider support.
- [ ]  Add template packaging.
- [x]  Add GitHub workflows.
- [x]  Add documentation.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).

