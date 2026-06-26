# Runtime Readiness Baseline

This page summarizes the runtime foundations that are considered part of the current stable readiness baseline. It is intended as a review map across observability, error handling, authentication, authorization, and request protection.

## Baseline Registration

The application registers the runtime baseline during startup in `Program.cs`:

- Serilog and structured request logging.
- Forwarded headers.
- Security headers.
- Rate limiting.
- OpenTelemetry tracing and metrics.
- Centralized Problem Details.
- Authentication and authorization.
- EF Core-ready data access.

The middleware pipeline applies forwarded headers early, then request logging, centralized error handling, Problem Details, security headers, HTTPS redirection, static files, routing, CORS, rate limiting, authentication, authorization, controllers, and Razor Pages.

## Observability Baseline

OpenTelemetry is included as the tracing and metrics baseline.

Default behavior:

- `ProjectTemplate:OpenTelemetry:Enabled` is `true`.
- Tracing is enabled.
- Metrics are enabled.
- ASP.NET Core instrumentation is enabled.
- HTTP client instrumentation is enabled.
- OTLP export is disabled until an endpoint is configured.

The template does not map a direct `/metrics` scraping endpoint by default. Prometheus scraping should be added through an OpenTelemetry collector, a Prometheus-compatible exporter, or a deliberate application-specific endpoint. If a direct scraping endpoint is added later, it should be reviewed with security headers, request logging exclusions, authentication, and deployment network controls.

## W3C Trace Context

The template uses ASP.NET Core and OpenTelemetry defaults for request activity creation and propagation. It does not replace the default propagator or force a custom trace identifier format.

Problem Details responses include `traceId` from `Activity.Current?.Id` when available, falling back to the ASP.NET Core request trace identifier. This preserves the connection between incoming trace context, server-side logs, and API error responses when tracing is active.

## Structured Logging Baseline

Serilog is the structured logging provider.

The default production-oriented log-level posture is:

| Source | Default level |
|:---|:---|
| Application | `Information` |
| `Microsoft` | `Warning` |
| `Microsoft.AspNetCore` | `Warning` |
| `Microsoft.AspNetCore.Hosting` | `Warning` |
| `Microsoft.AspNetCore.Mvc` | `Warning` |
| `Microsoft.AspNetCore.Routing` | `Warning` |
| `System` | `Warning` |

Console and rolling file sinks use templates that include timestamp, level, source context, request ID, request path, and exception details when present. Request logging adds correlation ID, scheme, host, remote IP address when enabled, authenticated user name when enabled, elapsed time, and response status.

Sensitive values such as credentials, cookies, authentication tokens, request bodies, and response bodies are not logged by default.

## Problem Details Baseline

Centralized Problem Details support is registered through `AddApplicationProblemDetails` and applied in the pipeline through `UseProblemDetails`.

For API, AJAX, and JSON-oriented requests, the application returns safe Problem Details responses. These responses include status, title, instance path, trace ID, and request ID.

In Development, detailed exception messages may be included to support local troubleshooting. Outside Development, server errors use a generic detail message and do not expose stack traces, exception messages, connection details, secret values, or internal implementation details in the response.

Browser-oriented requests use the shared error-page flow instead of JSON Problem Details.

## Authentication Baseline

Application authentication is enabled by default with local cookie authentication.

Default behavior:

- `ProjectTemplate:Authentication:Enabled` is `true`.
- The default authenticate, challenge, and sign-in schemes use `Cookies`.
- Local cookie authentication is enabled.
- External OpenID Connect, SAML2, Microsoft, Google, and GitHub providers are disabled until explicitly configured.
- Enabled external providers are validated during startup and fail fast when required values are missing.

External provider secrets, certificates, client secrets, tokens, and real identity-provider metadata must be supplied through user secrets, environment variables, deployment secrets, or an approved secret store.

## Authorization Baseline

The application includes named baseline authorization policies for authenticated users, administrator role checks, and manage-application permission checks.

| Policy | Purpose |
|:---|:---|
| `application.AuthenticatedUser` | Requires an authenticated user. |
| `application.Role.Administrator` | Requires an authenticated user with a configured administrator role claim. |
| `application.Permission.ManageApplication` | Requires an authenticated user with a configured manage-application permission claim. |

The default role claim type is `application:role`, and the default permission claim type is `application:permission`.

## Rate Limiting Baseline

Rate limiting is enabled and configurable by default.

Default behavior:

- A global fixed-window limiter is available for baseline request protection.
- A named fixed-window policy is available for endpoint-specific request protection.
- A named concurrency policy is available for sensitive or resource-heavy operations.
- Rejected requests return HTTP `429 Too Many Requests`.
- Rejections are logged.

Default limits are intentionally conservative starter values. Production applications should tune limits based on traffic profile, endpoint sensitivity, reverse proxy behavior, authenticated versus anonymous traffic, and whether an endpoint performs expensive I/O or external service calls.

If rate limiting needs to partition by authenticated user, tenant, API key, or role, review middleware ordering and move rate limiting after authentication only when the policy requires authenticated identity information.

## Follow-up Gap Review

No new implementation gaps were identified during this documentation pass. Remaining production decisions are deployment-specific tuning items rather than template blockers:

- OTLP collector and metrics exporter selection.
- Prometheus scrape strategy, if required.
- Production log sink selection and retention policy.
- Environment-specific authentication provider configuration.
- Production rate-limit values and partitioning strategy.

If any of these become template-level requirements, they should be split into dedicated implementation issues rather than being folded into release checklist work.
