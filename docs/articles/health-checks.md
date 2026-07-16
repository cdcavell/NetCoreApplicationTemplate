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

| Endpoint | Purpose |
|:---------|:--------|
| `/health` | General application health endpoint. |
| `/health/ready` | Readiness endpoint intended for dependency-aware checks such as database, cache, or external service availability. |
| `/health/live` | Liveness endpoint intended to verify that the application process can respond. |

The baseline application provides the readiness endpoint shape. It does not, by itself, prove database, cache, queue, or external service availability.

Each generated application must register the dependency checks that define production readiness for that service. Future modules, such as EF Core, SQL Server, authentication providers, or external integrations, can add tagged checks for readiness scenarios.

## Access and Deployment Boundary

All three health endpoints are mapped with `.AllowAnonymous()` intentionally. This keeps container, reverse-proxy, load-balancer, and orchestration probes independent of browser login state and prevents the authenticated fallback policy from turning a failed probe into an authentication redirect.

Anonymous application access does not imply unrestricted Internet exposure. Production deployments should restrict health endpoint reachability through the deployment boundary appropriate to the environment, such as:

- Private ingress or internal load-balancer listeners.
- Firewall, network security group, or service-mesh policy.
- Reverse-proxy path restrictions.
- Monitoring-system source restrictions.

Avoid returning secrets, configuration values, dependency connection details, exception messages, or other sensitive diagnostics from health responses. Applications that require authenticated health diagnostics should add a separate protected diagnostics endpoint rather than changing the lightweight liveness contract accidentally.

## Liveness Semantics

`/health/live` should stay lightweight. It is intended to answer one question:

```text
Can the application process respond to requests?
```

Do not add database, cache, authentication provider, external HTTP service, queue, or storage checks to liveness. A temporary downstream dependency failure should not normally cause an orchestrator to restart an otherwise healthy application process.

## Readiness Semantics

`/health/ready` is the place for deployment-readiness checks.

A readiness check should answer:

```text
Should this application instance receive normal traffic?
```

Only checks tagged `ready` are included in the readiness endpoint. This keeps dependency-aware readiness separate from process liveness.

Example future readiness check:

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: new[] { "ready" });
```

Use readiness for dependencies that should remove an instance from rotation when unavailable, such as required database connectivity or a required local cache. Avoid adding optional integrations unless the application cannot serve useful traffic without them.

## Container and Hosting Probe Contract

The Dockerfile intentionally delegates active HTTP health probing to Docker Compose, Kubernetes, load balancers, or hosting infrastructure instead of adding probe-only tools to the runtime image.

Recommended probe paths:

```text
/health/live
/health/ready
```

A container platform or reverse proxy can use `/health/live` for liveness and `/health/ready` for readiness. See [Docker Development Workflow](docker.md) and [Container Release Publishing](container-publish.md) for container-specific guidance.

## Reverse Proxy and Hosting Use

Health endpoints are intended for infrastructure-level checks from reverse proxies, load balancers, deployment platforms, and monitoring systems.

Typical uses include:

- Confirming the application process is running.
- Removing an unhealthy or not-ready instance from load balancing rotation.
- Supporting container or deployment probes.
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
