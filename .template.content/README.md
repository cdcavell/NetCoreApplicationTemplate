# ProjectTemplate

ProjectTemplate is a scaffolded ASP.NET Core application created from the `netcoreapp-template` template.

The generated solution includes a production-oriented baseline for common web application infrastructure concerns:

- ASP.NET Core web application structure.
- Centralized middleware organization.
- Structured Serilog logging.
- Security headers.
- Forwarded headers and reverse proxy support.
- Rate limiting.
- Centralized exception and status-code handling.
- Problem Details responses.
- Authentication-ready and authorization-ready structure.
- EF Core-ready data access structure.
- Health checks.
- Docker and Docker Compose support.
- Baseline automated tests.

## Prerequisites

- .NET SDK 10.0 or later.
- Docker Desktop or a compatible container runtime, only if you plan to use Docker Compose.

The generated project includes `global.json` so SDK selection is consistent across local development and CI validation.

## Template Options Used

This project was generated from the `netcoreapp-template` template.

The template supports these scaffold options:

| Option | Default | Supported values | Description |
|:---|:---|:---|:---|
| `--authProvider` | `cookie` | `cookie`, `none` | Selects whether the generated application starts with the cookie-authentication-ready baseline or with application authentication disabled by default. |
| `--dbProvider` | `sqlite` | `sqlite`, `sqlserver`, `none` | Selects the generated data access mode. |
| `--skipRestore` | `false` | `true`, `false` | Skips the post-create NuGet restore action when set to `true`. |

The generated application still includes the core production-oriented guardrails regardless of these options, including structured logging, centralized error handling, health checks, security headers, rate limiting, and safe defaults.

### Default scaffold

The default scaffold uses cookie-authentication-ready configuration and SQLite development data access:

```powershell
dotnet new netcoreapp-template --name ProjectTemplate
```

Equivalent explicit options:

```powershell
dotnet new netcoreapp-template `
  --name ProjectTemplate `
  --authProvider cookie `
  --dbProvider sqlite
```

### Authentication-disabled SQL Server scaffold

A common non-default scaffold disables application authentication by default and selects SQL Server configuration:

```powershell
dotnet new netcoreapp-template `
  --name ProjectTemplate `
  --authProvider none `
  --dbProvider sqlserver
```

When `--authProvider none` is used, application authentication and cookie authentication are disabled in the generated `appsettings.json`.

This is intended for applications that do not need local cookie authentication at scaffold time, or that plan to add a different authentication approach later.

Authentication and authorization tests that intentionally exercise protected endpoints may need to enable test authentication through in-memory test configuration.

When `--dbProvider sqlserver` is used, the generated data access configuration selects `SqlServer` and `ApplicationSqlServer`. The generated `ConnectionStrings:ApplicationSqlServer` value is a local development example and should be replaced through environment-specific configuration before production use.

When `--dbProvider none` is used, the generated data access configuration selects `None`. EF Core application data access services are not registered, and no data access connection string is required unless the consuming application adds its own persistence strategy.

## Restore

```powershell
dotnet restore
```

## Build

```powershell
dotnet build --configuration Release
```

## Test

```powershell
dotnet test --configuration Release
```

## Run Locally

```powershell
dotnet run --project src/ProjectTemplate.Web
```

The local development profile may choose an available HTTPS or HTTP port depending on launch settings and environment. Docker Compose uses the fixed local port documented below.

## Run with Docker Compose

```powershell
docker compose up --build
```

The Docker-hosted application is available at:

```text
http://localhost:8080
```

Health endpoints are available at:

```text
http://localhost:8080/health
http://localhost:8080/health/ready
http://localhost:8080/health/live
```

## Docker Environment Overrides

The generated scaffold includes `.env.example` for optional local Docker Compose overrides.

To customize local Compose values:

```powershell
Copy-Item .env.example .env
```

Do not commit `.env` files that contain local or production environment-specific values.

## Configuration Notes

The generated `appsettings.json` includes baseline configuration for:

- Forwarded headers.
- Security headers.
- Rate limiting.
- Request logging.
- OpenTelemetry.
- Authentication and external provider placeholders.
- Claims transformation.
- Authorization.
- Data access.
- API versioning.

### Authentication

The default scaffold enables the application authentication baseline with cookie authentication enabled. External providers such as OpenID Connect, SAML2, Microsoft, Google, and GitHub are present as disabled placeholders.

Enable external providers intentionally and store provider-specific values outside committed configuration.

### Data access

The default scaffold selects SQLite with `ConnectionStrings:ApplicationDatabase`.

SQL Server scaffolds select `ConnectionStrings:ApplicationSqlServer`.

The EF Core model includes baseline optimistic concurrency support for entities that inherit from the shared data entity base type. Concurrency conflicts are surfaced through EF Core rather than silently overwriting stale updates.

The scaffold does not run EF Core migrations automatically during startup. Apply migrations intentionally as part of local setup or deployment.

When data access is disabled with `--dbProvider none`, EF Core services are not registered and migration commands are not applicable unless the consuming application adds its own persistence layer.

### Health checks

Use `/health/live` for process liveness and `/health/ready` for dependency-aware deployment readiness.

The baseline readiness endpoint intentionally includes only checks explicitly tagged for readiness. Add database, cache, or external dependency checks only when they should remove the instance from normal traffic rotation.

## Generated Content

This scaffold intentionally includes:

- Solution and project files.
- Source projects under `src/`.
- Baseline tests under `tests/`.
- Docker support files.
- Local configuration examples.
- `LICENSE.txt`.
- `ASSETS-LICENSES.md`.
- This consumer-oriented `README.md`.

This scaffold intentionally excludes repository-maintainer files such as GitHub issue templates, release workflows, DocFX documentation source, ADRs, contribution policy, security policy, package README, changelog, release-management instructions, and repository badges.

## Upstream References

The upstream template package and documentation can be used for deeper implementation guidance:

- NuGet package: `CDCavell.NetCoreApplicationTemplate`
- GitHub repository: `cdcavell/NetCoreApplicationTemplate`
- Published documentation: `https://cdcavell.github.io/NetCoreApplicationTemplate/`

## License

This generated project includes the template license in `LICENSE.txt`.

Third-party asset and notice information is included in `ASSETS-LICENSES.md`.

Review both files before publishing or redistributing an application created from this scaffold.
