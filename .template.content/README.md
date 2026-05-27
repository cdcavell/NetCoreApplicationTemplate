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
- Baseline automated tests.

## Prerequisites

- .NET SDK 10.0 or later.
- Docker Desktop or a compatible container runtime, only if you plan to use Docker Compose.

## Template Options Used

This project was generated from the `netcoreapp-template` template.

The template supports a small set of scaffold options for common application variants:

| Option | Default | Supported values | Description |
|:---|:---|:---|:---|
| `authProvider` | `cookie` | `cookie`, `none` | Selects whether the generated application starts with the cookie-authentication-ready baseline or with application authentication disabled by default. |
| `dbProvider` | `sqlite` | `sqlite`, `sqlserver` | Selects the generated EF Core provider configuration. |

The generated application still includes the core production-oriented guardrails regardless of these options, including structured logging, centralized error handling, health checks, security headers, rate limiting, and safe defaults.

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

Do not commit `.env` files that contain local secrets, production connection strings, or machine-specific values.

## Health Probe Semantics

Use `/health/live` for process liveness and `/health/ready` for dependency-aware deployment readiness.

The baseline readiness endpoint intentionally includes only checks explicitly tagged for readiness. Add database, cache, or external dependency checks only when they should remove the instance from normal traffic rotation.

## Generated Content

This scaffold intentionally includes source projects, baseline tests, Docker support, license files, and third-party asset notices.

This scaffold intentionally excludes repository-maintainer files such as GitHub issue templates, release workflows, DocFX documentation source, ADRs, contribution policy, citation metadata, and release-management instructions.

## License

This generated project includes the template license in `LICENSE.txt`.

Third-party asset and notice information is included in `ASSETS-LICENSES.md`.

Review both files before publishing or redistributing an application created from this scaffold.
