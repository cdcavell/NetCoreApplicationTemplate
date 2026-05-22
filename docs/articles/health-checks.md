# Health Checks

The application includes baseline ASP.NET Core health check endpoints for local development, reverse proxy hosting, load balancers, container platforms, and future deployment scenarios.

Health checks are registered during service configuration:

```csharp
builder.Services.AddApplicationHealthChecks();
```
The endpoints are mapped during application startup:
```csharp
app.MapApplicationHealthChecks();
``` 
## Default Endpoints
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
## Reverse Proxy and Hosting Use

Health endpoints are intended for infrastructure-level checks from reverse proxies, load balancers, deployment platforms, and monitoring systems.

Typical uses include:

- Confirming the application process is running.
- Removing an unhealthy instance from load balancing rotation.
- Supporting future container or deployment probes.
- Providing a stable infrastructure path that avoids normal browser-facing error pages.

## Security Headers
The default security header configuration excludes `/health`:
```json
"ExcludedPathPrefixes": [
  "/health",
  "/metrics"
]
```
Because the exclusion is prefix-based, `/health`, `/health/ready`, and `/health/live` are all excluded from security header application. This keeps health probe responses small and infrastructure-friendly.
