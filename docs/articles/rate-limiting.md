# Rate Limiting

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

## Default Behavior

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

## Configuration

Rate limiting values can be configured from `appsettings.json`:

```json
"ProjectTemplate": {
  "RateLimiting": {
    "Enabled": true,
    "UseGlobalLimiter": true,
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

## Endpoint-Specific Policies

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

## Production Tuning Guidance

Before production use, review the configured limits against expected traffic and endpoint cost.

Consider:

- Anonymous versus authenticated traffic volume.
- Reverse proxy and load balancer behavior.
- Whether client IP, user identity, tenant ID, or API key should define the partition.
- Login, registration, export, report, file upload, and external-service endpoints that may need stricter limits.
- Queue behavior. The template defaults to `QueueLimit: 0` so rejected requests fail quickly instead of building server-side backlog.
- Whether upstream infrastructure such as a CDN, ingress controller, API gateway, or web application firewall already applies additional limits.

If a policy needs authenticated user, tenant, role, or permission data, review middleware ordering. The template currently places rate limiting before authentication so default protections can reject requests before authentication work is performed.

## Middleware Order

The application pipeline applies rate limiting after routing:

```csharp
app.UseRouting();

app.UseCors();

app.UseRateLimiter();
```

If a future policy depends on the authenticated user identity, rate limiting may need to move after authentication so user-specific partitioning can be applied.

## Automated Test Strategy

Rate limiting behavior is covered by integration tests under `tests/ProjectTemplate.Web.Tests`.

The tests use `WebApplicationFactory<Program>` to boot the real `ProjectTemplate.Web` pipeline in memory, override rate limiting configuration with in-memory settings, and register a test-only MVC controller from the test assembly.

This keeps production endpoints unchanged while allowing the tests to verify:

- Global fixed-window limiter behavior.
- Named fixed-window policy behavior.
- Named concurrency policy behavior.
- JSON `429 Too Many Requests` rejection responses.
- Disabled rate limiting behavior.
- Configuration binding for application rate limiting options.

See [Runtime Readiness Baseline](runtime-readiness.md) for the consolidated release-readiness view.