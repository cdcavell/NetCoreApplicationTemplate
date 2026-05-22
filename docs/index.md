# .NET Core Application Template Documentation

Welcome to the documentation for the .NET Core Application Template.

This documentation describes the reusable ASP.NET Core application baseline provided by the repository. It is intended for developers who want to understand the project structure, local development workflow, configuration model, middleware pipeline, security posture, authentication options, data access setup, and future template packaging direction.

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
- Future packaging as a reusable .NET template

Use this documentation as the detailed reference. The root `README.md` provides the project summary and quick-start information.
## Documentation Areas

- [__Getting Started__](articles/getting-started.md)
- [__Project Structure__](articles/project-structure.md)
- [__Configuration__](articles/configuration.md)
- __Middleware__
  - [Middleware Pipeline](articles/middleware.md)
  - [Error Handling](articles/error-handling.md)
  - [Security Headers](articles/security-headers.md)
  - [Forwarded Headers](articles/forwarded-headers.md)
  - [Rate Limiting](articles/rate-limiting.md)
  - [Health Checks](articles/health-checks.md)
- __Observability__
  - [Logging](articles/logging.md)
  - [Telemetry](articles/telemetry.md)
- __Authentication and Authorization__
  - [Authentication](articles/authentication.md)
  - [Authorization](articles/authorization.md)
- [__Data Access__](articles/data-access.md)
- [__GitHub Workflow__](articles/github-workflow.md)
- [__Template Packaging__](articles/template-packaging.md)
