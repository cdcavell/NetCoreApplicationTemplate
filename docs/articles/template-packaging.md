# Template Packaging

This repository includes an initial `dotnet new` template scaffold.

The scaffold allows the repository to be installed locally as a project template and used to generate a new application from the current source structure.

## Install the Template Locally

From the repository root:

```powershell
dotnet new install .\
```
On Linux or macOS:
```bash
dotnet new install ./
```
## Create a New Project from the Template
From a separate working directory:
```powershell
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```
_For the initial template scaffold, use a project name that is also a valid C# identifier, such as `ContosoSecurityPortal`. Dotted project names require additional template symbol handling so namespace replacement and type-name replacement can be handled separately._

This creates a new project using ContosoSecurityPortal as the replacement name for the source template namespace and project prefix.
## Build the Generated Project
```powershell
cd ContosoSecurityPortal
dotnet build
dotnet test
```

## Distribution Direction

The current scaffold supports local installation from the repository root. This is useful for development, validation, and preview use while the template is still being finalized.

The intended stable distribution model is a published NuGet template package installable with `dotnet new install`.

Future stable usage is expected to follow this pattern:

```powershell
dotnet new install <published-template-package-id>
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

Clone-and-modify remains valid for source review, contribution, and direct customization. However, after package publishing is available, the NuGet template package should be treated as the primary stable distribution path for normal template consumers.

After the `v1.0.0` release, changes to the template short name, package identity, template parameters, symbols, or source-name replacement behavior should be reviewed as release-surface changes.

See [ADR-0003: Record Release Surface and Distribution Strategy](../adr/0003-record-release-surface-and-distribution-strategy.md) for the release-surface decision.
