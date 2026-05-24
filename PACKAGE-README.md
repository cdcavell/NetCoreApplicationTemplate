# .NET Core Application Template Package

This package installs the `cdcavell-netcoreapp` project template for `dotnet new`.

## Install from a local package

```powershell
dotnet new install ./artifacts/template-package/CDCavell.NetCoreApplicationTemplate.0.4.2.nupkg
```

## Generate a project

```powershell
dotnet new cdcavell-netcoreapp -n ContosoSecurityPortal
```

## Build and test generated output

```powershell
cd ContosoSecurityPortal
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## Update

Install the newer package version with `dotnet new install`.

```powershell
dotnet new install <path-or-package-id-for-new-version>
```

## Uninstall

```powershell
dotnet new uninstall CDCavell.NetCoreApplicationTemplate
```

This package README is intended for template package distribution. Generated projects receive their own consumer README from the scaffold.
