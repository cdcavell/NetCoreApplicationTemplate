![.NET Core Application Social Preview](https://raw.githubusercontent.com/cdcavell/NetCoreApplicationTemplate/main/docs/images/social-preview.png)

# .NET Core Application Template

A reusable dotnet new template for creating a production-oriented ASP.NET Core application baseline with structured logging, security headers, forwarded headers, rate limiting, centralized error handling, authentication-ready architecture, and EF Core-ready structure.

This README is intended for NuGet package consumers. The full repository README and documentation site provide deeper implementation and maintainer guidance.

## Template identity

| Item | Value |
|---|---|
| Package ID | `NetCoreApplicationTemplate` |
| Template short name | `netcoreapp-template` |
| Default authentication provider | `cookie` |
| Default data provider | `sqlite` |

## Install

Install the template package from NuGet:

```text
dotnet new install NetCoreApplicationTemplate::2.2.0
```

For local package validation, install a packed package directly:

```text
dotnet new install ./artifacts/template-package/NetCoreApplicationTemplate.2.2.0.nupkg
```

## Generate a project

Create a default scaffold:

```text
dotnet new netcoreapp-template -n ContosoSecurityPortal
```

The default scaffold uses the cookie-authentication-ready baseline and SQLite development configuration.

Generate with authentication disabled:

```text
dotnet new netcoreapp-template --name ContosoNoAuth --authProvider none
```

Generate with SQL Server selected:

```text
dotnet new netcoreapp-template --name ContosoSqlServer --dbProvider sqlserver
```

Generate with authentication disabled and SQL Server selected:

```text
dotnet new netcoreapp-template --name ContosoNoAuthSqlServer --authProvider none --dbProvider sqlserver
```

## Template options

| Option | Default | Supported values | Description |
|---|---|---|---|
| `--authProvider` | `cookie` | `cookie`, `none` | Selects the generated authentication baseline. |
| `--dbProvider` | `sqlite` | `sqlite`, `sqlserver`, `none` | Selects the generated EF Core provider configuration. |
| `--skipRestore` | `false` | `true`, `false` | Skips the post-create NuGet restore action when set to `true`. |

## Build and test generated output

```text
cd ContosoSecurityPortal
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## Update

Install the newer package version with the same package identity:

```text
dotnet new install NetCoreApplicationTemplate::2.2.0
```

## Uninstall

```text
dotnet new uninstall NetCoreApplicationTemplate
```

## Additional resources

- GitHub repository: https://github.com/cdcavell/NetCoreApplicationTemplate
- Published documentation: https://cdcavell.github.io/NetCoreApplicationTemplate/
- Template packaging documentation: https://cdcavell.github.io/NetCoreApplicationTemplate/articles/template-packaging.html
- Changelog: https://github.com/cdcavell/NetCoreApplicationTemplate/blob/main/CHANGELOG.md
- License: https://github.com/cdcavell/NetCoreApplicationTemplate/blob/main/LICENSE.txt
- Releases: https://github.com/cdcavell/NetCoreApplicationTemplate/releases
- Coverage Report: https://cdcavell.github.io/NetCoreApplicationTemplate/coverage/index.html

Generated projects receive their own consumer-oriented README from the scaffold. This package README is intentionally limited to NuGet installation, scaffold options, validation commands, and consumer-facing reference links.
