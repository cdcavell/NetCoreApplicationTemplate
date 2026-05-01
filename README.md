# .NET Core Application Template

A reusable, production-oriented .NET application template designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

This project is intended to serve as a starting point for applications that need a consistent foundation for middleware ordering, logging, security headers, proxy support, rate limiting, centralized error handling, authentication integration, data access, and future template packaging.

## Project Goals

The purpose of this template is to provide a clean enterprise-ready baseline that can be reused across multiple applications while keeping common infrastructure concerns consistent and easy to maintain.

Primary goals include:

- Production-ready ASP.NET Core middleware configuration.
- Centralized application startup and pipeline organization.
- Serilog-based structured logging.
- Forwarded headers and reverse proxy support.
- Security header middleware.
- Rate limiting policies.
- Centralized exception and status code handling.
- EF Core data access patterns.
- SQLite support for development.
- SQL Server support through pluggable configuration.
- Authentication-ready architecture.
- Support for OIDC, SAML2, Microsoft, Google, and other external providers.
- Future packaging as a reusable .NET project template.
- Local Git development with GitHub remote repository support.

## Current Status

Initial repository and folder structure are in place.

Development has not yet started. This README will evolve as each area of the project is implemented.

## Repository Structure

```text
/
├── src/
│   └── Application source projects
│
├── tests/
│   └── Automated tests
│
├── docs/
│   └── Project documentation
│
├── templates/
│   └── Future .NET template packaging files
│
├── scripts/
│   └── Utility scripts for setup, build, or maintenance
│
├── .github/
│   └── GitHub workflows, issue templates, and repository metadata
│
├── .gitattributes
├── .gitignore
├── README.md
├── LICENSE.txt
└── ASSETS-LICENSES.md
```

## Application Architecture

This section will describe the high-level architecture of the template application.

Planned areas:

- Application startup structure.
- Dependency injection organization.
- Extension method conventions.
- Middleware composition.
- Environment-specific behavior.
- Configuration layering.
- Modular service registration.
- Separation of infrastructure, application, and web concerns.

## Middleware Pipeline

This section will document the standard middleware order used by the template.

Planned areas:

- Forwarded headers.
- Exception handling.
- HTTPS redirection.
- Static files.
- Routing.
- Security headers.
- CORS, if applicable.
- Authentication.
- Authorization.
- Rate limiting.
- Response caching, if applicable.
- Endpoint mapping.
- Status code handling.

## Logging

This template uses Serilog for structured application logging.

Serilog is configured as the primary logging provider so that application events, startup events, errors, and HTTP request activity are written using a consistent structured format.

#### Bootstrap Logging
The application logs a bootstrap message when the web application begins provider configuration:
```csharp
Log.Information("Bootstrapping Template.Web application");
```

#### Startup Logging
The application logs a startup message when the web application begins initialization:
```csharp
Log.Information("Starting Template.Web application");
```

#### Pipline Logging 
The application logs a startup message when the web application begins configuring the middleware pipeline:
```csharp
Log.Information("Configuring pipline for Template.Web application");
```

#### Runtime Logging
The application logs a startup message when the web application begins running:
```csharp
Log.Information("Running Template.Web application");
```

#### Ongoing Application Logging
While the application is running, structured logs are written for normal application activity, warnings, errors, and HTTP request processing. These logs help provide visibility into the current behavior of the application without requiring a debugger to be attached.

Runtime logging may include:

- Application lifecycle events
- Controller or endpoint activity
- HTTP request completion details
- Warnings from expected but noteworthy conditions
- Exceptions and unexpected failures
- Framework or infrastructure messages based on configured log levels

The default logging configuration is intended to capture useful operational information while avoiding sensitive data such as passwords, authentication tokens, cookies, request bodies, and response bodies.

Additional logging can be added throughout the application by injecting ILogger<T> into services, controllers, middleware, or other application components.

#### Bootstrap Exception Logging
The application logs any exceptions that occur during the bootstrapping process or while configuring the middleware pipeline:
```csharp
Log.Fatal(ex, "Template.Web application terminated unexpectedly");
```


## Error Handling

This section will document centralized exception handling and status code behavior.

Planned areas:

- Global exception handling.
- Status code pages.
- Error controller or endpoint strategy.
- Problem Details support.
- User-safe error responses.
- Developer exception pages for local development.
- Logging behavior for handled and unhandled errors.

