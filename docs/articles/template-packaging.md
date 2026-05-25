# Template Packaging

This repository includes a `dotnet new` template scaffold and a NuGet template package project.

The scaffold can be installed locally from source for development, or packed into a `.nupkg` and installed through the same package-based flow expected for consumers.

Package-based validation is preferred because it verifies the actual distribution artifact instead of only the repository working tree.

## Template Identity

| Field | Value |
|:---|:---|
| Template short name | `cdcavell-netcoreapp` |
| Template package ID | `CDCavell.NetCoreApplicationTemplate` |
| Source replacement token | `ProjectTemplate` |
| Current package version | `0.5.3` |

## Consumer Scaffold Boundaries

The scaffolded output intentionally includes:

- Source projects under `src/`.
- Baseline tests under `tests/`.
- Docker support files.
- `LICENSE.txt`.
- `ASSETS-LICENSES.md`.
- A consumer-oriented `README.md` generated from `.template.content/README.md`.

The scaffolded output intentionally excludes repository-maintainer content such as:

- `.github/` workflow and issue-template files.
- `.template.config/` and `.template.content/` authoring files.
- DocFX documentation source and ADRs.
- Changelog, citation, contribution, security, and release-management files.
- Repository maintainer badges and release instructions.

## Pack the Template Package

From the repository root:

```powershell
dotnet pack ./NetCoreApplicationTemplate.Template.csproj --configuration Release --output ./artifacts/template-package
```

## Install the Template Package

```powershell
dotnet new install ./artifacts/template-package/CDCavell.NetCoreApplicationTemplate.0.5.3.nupkg
```

## Create a New Project from the Template

From a separate working directory:

```powershell
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

Use a project name that is also a valid C# identifier, such as `ContosoSecurityPortal`. Dotted project names require additional template symbol handling so namespace replacement and type-name replacement can be handled separately.

This creates a new project using `ContosoSecurityPortal` as the replacement name for the source template namespace and project prefix.

## Restore, Build, and Test the Generated Project

```powershell
cd ContosoSecurityPortal
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## Update the Installed Template

Install the newer package version:

```powershell
dotnet new install <path-or-package-id-for-new-version>
```

The .NET SDK updates the installed template package when the package identity matches and the new package version is higher.

## Uninstall the Template

```powershell
dotnet new uninstall CDCavell.NetCoreApplicationTemplate
```

## Local Repository Install

For local authoring and quick iteration, the template can still be installed from the repository root.

On Windows:

```powershell
dotnet new install .\
```

On Linux or macOS:

```bash
dotnet new install ./
```

Then generate from a separate working directory:

```powershell
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

Local repository install is useful during template development, but package-based install should remain the primary validation path before release.

## CI Smoke Test

The CI workflow packs the template package, installs the generated `.nupkg`, scaffolds a new project with `dotnet new cdcavell-netcoreapp`, validates expected and forbidden scaffolded paths, builds the generated output, runs generated tests, and uninstalls the template package.

The smoke test runs on Linux, Windows, and macOS so path handling and package install behavior are validated across supported runner environments.

## Distribution Direction

The intended stable distribution model is a published NuGet template package installable with `dotnet new install`.

Future stable usage is expected to follow this pattern:

```powershell
dotnet new install CDCavell.NetCoreApplicationTemplate
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

Clone-and-modify remains valid for source review, contribution, and direct customization. However, after package publishing is available, the NuGet template package should be treated as the primary stable distribution path for normal template consumers.

After the `v1.0.0` release, changes to the template short name, package identity, template parameters, symbols, or source-name replacement behavior should be reviewed as release-surface changes.

See [ADR-0003: Record Release Surface and Distribution Strategy](../adr/0003-record-release-surface-and-distribution-strategy.md) for the release-surface decision.
