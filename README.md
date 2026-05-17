# .NET Core Application Template 
[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)
[![Documentation](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://cdcavell.github.io/NetCoreApplicationTemplate/)
[![GitHub Release](https://img.shields.io/github/v/release/cdcavell/NetCoreApplicationTemplate?display_name=tag)](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/cdcavell/NetCoreApplicationTemplate)](LICENSE.txt)

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

## Versioning

This project follows Semantic Versioning using the format:

```text
MAJOR.MINOR.PATCH
```
Version numbers are centrally managed through project build metadata so assemblies, future packages, and releases can share a consistent version identity.

## Current Release

<!-- BEGIN LATEST_RELEASE -->
Current release: __[Release 0.2.4](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/tag/v0.2.4)__

Tag: `v0.2.4`
<!-- END LATEST_RELEASE -->

## Documentation

The generated project documentation is published with DocFX and GitHub Pages:

[View Documentation](https://cdcavell.github.io/NetCoreApplicationTemplate/)

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
### Structured Request Logging

The template includes structured HTTP request logging through Serilog.

Request logging records a single completion event for each normal request and includes:

- HTTP method
- Request path
- Response status code
- Elapsed request duration
- Request ID
- Correlation ID
- Request scheme and host
- Remote IP address, when enabled
- Authenticated user name, when enabled

Request logging is configured through:

```csharp
builder.Services.AddTemplateRequestLogging(builder.Configuration);
```
And applied through the standard template pipeline:
```csharp
app.UseTemplateRequestLogging();
```
Configuration is controlled through `appsettings.json`:
```json
"Template": {
  "RequestLogging": {
    "Enabled": true,
    "CorrelationHeaderName": "X-Correlation-ID",
    "IncludeQueryString": false,
    "IncludeUserName": true,
    "IncludeRemoteIpAddress": true,
    "ExcludedPathPrefixes": [
      "/health",
      "/metrics",
      "/favicon.ico",
      "/css",
      "/js",
      "/lib",
      "/_framework"
    ]
  }
}
```
Query string logging is disabled by default because query strings may contain sensitive values. Applications should avoid logging request bodies, response bodies, cookies, authorization headers, access tokens, refresh tokens, or authentication payloads unless a specific, reviewed diagnostic need exists.


## Error Handling

The template includes centralized error handling for both unhandled exceptions and HTTP status code responses.

Error handling is configured through the application pipeline using:

```csharp
app.UseTemplateErrorHandling();
```
The error handling behavior is environment-aware:

- In Development, the application uses the developer exception page.
- In non-development environments, unhandled exceptions are routed to `/Home/Error/500`.
- HTTP status code responses are re-executed through `/Home/Error/{statusCode}`.
- Error responses are user-safe and do not expose exception details.
- Error events are logged using source-generated `LoggerMessage` methods.
- The request ID displayed on the error page matches the request ID written to the application logs.

### Status Code Pages

Status code pages are handled centrally using:
```csharp
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
```
This allows common HTTP responses such as `404 Not Found`, `401 Unauthorized`, `403 Forbidden`, and `429 Too Many Requests` to use the shared error page strategy.

### Unhandled Exceptions

In non-development environments, unhandled exceptions are routed through:
```csharp
app.UseExceptionHandler("/Home/Error/500");
```
This allows unhandled exceptions to be logged and displayed using the shared error page strategy without exposing sensitive exception details to end users.

### Request ID Correlation

The error page displays the same request ID that is written to the log entry.

Example browser output:
```text
Request ID: 0HNL9ADUFCPUT:00000009
```
Example log output:
```text
Status code page routed to error page. StatusCode: 404; OriginalPath: /invalid; RemoteIpAddress: ::1; RequestId: 0HNL9ADUFCPUT:00000009
```
This makes it easier to match a user-facing error page with the corresponding application log entry.

### Logging

Error handling logs include:
- Status code routed to the error page.
- Original request path.
- Remote IP address when available.
- Request ID.
- Exception details for unhandled exceptions.

Log event IDs are centralized in TemplateLogEventIds to keep application logging consistent.

### Centralized Problem Details Error Handling

The application uses centralized error handling to provide consistent responses for both browser and API-style requests.

Browser requests are routed to the standard application error page, such as `/Home/Error/{statusCode}`. API, AJAX, and JSON-oriented requests receive a Problem Details response using the ASP.NET Core `IProblemDetailsService`.

Problem Details responses include safe metadata such as:

- HTTP status code
- Error title
- Request path
- Trace ID
- Request ID

Detailed exception information is only exposed in the Development environment. Production responses avoid leaking stack traces, exception messages, connection details, or internal implementation information.

Unhandled exceptions are logged centrally and converted into safe Problem Details responses when the request expects JSON.

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

## Health Checks

The template includes baseline ASP.NET Core health check endpoints for local development, reverse proxy hosting, load balancers, container platforms, and future deployment scenarios.

Health checks are registered during service configuration:

```csharp
builder.Services.AddTemplateHealthChecks();
```
The endpoints are mapped during application startup:
```csharp
app.MapTemplateHealthChecks();
``` 
### Default Endpoints
|Endpoint|Purpose|
|:-------|:------|
|/health|General application health endpoint.|
|/health/ready|Readiness endpoint intended for dependency-aware checks such as database, cache, or external service availability.|
|/health/live|Liveness endpoint intended to verify that the application process can respond.|

The baseline template does not yet register database or external dependency checks. Future modules, such as EF Core, SQL Server, authentication providers, or external integrations, can add tagged checks for readiness scenarios.

Example future readiness check:
```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: new[] { "ready" });
```
### Reverse Proxy and Hosting Use

Health endpoints are intended for infrastructure-level checks from reverse proxies, load balancers, deployment platforms, and monitoring systems.

Typical uses include:

- Confirming the application process is running.
- Removing an unhealthy instance from load balancing rotation.
- Supporting future container or deployment probes.
- Providing a stable infrastructure path that avoids normal browser-facing error pages.
### Security Headers

The default security header configuration excludes `/health`:
```json
"ExcludedPathPrefixes": [
  "/health",
  "/metrics"
]
```
Because the exclusion is prefix-based, `/health`, `/health/ready`, and `/health/live` are all excluded from security header application. This keeps health probe responses small and infrastructure-friendly.
```text

## Run tests

```bash
dotnet test
```
## Rate Limiting

The template includes baseline ASP.NET Core rate limiting support to help protect applications from accidental request floods, scraping, repeated automated requests, and concurrency-heavy operations.

Rate limiting is registered through the template service extension:

```csharp
builder.Services.AddTemplateRateLimiting(builder.Configuration, builder.Environment);
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

### Automated Test Strategy

Rate limiting behavior is covered by integration tests under `tests/Template.Web.Tests`.

The tests use `WebApplicationFactory<Program>` to boot the real `Template.Web` pipeline in memory, override rate limiting configuration with in-memory settings, and register a test-only MVC controller from the test assembly.

This keeps production endpoints unchanged while allowing the tests to verify:

- Global fixed-window limiter behavior.
- Named fixed-window policy behavior.
- Named concurrency policy behavior.
- JSON `429 Too Many Requests` rejection responses.
- Disabled rate limiting behavior.
- Configuration binding for template rate limiting options.

## OpenTelemetry

The template includes baseline OpenTelemetry support for tracing and metrics.

OpenTelemetry is registered through:

```csharp
builder.Services.AddTemplateOpenTelemetry(builder.Configuration, builder.Environment);
```
Configuration is controlled through `appsettings.json`:
```json
"Template": {
  "OpenTelemetry": {
    "Enabled": true,
    "ServiceName": "Template.Web",
    "ServiceVersion": "0.1.3",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableAspNetCoreInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "Otlp": {
      "Enabled": false,
      "Endpoint": "",
      "Protocol": "Grpc"
    }
  }
}
```
By default, the template collects local tracing and metrics instrumentation but does not export telemetry to an external collector. To enable OTLP export, configure an OTLP endpoint and set `Template:OpenTelemetry:Otlp:Enabled` to `true`.

Common local OTLP collector endpoints:
```text
http://localhost:4317
http://localhost:4318
```
The OTLP exporter can also be configured through standard OpenTelemetry environment variables such as `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL`.

## Authentication and Authorization

### Default Authentication Posture

The base template enables the template authentication module and local cookie authentication by default.

By default:

- `Template:Authentication:Enabled` is `true`.
- The default authenticate, challenge, and sign-in schemes use `Cookies`.
- Local cookie authentication is enabled.
- External providers such as OpenID Connect, SAML2, Microsoft, Google, and GitHub are disabled.

This gives applications a working local authentication baseline while keeping external identity provider integration opt-in.

To enable an external provider, keep template authentication enabled and set only the required provider configuration to enabled. For example, OIDC requires `Template:Authentication:Providers:OpenIdConnect:Enabled` to be set to `true` along with valid authority, client ID, and client secret values.

### OpenID Connect

The template includes standards-based OpenID Connect authentication support.
External OIDC provider integration is disabled by default. To enable it, configure the `Template:Authentication` section and set both authentication and the OpenID Connect provider to enabled.

```json
"Template": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "OpenIdConnect",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "OpenIdConnect": {
        "Enabled": true,
        "Scheme": "OpenIdConnect",
        "DisplayName": "OpenID Connect",
        "Authority": "https://login.example.com",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-oidc",
        "ResponseType": "code",
        "SaveTokens": true,
        "Scopes": [
          "openid",
          "profile",
          "email"
        ]
      }
    }
  }
}
```
_Do not commit real client secrets to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

### SAML2

The template includes standards-based SAML2 authentication support.
External SAML2 provider integration is disabled by default. To enable it, configure the `Template:Authentication` section and set both authentication and the Saml2 provider to enabled.
```json
"Template": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "Saml2",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "Saml2": {
        "Enabled": true,
        "Scheme": "Saml2",
        "DisplayName": "SAML2",
        "EntityId": "https://localhost:5001/saml2",
        "MetadataUrl": "https://idp.example.com/metadata",
        "ModulePath": "/Saml2/Acs",
        "LoadMetadata": true,
        "RequireSignedAssertions": true,
        "ValidateCertificates": true
      }
    }
  }
}
```
_Do not commit real certificates, private keys, or real IdP metadata to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

### Microsoft External Provider

The template includes Microsoft external authentication support through `Microsoft.AspNetCore.Authentication.MicrosoftAccount`.

The Microsoft provider is disabled by default and only registers when:

`Template:Authentication:Providers:Microsoft:Enabled`

is set to `true`.

```json
"Template": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "Microsoft",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "Microsoft": {
        "Enabled": true,
        "Scheme": "Microsoft",
        "DisplayName": "Microsoft",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-microsoft",
        "Scopes": []
      }
    }
  }
}
```
_Do not commit real client IDs, client secrets, certificates, tokens, or provider credentials to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._
### Google External Provider

The template includes Google external authentication support through `Microsoft.AspNetCore.Authentication.Google`.

The Google provider is disabled by default and only registers when:

`Template:Authentication:Providers:Google:Enabled`

is set to `true`.

```json
"Template": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "Google",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "Google": {
        "Enabled": true,
        "Scheme": "Google",
        "DisplayName": "Google",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-google",
        "Scopes": [
          "profile",
          "email"
        ]
      }
    }
  }
}
```
_Do not commit real client IDs, client secrets, certificates, tokens, or provider credentials to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._
### External Providers

The template includes foundational external provider configuration for Google, GitHub, and future OAuth/OIDC-compatible providers.
External providers are disabled by default. This issue provides the configuration and extension-point structure only. Production provider registration, account linking, MFA enforcement, and provider-specific setup are handled by future dedicated issues.
```json
"Template": {
  "Authentication": {
    "GitHub": {
      "Enabled": false,
      "Scheme": "GitHub",
      "DisplayName": "GitHub",
      "ClientId": "",
      "ClientSecret": "",
      "CallbackPath": "/signin-github"
    }
  }
}
```
_Do not commit real client IDs, client secrets, certificates, tokens, or provider credentials to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

### Authentication Provider Startup Validation

Authentication provider configuration is validated during application startup.

Provider-specific values are only required when that provider is enabled. Disabled providers may keep placeholder or empty values so the base template remains safe to run without external identity-provider setup.

When a provider is enabled, startup validation fails fast if required values are missing. Validation messages identify the missing configuration key, but do not log configured secret values.

Validated providers include:

- OpenID Connect
- SAML2
- Microsoft
- Google
- GitHub

This prevents partially configured authentication providers from failing later during runtime login flows.

### Baseline Authentication Endpoints

The template provides minimal account and external authentication endpoints:

| Endpoint | Purpose |
|---|---|
| `GET /Account/Login` | Displays the baseline login page and available registered external providers. |
| `POST /Account/Logout` | Signs out of the local cookie session. Requires anti-forgery validation. |
| `GET /Account/AccessDenied` | Displays a safe access denied response. |
| `GET /External/Challenge` | Starts an external authentication challenge for a registered provider scheme. |

`/External/Challenge` accepts a `provider` value and an optional `returnUrl`.

Return URLs are validated as local URLs before redirecting to avoid open redirect vulnerabilities. Unknown provider schemes are rejected safely. Provider secrets, tokens, cookies, and sensitive query-string values should not be logged.

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

## Configuration Validation

The template uses strongly typed options classes for template-owned configuration under the `Template` section.

Validated configuration areas include:

- `Template:SecurityHeaders`
- `Template:ForwardedHeaders`
- `Template:RateLimiting`

Invalid startup-sensitive values fail application startup rather than being silently corrected. This helps catch unsafe or malformed production configuration before the application begins serving requests.

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

## Testing

The repository includes automated tests under `tests/Template.Web.Tests`.

The test project uses `WebApplicationFactory<Program>` to start the real `Template.Web`
application pipeline in memory. Tests can override configuration through in-memory
settings and can register test-only MVC controllers from the test assembly.

### Current integration coverage includes:

- Health check endpoints.
- Security header middleware behavior.
- Forwarded header behavior.
- Rate limiting policies.
- JSON 429 rejection responses.
- Configuration binding and validation.

### Run tests with:

```bash
dotnet test
```

### Run locally:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```
### Code Coverage

Automated test runs collect code coverage using Coverlet.

Coverage reports are generated during CI using ReportGenerator and published as GitHub Actions artifacts.

The initial line coverage threshold is:

```text
60%
```
This threshold is intentionally modest while the template is still growing. It should be raised over time as additional modules, data access features, authentication providers, and template packaging tests are added.


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
- [x]  Add centralized error handling.
- [ ]  Add EF Core with SQLite.
- [ ]  Add SQL Server provider option.
- [x]  Add authentication module structure.
- [x]  Add OIDC support.
- [x]  Add SAML2 support.
- [x]  Add external provider support.
- [ ]  Add template packaging.
- [x]  Add GitHub workflows.
- [x]  Add documentation.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).