## Security Headers

This section will document the security header middleware used by the template.

Planned areas:

- Content-Security-Policy.
- X-Content-Type-Options.
- X-Frame-Options or frame-ancestors.
- Referrer-Policy.
- Permissions-Policy.
- Strict-Transport-Security.
- Cache-related headers.
- Environment-specific header behavior.

## Forwarded Headers and Proxy Support

This section will document reverse proxy and load balancer support.

Planned areas:

- X-Forwarded-For.
- X-Forwarded-Proto.
- X-Forwarded-Host.
- Known proxies.
- Known networks.
- Internal network considerations.
- HTTPS scheme correction behind proxy infrastructure.
- Logging original client IP addresses.

## Rate Limiting

This section will document rate limiting policies used by the template.

Planned areas:

- Global rate limiting.
- Endpoint-specific policies.
- Fixed window policies.
- Concurrency policies.
- Authentication-sensitive endpoint protection.
- Rejection responses.
- Logging and monitoring rejected requests.

## Authentication and Authorization

This section will document authentication and authorization architecture.

Planned areas:

- Cookie authentication.
- OpenID Connect.
- SAML2.
- Microsoft authentication.
- Google authentication.
- Social provider modules.
- External login configuration.
- Claims transformation.
- Authorization policies.
- Role and permission patterns.
- Pluggable authentication module design.

## Data Access

This section will document EF Core and database patterns.

Planned areas:

- DbContext registration.
- SQLite development configuration.
- SQL Server production configuration.
- Connection string management.
- Migrations.
- Repository or service patterns, if used.
- Transaction handling.
- Retry strategies.
- Concurrency handling.
- Auditing fields.
- Soft delete patterns, if applicable.

## Configuration

This section will document application configuration strategy.

Planned areas:

- appsettings.json.
- appsettings.Development.json.
- User secrets.
- Environment variables.
- Provider-specific configuration.
- Options pattern.
- Strongly typed settings.
- Validation on startup.
- Sensitive configuration handling.

## Template Packaging

This section will document how the project will eventually be packaged as a reusable template.

Planned areas:

- .template.config setup.
- Template parameters.
- Project naming replacements.
- Optional modules.
- Authentication provider variants.
- Database provider variants.
- Local installation.
- Template testing.
- Publishing strategy.

## Development Environment

This section will document local development requirements.

Planned tools:

- Visual Studio 2026.
- .NET SDK.
- Git.
- GitHub remote repository.
- SQLite for local development.
- SQL Server support for production-style configuration.

## Getting Started

Development instructions will be added as the project implementation begins.

Planned setup steps:

- Clone the repository.
- Restore NuGet packages.
- Configure local settings.
- Apply database migrations.
- Run the application.
- Execute tests.

## Build and Test

Build and test instructions will be added once the solution structure is finalized

Planned commands:

```
dotnet restore
dotnet build
dotnet test
```

## Git Workflow

This project uses Git for local source control with a remote repository hosted on GitHub.

Recommended branch naming:

```
main
feature/<description>
fix/<description>
docs/<description>
refactor/<description>
```

Recommended commit style:

```
Add initial repository attributes
Add template README scaffold
Implement security header middleware
Configure Serilog request logging
Add EF Core SQLite provider
```

## Documentation

Additional documentation will be added under the docs/ folder as the project grows.

Planned documentation:

- Architecture overview.
- Middleware pipeline notes.
- Security configuration.
- Authentication provider setup.
- Database provider setup.
- Template packaging guide.
- Deployment notes.

## Roadmap

Initial planned milestones:

- [x]  Add repository metadata files.
- [x]  Create initial solution structure.
- [ ]  Add baseline ASP.NET Core web application.
- [ ]  Add centralized service registration.
- [ ]  Add production middleware pipeline.
- [x]  Add Serilog logging.
- [ ]  Add security headers.
- [ ]  Add forwarded headers support.
- [ ]  Add rate limiting policies.
- [ ]  Add centralized error handling.
- [ ]  Add EF Core with SQLite.
- [ ]  Add SQL Server provider option.
- [ ]  Add authentication module structure.
- [ ]  Add OIDC support.
- [ ]  Add SAML2 support.
- [ ]  Add external provider support.
- [ ]  Add template packaging.
- [ ]  Add GitHub workflows.
- [ ]  Add documentation.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).

