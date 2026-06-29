![.NET Core Application Social Preview](https://raw.githubusercontent.com/cdcavell/NetCoreApplicationTemplate/main/docs/images/social-preview.png)

# .NET Core Application Template 
[![CI](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/ci.yml)
[![Coverage Report](https://img.shields.io/badge/coverage%20gate-75%25-brightgreen)](https://cdcavell.github.io/NetCoreApplicationTemplate/coverage/index.html)
[![Documentation](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml/badge.svg)](https://github.com/cdcavell/NetCoreApplicationTemplate/actions/workflows/publish-docs.yml)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://cdcavell.github.io/NetCoreApplicationTemplate/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE.txt)
[![GitHub Release](https://img.shields.io/github/v/release/cdcavell/NetCoreApplicationTemplate?display_name=tag)](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/NetCoreApplicationTemplate?label=NuGet)](https://www.nuget.org/packages/NetCoreApplicationTemplate)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetCoreApplicationTemplate?label=downloads)](https://www.nuget.org/packages/NetCoreApplicationTemplate)
[![Zenodo DOI](https://img.shields.io/badge/DOI-10.5281%2Fzenodo.20373042-blue)](https://doi.org/10.5281/zenodo.20373042)

A reusable, production-oriented .NET application template designed to provide a secure, maintainable, and extensible baseline for building ASP.NET Core applications.

This repository provides a working application baseline with common infrastructure concerns already organized, including middleware ordering, structured logging, forwarded headers, security headers, rate limiting, centralized error handling, authentication and authorization foundations, EF Core data access patterns, GitHub Actions validation, DocFX documentation, and local dotnet new template scaffold support.

## Current Release

<!-- BEGIN LATEST_RELEASE -->
Current release: __[Release 2.2.0](https://github.com/cdcavell/NetCoreApplicationTemplate/releases/tag/v2.2.0)__

Tag: `v2.2.0`
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

### Install from NuGet

Install the published template package:

```powershell
dotnet new install NetCoreApplicationTemplate::2.2.0
```

### Pack and Install Locally

Pack the template package:

```powershell
dotnet pack ./NetCoreApplicationTemplate.Template.csproj --configuration Release --output ./artifacts/template-package
```

Install the generated package:

```powershell
dotnet new install ./artifacts/template-package/NetCoreApplicationTemplate.2.2.0.nupkg
```

Generate a consumer project:

```powershell
dotnet new netcoreapp-template -n ContosoSecurityPortal
```

Build and test the generated output:

```powershell
cd ContosoSecurityPortal
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

Generate a non-default scaffold with authentication disabled and SQL Server selected as the data provider:

```powershell
dotnet new netcoreapp-template `
  --name ContosoNoAuthSqlServer `
  --authProvider none `
  --dbProvider sqlserver
```

Generate a scaffold with authentication and EF Core data access disabled:

```powershell
dotnet new netcoreapp-template `
  --name ContosoNoAuthNoDataAccess `
  --authProvider none `
  --dbProvider none
```

Build and test either non-default generated output, for example:

```powershell
cd ContosoNoAuthSqlServer
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

```powershell
cd ContosoNoAuthNoDataAccess
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

The non-default scaffold preserves the template's core guardrails, including structured logging, centralized error handling, health checks, security headers, rate limiting, and safe defaults.

Update the installed template by installing a newer package version:

```powershell
dotnet new install NetCoreApplicationTemplate::2.2.0
```

Uninstall the template package:

```powershell
dotnet new uninstall NetCoreApplicationTemplate
```

### Local Repository Install

For local development, the template can also be installed from the repository root:

```powershell
dotnet new install .\
dotnet new netcoreapp-template -n ContosoSecurityPortal
```

Package-based install remains the preferred validation path because it more closely matches consumer distribution.

## Documentation

Detailed documentation is maintained in the `docs` folder and published with DocFX.
