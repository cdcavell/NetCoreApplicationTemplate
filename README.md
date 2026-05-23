# .NET Core Application Template 
[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)
[![Documentation](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://cdcavell.github.io/NetCoreApplicationTemplate/)
[![GitHub Release](https://img.shields.io/github/v/release/cdcavell/NetCoreApplicationTemplate?display_name=tag)](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/cdcavell/NetCoreApplicationTemplate)](LICENSE.txt)

A reusable, production-oriented .NET application template designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

This repository provides a working application baseline with common infrastructure concerns already organized, including middleware ordering, structured logging, forwarded headers, security headers, rate limiting, centralized error handling, authentication and authorization foundations, EF Core data access patterns, GitHub Actions validation, DocFX documentation, and future .NET template packaging support.

## Current Release

<!-- BEGIN LATEST_RELEASE -->
Current release: __[Release 0.4.1](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/tag/v0.4.1)__

Tag: `v0.4.1`
<!-- END LATEST_RELEASE -->

## Project Goals

The goal of this project is to provide a clean ASP.NET Core starting point that can be reused across future applications while keeping common application infrastructure consistent and easy to maintain.

The template focuses on:

- Production-oriented ASP.NET Core startup and middleware organization.
- Secure-by-default application configuration.
- Structured application and request logging.
- Centralized exception and status code handling.
- Browser and API-friendly error responses.
- Reverse proxy and forwarded header support.
- Baseline rate limiting and health checks.
- Authentication-ready and authorization-ready structure.
- EF Core data access patterns.
- Automated build, test, coverage, and documentation workflows.
- Future packaging as a reusable .NET project template.

## Quick Start

Clone the repository:

```powershell
git clone https://github.com/cdcavell/NetCoreApplicationTemplate.git
cd NetCoreApplicationTemplate
```
Restore dependencies:
```powershell
dotnet restore
```
Build the solution:
```powershell
dotnet build
```
Run tests:
```powershell
dotnet test
```
Run the web application:
```powershell
dotnet run --project src/ProjectTemplate.Web
```
## Documentation

Detailed documentation is maintained in the `docs` folder and published with DocFX.

- [Documentation source](docs/index.md)
- [Published documentation](https://cdcavell.github.io/NetCoreApplicationTemplate/)
- [Architecture Decision Records](https://cdcavell.github.io/NetCoreApplicationTemplate/adr/)
- Generated API reference is available through the published documentation navigation.

Documentation areas include:

- [Getting Started](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/getting-started.html)
- [Project Structure](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/project-structure.html)
- [Configuration](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/configuration.html)
- [Deployment](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/deployment.html)
- [Middleware Pipeline](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/middleware.html)
- [Logging](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/logging.html)
- [Error Handling](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/error-handling.html)
- [Security Headers](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/security-headers.html)
- [Forwarded Headers](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/forwarded-headers.html)
- [Rate Limiting](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/rate-limiting.html)
- [Health Checks](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/health-checks.html)
- [Telemetry](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/telemetry.html)
- [Authentication](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/authentication.html)
- [Authorization](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/authorization.html)
- [Data Access](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/data-access.html)
- [GitHub Workflow](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/github-workflow.html)
- [Template Packaging](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/template-packaging.html)

## Contributing

Contributor guidance is available in [CONTRIBUTING.md](CONTRIBUTING.md).

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
## Local Documentation Build
Restore local tools:
```powershell
dotnet tool restore
```
Build the DocFX site:
```powershell
dotnet tool run docfx -- docs/docfx.json
```
Serve the generated site locally:
```powershell
dotnet tool run docfx -- serve docs/_site
```

## GitHub Actions
The repository includes GitHub Actions workflows for:
- Restoring dependencies
- Building the solution
- Verifying formatting
- Running tests
- Generating coverage reports
- Running CodeQL analysis
- Building and publishing DocFX documentation
See [GitHub Workflow](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/github-workflow.html) for details.

## Versioning
This project follows Semantic Versioning using the format:
```
MAJOR.MINOR.PATCH
```
Version numbers are centrally managed through project build metadata so assemblies, future packages, and releases can share a consistent version identity.

## Roadmap
The project is being developed toward a reusable .NET application template. Future work may include additional provider modules, stronger packaging support, template parameterization, expanded examples, and release-ready template distribution.

See [Template Packaging](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/template-packaging.html) for the current packaging direction.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).
