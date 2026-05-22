# Template Packaging

This repository includes an initial `dotnet new` template scaffold.

The scaffold allows the repository to be installed locally as a project template and used to generate a new application from the current source structure.

## Install the Template Locally

From the repository root:

```bash
dotnet new install .\
```
On Linux or macOS:
```bash
dotnet new install ./
```
## Create a New Project from the Template
From a separate working directory:
```bash
dotnet new cavell-netcoreapp -n ContosoSecurityPortal
```
_For the initial template scaffold, use a project name that is also a valid C# identifier, such as `ContosoSecurityPortal`. Dotted project names require additional template symbol handling so namespace replacement and type-name replacement can be handled separately._

This creates a new project using ContosoSecurityPortal as the replacement name for the source template namespace and project prefix.
## Build the Generated Project
```bash
cd ContosoSecurityPortal
dotnet build
dotnet test
```
