# Public Surface v1.0

This article defines the public compatibility surface for the `v1.0.0` release of the .NET Core Application Template.

After `v1.0.0`, this document should be used with [ADR-0003](../adr/0003-record-release-surface-and-distribution-strategy.md), [Release Checklist](../../RELEASE.md), and the [v1.0 Upgrade Notes](v1-upgrade-notes.md) to decide whether a future change is SemVer-major, SemVer-minor, SemVer-patch, or internal-only.

## Purpose

The v1.0 public surface is the set of template behaviors, generated artifacts, configuration keys, routes, conventions, and distribution identifiers that consumers may reasonably depend on after the stable release.

A change is breaking when it causes an application generated from the previous stable version to require code, configuration, deployment, or usage changes that were not clearly optional.

## Package and Template Identity

The following identifiers are part of the v1.0 public surface.

| Surface | Committed value |
|:---|:---|
| NuGet package ID | `CDCavell.NetCoreApplicationTemplate` |
| Template identity | `CDCavell.NetCoreApplicationTemplate.CSharp` |
| Template group identity | `CDCavell.NetCoreApplicationTemplate` |
| Template name | `.NET Core Application Template` |
| `dotnet new` short name | `cdcavell-netcoreapp` |
| Source replacement name | `ProjectTemplate` |
| Preferred name directory | `true` |

Changing, removing, or repurposing these values after `v1.0.0` is a breaking change unless the old value remains supported through an intentional compatibility path.

## Template Symbols and Defaults

The following template symbols are part of the v1.0 public surface.

| Symbol | Type | Default | Contract |
|:---|:---|:---|:---|
| `skipRestore` | `bool` | `false` | Controls whether the post-create NuGet restore action runs. |

Breaking changes include renaming the symbol, removing it, inverting its meaning, changing its default in a behavior-changing way, or making generated output depend on it differently without a migration path.

Adding a new optional symbol with a safe default is normally a minor change.

## Generated Output Structure

The following generated structure is part of the v1.0 public surface.

```text
/
├── ProjectTemplate.slnx
├── src/
│   ├── ProjectTemplate.Web/
│   └── ProjectTemplate.Infrastructure/
├── tests/
│   └── ProjectTemplate.Web.Tests/
├── scripts/
├── Dockerfile
├── docker-compose.yml
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── .editorconfig
├── .env.example
├── .dockerignore
├── .gitattributes
├── .gitignore
├── ASSETS-LICENSES.md
└── LICENSE.txt
```

The generated project responsibilities are:

| Project | Contract |
|:---|:---|
| `ProjectTemplate.Web` | ASP.NET Core host, startup composition, middleware pipeline, authentication and authorization registration, endpoint mapping, Razor Pages, MVC controllers, health checks, request logging, error handling, and web-facing configuration. |
| `ProjectTemplate.Infrastructure` | Infrastructure and persistence concerns used by the web application, including EF Core data access and related implementation details. |
| `ProjectTemplate.Web.Tests` | Tests for startup, configuration, middleware, authentication, authorization, and runtime behavior. |

Breaking changes include removing, renaming, or relocating these projects, changing the generated solution entry point, or changing the expected namespace replacement behavior in a way that breaks generated applications.

Adding optional folders, examples, or additional projects without disrupting the existing structure is normally a minor change.

## Configuration Surface

The following root configuration sections are part of the v1.0 public surface.

|Section|Contract|
|:---|:---|
|`ConnectionStrings`|Contains named database connection strings used by the generated application.|
|`AllowedHosts`|Uses standard ASP.NET Core host filtering configuration.|
|`Serilog`|Defines default structured logging configuration.|
|`ProjectTemplate`|Contains application-owned template configuration.|

The following ProjectTemplate:* sections are part of the v1.0 public surface.

|Section|Contract|
|:---|:---|
|`ProjectTemplate:ForwardedHeaders`|Reverse proxy and forwarded header behavior.|
|`ProjectTemplate:SecurityHeaders`|Security header and CSP behavior.|
|`ProjectTemplate:RateLimiting`|Global, fixed-window, and concurrency rate-limiting options.|
|`ProjectTemplate:RequestLogging`|Correlation ID and request logging behavior.|
|`ProjectTemplate:OpenTelemetry`|Tracing, metrics, service metadata, and OTLP exporter settings.|
|`ProjectTemplate:Authentication`|Cookie authentication and external provider configuration.|
|`ProjectTemplate:ClaimsTransformation`|Claim normalization and provider mapping behavior.|
|`ProjectTemplate:Authorization`|Application role and permission claim conventions.|
|`ProjectTemplate:DataAccess`|Data provider, connection string name, and auditing defaults.|
|`ProjectTemplate:ApiVersioning`|API versioning defaults and supported readers.|

