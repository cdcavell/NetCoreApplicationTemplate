# .NET Core Application Template 
[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)
[![Documentation](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://cdcavell.github.io/NetCoreApplicationTemplate/)
[![GitHub Release](https://img.shields.io/github/v/release/cdcavell/NetCoreApplicationTemplate?display_name=tag)](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/cdcavell/NetCoreApplicationTemplate)](LICENSE.txt)

A reusable, production-oriented .NET application application designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

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
Current release: __[Release 0.3.1](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/tag/v0.3.1)__

Tag: `v0.3.1`
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

This section will describe the high-level architecture of the application.

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

This section will document the standard middleware order used by the application.

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

This application uses Serilog for structured application logging.

Serilog is configured as the primary logging provider so that application events, startup events, errors, and HTTP request activity are written using a consistent structured format.

#### Bootstrap Logging
The application logs a bootstrap message when the web application begins provider configuration:
```csharp
Log.Information("Bootstrapping ProjectTemplate.Web application");
```

#### Startup Logging
The application logs a startup message when the web application begins initialization:
```csharp
Log.Information("Starting ProjectTemplate.Web application");
```

#### Pipline Logging 
The application logs a startup message when the web application begins configuring the middleware pipeline:
```csharp
Log.Information("Configuring pipline for ProjectTemplate.Web application");
```

#### Runtime Logging
The application logs a startup message when the web application begins running:
```csharp
Log.Information("Running ProjectTemplate.Web application");
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
Log.Fatal(ex, "ProjectTemplate.Web application terminated unexpectedly");
```
### Structured Request Logging

The application includes structured HTTP request logging through Serilog.

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
builder.Services.AddApplicationRequestLogging(builder.Configuration);
```
And applied through the standard application pipeline:
```csharp
app.UseApplicationRequestLogging();
```
Configuration is controlled through `appsettings.json`:
```json
"ProjectTemplate": {
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

The application includes centralized error handling for both unhandled exceptions and HTTP status code responses.

Error handling is configured through the application pipeline using:

```csharp
app.UseApplicationErrorHandling();
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

Log event IDs are centralized in ApplicationLogEventIds to keep application logging consistent.

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

## Forwarded Headers and Proxy Support

The application includes optional forwarded headers support for deployments behind reverse proxies,
load balancers, ingress controllers, and hosted infrastructure.

Forwarded headers allow the application to correctly resolve the original client IP address,
request scheme, and host when traffic is forwarded through another server before reaching Kestrel.

Configuration is controlled through `appsettings.json`:

```json
"ProjectTemplate": {
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
By default, the application processes:

- `X-Forwarded-For`
- `X-Forwarded-Proto`

Production deployments should explicitly configure trusted proxy IP addresses or trusted proxy
networks using `KnownProxies` or `KnownNetworks`.

`XForwardedHost` is intentionally not enabled by default. If enabled, configure `AllowedHosts`
to reduce the risk of host header spoofing.

## Health Checks

The application includes baseline ASP.NET Core health check endpoints for local development, reverse proxy hosting, load balancers, container platforms, and future deployment scenarios.

Health checks are registered during service configuration:

```csharp
builder.Services.AddApplicationHealthChecks();
```
The endpoints are mapped during application startup:
```csharp
app.MapApplicationHealthChecks();
``` 
### Default Endpoints
|Endpoint|Purpose|
|:-------|:------|
|/health|General application health endpoint.|
|/health/ready|Readiness endpoint intended for dependency-aware checks such as database, cache, or external service availability.|
|/health/live|Liveness endpoint intended to verify that the application process can respond.|

The baseline application does not yet register database or external dependency checks. Future modules, such as EF Core, SQL Server, authentication providers, or external integrations, can add tagged checks for readiness scenarios.

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

The application includes baseline ASP.NET Core rate limiting support to help protect applications from accidental request floods, scraping, repeated automated requests, and concurrency-heavy operations.

Rate limiting is registered through the application service extension:

```csharp
builder.Services.AddApplicationRateLimiting(builder.Configuration, builder.Environment);
```
The middleware is applied in the standard application pipeline:
```csharp
app.UseRateLimiter();
```
`UseRateLimiter()` is intentionally placed after routing so endpoint-specific rate limiting policies can be applied, and before endpoint execution so requests can be rejected before reaching controllers, Razor Pages, or minimal API handlers.

### Default Behavior

The application supports:

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
"ProjectTemplate": {
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

The application pipeline applies rate limiting after routing:
```csharp
app.UseRouting();

app.UseCors();

app.UseRateLimiter();
```
If a future policy depends on the authenticated user identity, rate limiting may need to move after authentication so user-specific partitioning can be applied.

### Automated Test Strategy

Rate limiting behavior is covered by integration tests under `tests/ProjectTemplate.Web.Tests`.

The tests use `WebApplicationFactory<Program>` to boot the real `ProjectTemplate.Web` pipeline in memory, override rate limiting configuration with in-memory settings, and register a test-only MVC controller from the test assembly.

This keeps production endpoints unchanged while allowing the tests to verify:

- Global fixed-window limiter behavior.
- Named fixed-window policy behavior.
- Named concurrency policy behavior.
- JSON `429 Too Many Requests` rejection responses.
- Disabled rate limiting behavior.
- Configuration binding for application rate limiting options.

## OpenTelemetry

The application includes baseline OpenTelemetry support for tracing and metrics.

OpenTelemetry is registered through:

```csharp
builder.Services.AddApplicationOpenTelemetry(builder.Configuration, builder.Environment);
```
Configuration is controlled through `appsettings.json`:
```json
"ProjectTemplate": {
  "OpenTelemetry": {
    "Enabled": true,
    "ServiceName": "ProjectTemplate.Web",
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
By default, the application collects local tracing and metrics instrumentation but does not export telemetry to an external collector. To enable OTLP export, configure an OTLP endpoint and set `ProjectTemplate:OpenTelemetry:Otlp:Enabled` to `true`.

Common local OTLP collector endpoints:
```text
http://localhost:4317
http://localhost:4318
```
The OTLP exporter can also be configured through standard OpenTelemetry environment variables such as `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL`.

## Authentication and Authorization

### Default Authentication Posture

The base application enables the application authentication module and local cookie authentication by default.

By default:

- `ProjectTemplate:Authentication:Enabled` is `true`.
- The default authenticate, challenge, and sign-in schemes use `Cookies`.
- Local cookie authentication is enabled.
- External providers such as OpenID Connect, SAML2, Microsoft, Google, and GitHub are disabled.

This gives applications a working local authentication baseline while keeping external identity provider integration opt-in.

To enable an external provider, keep application authentication enabled and set only the required provider configuration to enabled. For example, OIDC requires `ProjectTemplate:Authentication:Providers:OpenIdConnect:Enabled` to be set to `true` along with valid authority, client ID, and client secret values.

### OpenID Connect

The application includes standards-based OpenID Connect authentication support.
External OIDC provider integration is disabled by default. To enable it, configure the `ProjectTemplate:Authentication` section and set both authentication and the OpenID Connect provider to enabled.

```json
"ProjectTemplate": {
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

The application includes standards-based SAML2 authentication support.
External SAML2 provider integration is disabled by default. To enable it, configure the `ProjectTemplate:Authentication` section and set both authentication and the Saml2 provider to enabled.
```json
"ProjectTemplate": {
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

The application includes Microsoft external authentication support through `Microsoft.AspNetCore.Authentication.MicrosoftAccount`.

The Microsoft provider is disabled by default and only registers when:

`ProjectTemplate:Authentication:Providers:Microsoft:Enabled`

is set to `true`.

```json
"ProjectTemplate": {
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

The application includes Google external authentication support through `Microsoft.AspNetCore.Authentication.Google`.

The Google provider is disabled by default and only registers when:

`ProjectTemplate:Authentication:Providers:Google:Enabled`

is set to `true`.

```json
"ProjectTemplate": {
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

### GitHub External Provider
The application includes Google external authentication support through `AspNet.Security.OAuth.GitHub`.

The GitHub provider is disabled by default and only registers when:

`ProjectTemplate:Authentication:Providers:GitHub:Enabled`

is set to `true`.

```json
"ProjectTemplate": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "GitHub",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "GitHub": {
        "Enabled": true,
        "Scheme": "GitHub",
        "DisplayName": "GitHub",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-github",
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

### Authentication Provider Startup Validation

Authentication provider configuration is validated during application startup.

Provider-specific values are only required when that provider is enabled. Disabled providers may keep placeholder or empty values so the base application remains safe to run without external identity-provider setup.

When a provider is enabled, startup validation fails fast if required values are missing. Validation messages identify the missing configuration key, but do not log configured secret values.

Validated providers include:

- OpenID Connect
- SAML2
- Microsoft
- Google
- GitHub

This prevents partially configured authentication providers from failing later during runtime login flows.

### Baseline Authentication Endpoints

The application provides minimal account and external authentication endpoints:

| Endpoint | Purpose |
|---|---|
| `GET /Account/Login` | Displays the baseline login page and available registered external providers. |
| `POST /Account/Logout` | Signs out of the local cookie session. Requires anti-forgery validation. |
| `GET /Account/AccessDenied` | Displays a safe access denied response. |
| `GET /External/Challenge` | Starts an external authentication challenge for a registered provider scheme. |

`/External/Challenge` accepts a `provider` value and an optional `returnUrl`.

Return URLs are validated as local URLs before redirecting to avoid open redirect vulnerabilities. Unknown provider schemes are rejected safely. Provider secrets, tokens, cookies, and sensitive query-string values should not be logged.

### External Social Provider Strategy and OpenIddict Client Evaluation

The application currently uses provider-specific ASP.NET Core authentication handlers for Microsoft, Google, and GitHub. This keeps the implementation simple, scheme-based, and consistent with the existing authentication module structure.

Current provider-specific packages remain supported and are the active implementation path for this application. They are disabled by default, registered only when enabled, validated during startup, and configured through:

```text
ProjectTemplate:Authentication:Providers:Microsoft
ProjectTemplate:Authentication:Providers:Google
ProjectTemplate:Authentication:Providers:GitHub
```
OpenIddict Client was evaluated as a future external social provider architecture. OpenIddict Client provides a broader OAuth 2.0/OpenID Connect client stack with web-provider integrations for many external providers, including GitHub, Microsoft, and Google. It also provides stronger long-term capabilities such as OpenID Connect support, stateful client behavior, replay protections, discovery support, token introspection/revocation support, and resilient backchannel behavior.

However, adopting OpenIddict Client would be an architectural migration rather than a direct package swap. A migration would need to account for:
- OpenIddict client/core service registration.
- Token/state storage requirements.
- Provider-specific redirect endpoint design.
- Callback endpoint/controller handling.
- Existing `/External/Challenge` behavior.
- Existing startup validation behavior.
- Existing tests and documentation.
- Compatibility with Microsoft, Google, GitHub, and future providers.

OpenIddict Client may be implemented as the preferred candidate for a future broader social-provider architecture if the application later needs a unified OAuth/OIDC client model across many providers or advanced token-handling features.

Any future migration would be handled through a dedicated implementation issue and should preserve the existing working Microsoft, Google, and GitHub behavior until a replacement path is fully tested.

### Claims Transformation and Normalization

The application includes an optional claims transformation layer that normalizes provider-specific claims into application-owned claim names.

External identity providers often use different claim names for the same concept. For example, one provider may emit `sub`, another may emit `nameidentifier`, and another may use a SAML claim URI. The claims transformation layer allows these inputs to be mapped into consistent application claim names such as:

- `application:subject`
- `application:name`
- `application:email`
- `application:role`
- `application:group`
- `application:permission`

Original provider claims are preserved by default. They are only removed when `ProjectTemplate:Authentication:ClaimsTransformation:RemoveOriginalClaims` is explicitly set to `true`.

### Role and Permission Authorization Policies

The application includes baseline authorization policy patterns for authenticated users, role-based access, and permission-based access.

Default policy names:

| Policy | Purpose |
|---|---|
| `ProjectTemplate.AuthenticatedUser` | Requires an authenticated user. |
| `ProjectTemplate.Role.Administrator` | Requires a normalized role claim. |
| `ProjectTemplate.Permission.ManageApplication` | Requires a normalized permission claim. |

Default normalized claim types:

| Claim Type | Purpose |
|---|---|
| `application:role` | Role claim used by role-based policies. |
| `application:permission` | Permission claim used by permission-based policies. |

Configuration example:

```json
"ProjectTemplate": {
  "Authorization": {
    "RoleClaimType": "application:role",
    "PermissionClaimType": "application:permission",
    "AdministratorRoles": [
      "Administrator"
    ],
    "ManageApplicationPermissions": [
      "application.manage"
    ]
  }
}
```
Example usage:
```csharp
[Authorize(Policy = ApplicationAuthorizationPolicyNames.AdministratorRole)]
public IActionResult AdminOnly()
{
    return View();
}
```

## Data Access

### EF Core, SQLite, and Database Updates

The application includes an initial EF Core data access foundation using SQLite as the default local development provider.

SQLite is used as the default development provider because it is lightweight, file-based, and does not require a separate database server.

The SQLite connection string is configured in:

```text
src/ProjectTemplate.Web/appsettings.json
```
The default connection string uses a local SQLite database file:
```json
"ConnectionStrings": {
  "ApplicationDatabase": "Data Source=application-dev.db"
}
```
SQLite is used as the default development provider because it is lightweight, file-based, and does not require a separate database server.

EF Core migrations are stored in the infrastructure project because `ProjectTemplate.Infrastructure` owns the `ApplicationDbContext`, entities, and EF Core configuration.

The web project is used as the startup project because it provides application configuration, dependency injection, provider setup, and connection-string resolution.

### EF Core CLI Tool
The dotnet ef command requires the EF Core command-line tool.

Check whether the tool is available:
```bash
dotnet ef --version
```
If the command is not found, install or update the tool:
```bash
dotnet tool install --global dotnet-ef
```
Or update an existing global installation:
```bash
dotnet tool update --global dotnet-ef
```

### Add a Migration
Create a new migration from the repository root:
```bash
dotnet ef migrations add MigrationName `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output-dir Data/Migrations
```
Replace `MigrationName` with a descriptive name, such as:
```bash
dotnet ef migrations add AddExternalLoginAccounts `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output-dir Data/Migrations
```
### Update the Local Database
Apply pending migrations to the configured local SQLite database:
```bash
dotnet ef database update `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
```
This creates or updates the local SQLite database using the `ApplicationDatabase` connection string resolved by the startup project.

### Verify Pending Migrations
List available migrations:
```bash
dotnet ef migrations list `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
```
Generate a SQL script for review:
```bash
dotnet ef migrations script `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
  --output migration.sql
```
### Connection String Resolution
Migration commands use the startup project to resolve configuration.

For this application, that means the connection string comes from:
```bash
src/ProjectTemplate.Web/appsettings.json
src/ProjectTemplate.Web/appsettings.{Environment}.json
user secrets
environment variables
other configured providers
```
By default, the application resolves:
```bash
ConnectionStrings:ProjectTemplateDatabase
```
For local development, the default SQLite value is:
```bash
Data Source=application-dev.db
```
Applications can override this value through normal ASP.NET Core configuration sources.

For example, an environment variable can override the connection string:
```bash
ConnectionStrings__ApplicationDatabase=Data Source=custom-application-dev.db
```
### Automatic Startup Migrations

The application does not automatically run EF Core migrations during application startup.

This is intentional.

Automatic startup migration execution can be useful for small local development scenarios, but it can be unsafe in production because multiple application instances may start at the same time, schema changes may require review, and failed migrations can prevent the application from starting cleanly.

For now, database migration execution should remain an explicit developer or deployment action.

A future issue may add an opt-in startup migration feature for development-only or controlled hosting scenarios, but it should include clear safeguards before being used.

### Recommended Production Posture

Production database updates should be handled outside normal application startup.

Recommended options include:
- CI/CD pipeline migration steps.
- Reviewed SQL migration scripts.
- Manual DBA-approved migration execution.
- Dedicated deployment jobs that run before the application is released.
- Environment-specific connection strings supplied through deployment secrets or secure configuration.

Before applying migrations to production:
__1.__ Review the generated migration.
__2.__ Generate and inspect the SQL script.
__3.__ Back up the target database when appropriate.
__4.__ Apply the migration through a controlled deployment process.
__5.__ Confirm the application version and database schema are compatible.

Example script generation command:
```bash
dotnet ef migrations script `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --idempotent `
  --output migration.sql
```
The `--idempotent` option is useful for deployment scenarios where the target database may already have some migrations applied.

### SQLite Development Flow
A common local development flow is:
```bash
dotnet restore
dotnet build --configuration Release
dotnet ef database update `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
dotnet test --configuration Release
```
To recreate the local SQLite database from scratch, stop the application, delete the local `.db` file, and run `dotnet ef database update` again.

Only do this for disposable local development databases.

### Troubleshooting
If dotnet ef is not recognized, install or update the EF Core CLI tool:
```bash
dotnet tool install --global dotnet-ef
```
If the startup project cannot be built, run:
```bash
dotnet build --configuration Release
```
If the connection string cannot be found, confirm that `ConnectionStrings:ApplicationDatabase` exists in the startup project configuration.

If migrations are not discovered, confirm that the command uses:
```bash
--project src/ProjectTemplate.Infrastructure
--startup-project src/ProjectTemplate.Web
--context ApplicationDbContext
```
If SQLite provider configuration fails, confirm that the infrastructure project references the SQLite provider package and that the data access registration uses the configured ApplicationDatabase connection string.

Future database providers, such as SQL Server, can be added by extending the data access registration configuration.
SQLite remains the default development provider. SQL Server can be selected through configuration. Because EF Core migrations are provider-specific, production SQL Server deployments should generate and maintain SQL Server-compatible migrations before applying database updates.

### External Login Account Linking Persistence

The application includes an optional EF Core persistence model for applications that need to link external provider identities to local application users.

This is different from claims-only sign-in.

Claims-only sign-in uses the external provider claims from the current authentication session and does not require a local account-linking table. This is sufficient when the application only needs to authenticate the current request.

Local account linking is useful when an application needs to associate one or more external identities with a local application user profile, preserve account-link audit history, support provider migration, or allow multiple providers to sign in to the same local account.

The external login persistence model stores:

- Local user ID
- Provider name
- Provider user ID
- Display name
- Email
- Created, updated, and last-login timestamps

Provider tokens are not stored by default. Applications that need token persistence should add that behavior intentionally and review the security, encryption, rotation, and retention requirements before enabling it.

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

The application uses strongly typed options classes for application-owned configuration under the `ProjectTemplate` section.

Validated configuration areas include:

- `ProjectTemplate:SecurityHeaders`
- `ProjectTemplate:ForwardedHeaders`
- `ProjectTemplate:RateLimiting`

Invalid startup-sensitive values fail application startup rather than being silently corrected. This helps catch unsafe or malformed production configuration before the application begins serving requests.

## Application Packaging

This repository includes an initial `dotnet new` template scaffold.

The scaffold allows the repository to be installed locally as a project template and used to generate a new application from the current source structure.

### Install the Template Locally

From the repository root:

```bash
dotnet new install .\
```
On Linux or macOS:
```bash
dotnet new install ./
```
### Create a New Project from the Template
From a separate working directory:
```bash
dotnet new cavell-netcoreapp -n ContosoSecurityPortal
```
_For the initial template scaffold, use a project name that is also a valid C# identifier, such as `ContosoSecurityPortal`. Dotted project names require additional template symbol handling so namespace replacement and type-name replacement can be handled separately._

This creates a new project using ContosoSecurityPortal as the replacement name for the source template namespace and project prefix.
### Build the Generated Project
```bash
cd ContosoSecurityPortal
dotnet build
dotnet test
```


- .template.config setup.
- Template parameters.
- Project naming replacements.
- Optional modules.
- Authentication provider variants.
- Database provider variants.
- Local installation.
- Application testing.
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

The repository includes automated tests under `tests/ProjectTemplate.Web.Tests`.

The test project uses `WebApplicationFactory<Program>` to start the real `ProjectTemplate.Web`
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
This threshold is intentionally modest while the application is still growing. It should be raised over time as additional modules, data access features, authentication providers, and application packaging tests are added.


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
Add application README scaffold #<issue-number>
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
- [x]  Add EF Core with SQLite.
- [x]  Add SQL Server provider option.
- [x]  Add authentication module structure.
- [x]  Add OIDC support.
- [x]  Add SAML2 support.
- [x]  Add external provider support.
- [x]  Add template packaging.
- [x]  Add GitHub workflows.
- [x]  Add documentation.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).

