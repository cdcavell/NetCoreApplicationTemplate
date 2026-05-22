# .NET Core Application Template 
[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)
[![Documentation](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://cdcavell.github.io/NetCoreApplicationTemplate/)
[![GitHub Release](https://img.shields.io/github/v/release/cdcavell/NetCoreApplicationTemplate?display_name=tag)](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/cdcavell/NetCoreApplicationTemplate)](LICENSE.txt)

A reusable, production-oriented .NET application application designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

This repository provides a working application baseline with common infrastructure concerns already organized, including middleware ordering, structured logging, forwarded headers, security headers, rate limiting, centralized error handling, authentication and authorization foundations, EF Core data access patterns, GitHub Actions validation, DocFX documentation, and future .NET template packaging support.

## Current Release

<!-- BEGIN LATEST_RELEASE -->
Current release: __[Release 0.4.0](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/tag/v0.4.0)__

Tag: `v0.4.0`
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

```bash
git clone https://github.com/cdcavell/NetCoreApplicationTemplate.git
cd NetCoreApplicationTemplate
```
Restore dependencies:
```bash
dotnet restore
```
Build the solution:
```bash
dotnet build
```
Run tests:
```bash
dotnet test
```
Run the web application:
```bash
dotnet run --project src/ProjectTemplate.Web
```
## Documentation
Detailed documentation is maintained in the `docs` folder and published with DocFX.
- [Documentation source](docs/index.md)
- [Published documentation](https://cdcavell.github.io/NetCoreApplicationTemplate/)
- [API reference](https://cdcavell.github.io/NetCoreApplicationTemplate/api/)

Documentation areas include:
- [Getting Started](docs/getting-started.md)
- [Project Structure](docs/project-structure.md)
- [Configuration](docs/configuration.md)
- [Middleware Pipeline](docs/middleware-pipeline.md)
- [Logging](docs/logging.md)
- [Error Handling](docs/error-handling.md)
- [Security Headers](docs/security-headers.md)
- [Forwarded Headers](docs/forwarded-headers.md)
- [Rate Limiting](docs/rate-limiting.md)
- [Health Checks](docs/health-checks.md)
- [Telemetry](docs/telemetry.md)
- [Authentication](docs/authentication.md)
- [Authorization](docs/authorization.md)
- [Data Access](docs/data-access.md)
- [GitHub Workflow](docs/github-workflow.md)
- [Template Packaging](docs/template-packaging.md)

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
```bash
dotnet tool restore
```
Build the DocFX site:
```bash
dotnet tool run docfx -- docs/docfx.json
```
Serve the generated site locally:
```bash
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
See [GitHub Workflow](docs/github-workflow.md) for details.

## Versioning
This project follows Semantic Versioning using the format:
```
MAJOR.MINOR.PATCH
```
Version numbers are centrally managed through project build metadata so assemblies, future packages, and releases can share a consistent version identity.

## Roadmap
The project is being developed toward a reusable .NET application template. Future work may include additional provider modules, stronger packaging support, template parameterization, expanded examples, and release-ready template distribution.

See [Template Packaging](docs/template-packaging.md) for the current packaging direction.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).

