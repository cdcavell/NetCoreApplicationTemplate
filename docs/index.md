# .NET Core Application Template Documentation

Welcome to the documentation for the .NET Core Application Template.

This documentation describes the reusable ASP.NET Core application baseline provided by the repository. It is intended for developers who want to understand the project structure, local development workflow, configuration model, middleware pipeline, security posture, authentication options, data access setup, template packaging, and v1.0 documentation surface.

The template is designed as a production-oriented starting point for ASP.NET Core applications that need consistent defaults for:

- Application startup and middleware ordering
- Serilog structured logging
- Forwarded headers and reverse proxy support
- Security headers
- Rate limiting
- Centralized exception and status code handling
- Problem Details responses
- Health checks
- OpenTelemetry tracing and metrics
- Authentication and authorization modules
- EF Core data access patterns
- GitHub Actions validation
- Package-based `dotnet new` template scaffolding

Use this documentation as the detailed reference. The root `README.md` provides the project summary and quick-start information.

## Documentation Areas

- [Getting Started](articles/getting-started.md)
- __v1.0 Readiness__
  - [v1.0 Migration Guide](articles/v1-migration-guide.md)
  - [Public Surface v1.0](articles/public-surface-v1.md)
  - [Production Deployment Checklist](articles/production-deployment-checklist.md)
  - [Runtime Readiness](articles/runtime-readiness.md)
  - [Build Quality and Reproducibility](articles/build-quality.md)
  - [Container Release Publishing](articles/container-publish.md)
  - [Template Packaging](articles/template-packaging.md)
- __Application Basics__
  - [Project Structure](articles/project-structure.md)
  - [Configuration](articles/configuration.md)
  - [Deployment Notes](articles/deployment.md)
  - [Docker Development](articles/docker.md)
- __Middleware Pipeline__
  - [Middleware Pipeline](articles/middleware.md)
  - [Error Handling](articles/error-handling.md)
  - [Security Headers](articles/security-headers.md)
  - [Forwarded Headers](articles/forwarded-headers.md)
  - [Rate Limiting](articles/rate-limiting.md)
  - [Health Checks](articles/health-checks.md)
- [API Versioning](articles/api-versioning.md)
- __Observability__
  - [Logging](articles/logging.md)
  - [Telemetry](articles/telemetry.md)
- __Authentication and Authorization__
  - [Authentication](articles/authentication.md)
  - [Authorization](articles/authorization.md)
- [Data Access](articles/data-access.md)
- [GitHub Workflow](articles/github-workflow.md)
- [Test Coverage](https://cdcavell.github.io/NetCoreApplicationTemplate/coverage/index.html)
