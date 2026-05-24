# ProjectTemplate

ProjectTemplate is a scaffolded ASP.NET Core application created from the `cdcavell-netcoreapp` template.

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

## Generated Content

This scaffold intentionally includes source projects, baseline tests, Docker support, license files, and third-party asset notices.

This scaffold intentionally excludes repository-maintainer files such as GitHub issue templates, release workflows, DocFX documentation source, ADRs, contribution policy, citation metadata, and release-management instructions.

## License

This generated project includes the template license in `LICENSE.txt`.

Third-party asset and notice information is included in `ASSETS-LICENSES.md`.

Review both files before publishing or redistributing an application created from this scaffold.
