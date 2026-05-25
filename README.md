# .NET Core Application Template 
[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)
[![Coverage Gate](https://img.shields.io/badge/coverage%20gate-60%25-brightgreen)](#github-actions)
[![Documentation](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://cdcavell.github.io/NetCoreApplicationTemplate/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/cdcavell/NetCoreApplicationTemplate)](LICENSE.txt)
[![GitHub Release](https://img.shields.io/github/v/release/cdcavell/NetCoreApplicationTemplate?display_name=tag)](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/CDCavell.NetCoreApplicationTemplate?label=NuGet)](https://www.nuget.org/packages/CDCavell.NetCoreApplicationTemplate)
[![DOI](https://zenodo.org/badge/DOI/10.5281/zenodo.20373042.svg)](https://doi.org/10.5281/zenodo.20373042)
<!--[![NuGet Downloads](https://img.shields.io/nuget/dt/CDCavell.NetCoreApplicationTemplate?label=downloads)](https://www.nuget.org/packages/CDCavell.NetCoreApplicationTemplate)-->

A reusable, production-oriented .NET application template designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

This repository provides a working application baseline with common infrastructure concerns already organized, including middleware ordering, structured logging, forwarded headers, security headers, rate limiting, centralized error handling, authentication and authorization foundations, EF Core data access patterns, GitHub Actions validation, DocFX documentation, and local dotnet new template scaffold support.

## Current Release

<!-- BEGIN LATEST_RELEASE -->
Current release: __[Release 0.5.4](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/tag/v0.5.4)__

Tag: `v0.5.4`
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
- Automated build, test, coverage, template smoke-test, and documentation workflows.
- Local and package-based `dotnet new` template scaffold support.

## Who This Template Is For

This template is intended for developers and teams who want a production-oriented ASP.NET Core starting point with common infrastructure concerns already organized.

It is best suited for:

- Small-to-medium internal applications.
- Line-of-business web applications.
- Prototypes that may grow into production systems.
- Teams that want consistent startup, configuration, logging, security, error handling, and data access patterns.
- Developers who want a practical baseline without adopting a large application framework.

The template is especially useful when an application needs more structure than the default `dotnet new webapp` output but does not yet need a full enterprise framework.

## When This Template May Not Be the Right Fit

This template may not be the best fit for:

- Very small throwaway applications.
- Static sites or applications with no server-side infrastructure needs.
- Highly specialized microservice frameworks.
- Teams that already standardize on a larger opinionated platform or internal enterprise template.
- Applications that need domain-driven layering from the first commit.

The project intentionally provides a secure and maintainable baseline. It does not try to replace every architectural style or framework.

## How This Differs From `dotnet new webapp`

The default ASP.NET Core templates are intentionally minimal. This repository starts further along the production-readiness path by organizing concerns that many real applications eventually need:

- Centralized middleware ordering.
- Structured Serilog request and application logging.
- Forwarded header support for reverse proxy deployments.
- Security header configuration.
- Rate limiting setup.
- Centralized exception and status code handling.
- Problem Details responses.
- Authentication and authorization foundations.
- EF Core provider structure.
- CI validation, template package smoke testing, and documentation publishing.
- Repository governance files for public review and release readiness.

## Application Preview

The starter application includes a simple default Razor Pages landing page that highlights the template's production-oriented infrastructure focus.

![Application preview](docs/images/application-preview.svg)

## Quick Start from Source

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
dotnet build --configuration Release
```

Run tests:

```powershell
dotnet test --configuration Release
```

Run the web application from source:

```powershell
dotnet run --project src/ProjectTemplate.Web
```

Run with Docker Compose:

```powershell
docker compose up --build
```

The Docker-hosted application is available at:

```powershell
http://localhost:8080
```

Docker health endpoints are available at:

```powershell
http://localhost:8080/health
http://localhost:8080/health/ready
http://localhost:8080/health/live
```

## Template Scaffold Status

This repository includes a `dotnet new` template scaffold through `.template.config/template.json` and a package project at `NetCoreApplicationTemplate.Template.csproj`.

The scaffolded consumer output intentionally includes:

- Source projects under `src/`.
- Baseline tests under `tests/`.
- Docker support files.
- `LICENSE.txt`.
- `ASSETS-LICENSES.md`.
- A consumer-oriented `README.md` generated from `.template.content/README.md`.

The scaffolded consumer output intentionally excludes repository-maintainer content such as `.github/`, DocFX documentation source, ADRs, release-management files, citation metadata, contribution policy, security policy, and repository badges.

### Pack and Install Locally

Pack the template package:

```powershell
dotnet pack ./NetCoreApplicationTemplate.Template.csproj --configuration Release --output ./artifacts/template-package
```

Install the generated package:

```powershell
dotnet new install ./artifacts/template-package/CDCavell.NetCoreApplicationTemplate.0.5.4.nupkg
```

Generate a consumer project:

```powershell
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

Build and test the generated output:

```powershell
cd ContosoSecurityPortal
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

Update the installed template by installing a newer package version:

```powershell
dotnet new install <path-or-package-id-for-new-version>
```

Uninstall the template package:

```powershell
dotnet new uninstall CDCavell.NetCoreApplicationTemplate
```

### Local Repository Install

For local development, the template can also be installed from the repository root:

```powershell
dotnet new install .\
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

Package-based install remains the preferred validation path because it more closely matches consumer distribution.

## Documentation

Detailed documentation is maintained in the `docs` folder and published with DocFX.

- [Documentation source](docs/index.md)
- [Published documentation](https://cdcavell.github.io/NetCoreApplicationTemplate/)
- [Architecture Decision Records](https://cdcavell.github.io/NetCoreApplicationTemplate/adr/)
- Generated API reference is available through the published documentation navigation.

Documentation areas include:

- [Getting Started](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/getting-started.html)
- [Docker Development](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/docker.html)
- [Project Structure](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/project-structure.html)
- [Configuration](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/configuration.html)
- [Deployment](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/deployment.html)
- [Middleware Pipeline](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/middleware.html)
- [API Versioning](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/api-versioning.html)
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

## Repository Governance

Repository-level guidance is maintained in root-level files:

- [Contributing](CONTRIBUTING.md)
- [Security Policy](SECURITY.md)
- [Changelog](CHANGELOG.md)
- [Third-Party Asset Notices](ASSETS-LICENSES.md)

Pull requests targeting `main` require passing CI and Code Owner review for owned paths; stale approvals are dismissed when new reviewable commits are pushed.

## Repository Structure

```text
/
├── .github/
│   ├── workflows/
│   │   └── GitHub Actions CI and documentation publishing workflows
│   ├── ISSUE_TEMPLATE/
│   │   └── Issue templates
│   ├── dependabot.yml
│   └── pull_request_template.md
│
├── .template.config/
│   └── dotnet new template metadata
│
├── .template.content/
│   └── consumer scaffold content
│
├── docs/
│   ├── adr/
│   │   └── Architecture decision records
│   ├── articles/
│   │   └── DocFX conceptual documentation
│   ├── images/
│   │   └── Documentation and README images
│   └── docfx.json
│
├── scripts/
│   └── Utility scripts for setup, build, or maintenance
│
├── src/
│   ├── ProjectTemplate.Infrastructure/
│   └── ProjectTemplate.Web/
│
├── tests/
│   └── ProjectTemplate.Web.Tests/
│
├── .dockerignore
├── .editorconfig
├── .gitattributes
├── .gitignore
├── ASSETS-LICENSES.md
├── CHANGELOG.md
├── CITATION.cff
├── CONTRIBUTING.md
├── Dockerfile
├── docker-compose.yml
├── LICENSE.txt
├── NetCoreApplicationTemplate.slnx
├── NetCoreApplicationTemplate.Template.csproj
├── PACKAGE-README.md
├── README.md
└── SECURITY.md
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

The repository currently includes workflows for:

- Restoring dependencies.
- Building the solution in Release configuration.
- Verifying formatting.
- Running tests.
- Generating test result and coverage artifacts.
- Enforcing a 60% minimum line coverage threshold in CI.
- Failing CI when coverage regresses below the threshold.
- Packing the template package.
- Installing the generated `.nupkg` into a clean SDK environment.
- Scaffolding a consumer project with `dotnet new cdcavell-netcoreapp`.
- Validating expected consumer files and excluded maintainer files.
- Building and testing scaffolded output on Linux, Windows, and macOS.
- Running CodeQL analysis.
- Building and publishing DocFX documentation to GitHub Pages.

Template smoke testing runs on pull requests, supported branch pushes, manual workflow dispatch, and release-style version tags matching `v*.*.*`.

See [GitHub Workflow](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/github-workflow.html), [Template Packaging](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/template-packaging.html), and [ADR-0003](docs/adr/0003-record-release-surface-and-distribution-strategy.md) for details.

## Versioning

This project follows Semantic Versioning using the format:

```text
MAJOR.MINOR.PATCH
```

Version numbers are centrally managed through project build metadata so assemblies, future packages, and releases can share a consistent version identity.

See [ADR-0003: Record Release Surface and Distribution Strategy](docs/adr/0003-record-release-surface-and-distribution-strategy.md) for the release-surface decision.

## Citation

Citation metadata is available in [CITATION.cff](CITATION.cff). GitHub can use this file to generate citation formats from the repository sidebar.

Suggested plain-text citation:

```text
Cavell, Christopher D. NetCoreApplicationTemplate. Version 0.5.4. MIT License. https://github.com/cdcavell/NetCoreApplicationTemplate
```

## Roadmap

The project is being developed toward a reusable .NET application template. Future work may include additional provider modules, stronger packaging support, template parameterization, expanded examples, and release-ready template distribution.

See [Template Packaging](https://cdcavell.github.io/NetCoreApplicationTemplate/articles/template-packaging.html) for the current packaging direction.

## License

This project is licensed under the MIT License.

See [LICENSE.txt](LICENSE.txt) for full license details.

Third-party assets, libraries, templates, icons, fonts, images, or other externally sourced materials used by this project are documented in [ASSETS-LICENSES.md](ASSETS-LICENSES.md).