Breaking changes include renaming or removing documented keys, changing value types, changing defaults in a way that materially changes runtime behavior, or moving settings to a different section without compatibility support.

Adding optional keys with safe defaults is normally a minor change.

Correcting descriptions, examples, comments, or validation messages without behavior changes is normally a patch change.

## Route and Endpoint Conventions

The following endpoint conventions are part of the v1.0 public surface.

|Endpoint or convention|Contract|
|:---|:---|
|`/health`|General application health endpoint.|
|`/health/ready`|Readiness endpoint for dependency-aware checks.|
|`/health/live`|Lightweight liveness endpoint.|
|`/Home/Error/500`|Non-development unhandled exception route.|
|`/Home/Error/{statusCode}`|Browser-oriented HTTP status code error route.|
|`/api/v{version}/<resource>`|Canonical API versioning route convention.|
|`X-API-Version`|Supported secondary API version reader.|

Changing or removing these conventions after `v1.0.0` is breaking unless the previous convention continues to work or a clear migration path is provided.

Adding new endpoints or optional route forms while preserving existing routes is normally a minor change.

## Middleware Ordering Contract

The v1.0 middleware order is part of the public runtime contract because it affects proxy behavior, logging, error handling, security headers, routing, CORS, rate limiting, authentication, authorization, and endpoint mapping.

The committed order is:

&emsp;**1.** Forwarded headers<br />
&emsp;**2.** Structured request logging<br />
&emsp;**3.** Centralized error handling<br />
&emsp;**4.** Problem Details handling<br />
&emsp;**5.** Security headers<br />
&emsp;**6.** HTTPS redirection<br />
&emsp;**7.** Static files<br />
&emsp;**8.** Routing<br />
&emsp;**9.** CORS<br />
&emsp;**10.** Rate limiting<br />
&emsp;**11.** Authentication<br />
&emsp;**12.** Authorization<br />
&emsp;**13.** Controller and Razor Page endpoint mapping

Reordering, removing, or replacing middleware in a way that changes documented runtime behavior is breaking.

Adding optional middleware or documented extension points without disrupting the order is normally a minor change.

Fixing narrow bugs while preserving the documented order is normally a patch change.

## Container and Publishing Surface

The repository-maintained container image is part of the release evidence and distribution surface, but generated consumer applications may choose their own image names, registries, and deployment process.

The documented repository image is:
```
ghcr.io/cdcavell/netcoreapplicationtemplate
```

Stable semantic version tags publish:
```
ghcr.io/cdcavell/netcoreapplicationtemplate:<major>.<minor>.<patch>
ghcr.io/cdcavell/netcoreapplicationtemplate:<major>
ghcr.io/cdcavell/netcoreapplicationtemplate:latest
```

Prerelease tags publish the full version tag only and do not update `latest` or the major tag.

The documented local development image tag is:
```
projecttemplate-web:dev
```

The container listens on port `8080`.

Recommended probe paths are:
```
/health/live
/health/ready
```

Breaking changes include renaming the documented repository image, removing documented stable tag forms, changing the default container port, or changing the health probe contract without a compatibility path.

Adding new image variants, additional tags, or additional examples while preserving the existing contract is normally a minor change.

## Internal-Only Changes

The following changes are normally internal-only when they do not affect generated consumer behavior or documented extension points:

- Refactoring private implementation details.
- Reorganizing repository-only documentation that is not included in generated output.
- Updating CI implementation details without changing release outputs.
- Improving tests without changing generated behavior.
- Updating comments or formatting.
- Rebuilding against compatible patch-level dependencies.
- Improving maintainability without changing documented keys, routes, defaults, generated structure, or package identity.

## Breaking Change Test

When classification is ambiguous, ask:

> Would an application generated from the previous stable version break, require reconfiguration, require a deployment change, or require consumers to learn a new documented usage pattern?

f yes, the change is probably SemVer-major unless the old behavior remains supported.

If the change only adds optional capability, it is probably SemVer-minor.

If the change corrects behavior without changing the documented contract, it is probably SemVer-patch.

If the change affects only repository internals and not generated or documented consumer behavior, it is internal-only.

## Related Documents
- [ADR-0003: Record Release Surface and Distribution Strategy](../adr/0003-record-release-surface-and-distribution-strategy.md)
- [v1.0 Upgrade Notes](v1-upgrade-notes.md)
- [Template Packaging](template-packaging.md)
- [Project Structure](project-structure.md)
- [Configuration](configuration.md)
- [Middleware Pipeline](middleware.md)
- [Health Checks](health-checks.md)
- [Error Handling](error-handling.md)
- [Container Release Publishing](container-publish.md)
