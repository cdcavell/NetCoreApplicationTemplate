# Getting Started

This guide explains how to clone, build, test, run, and locally document the .NET Core Application Template.

## Prerequisites

Install the following tools before working with the repository:

- .NET SDK 10.0 or later
- Git
- Visual Studio 2026 or another .NET-capable editor
- Optional: EF Core CLI tools for database migration work
- Optional: DocFX local tool restored through `dotnet tool restore`

## Clone the Repository

```powershell
git clone https://github.com/cdcavell/NetCoreApplicationTemplate.git
cd NetCoreApplicationTemplate
```

## Restore Dependencies
```powershell
dotnet restore
```

## Build the Solution
```powershell
dotnet build
```

For the same configuration used by CI:
```powershell
dotnet build --configuration Release
```

## Run Tests
```powershell
dotnet test
```

## Run the Web Application
```powershell
dotnet run --project src/ProjectTemplate.Web
```
The application uses the configuration from `src/ProjectTemplate.Web/appsettings.json` and environment-specific configuration when present.

## Local Database
The default local database provider is SQLite. The default connection string is:

```json
"ConnectionStrings": {
  "ApplicationDatabase": "Data Source=application-dev.db"
}
```
Database migrations are handled explicitly through EF Core CLI commands. The application does not automatically run migrations at startup.

## Build Documentation Locally
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
The local documentation site is usually available at:
```powershell
http://localhost:8080
```

## Development Notes
Do not commit real secrets, provider credentials, tokens, certificates, private keys, or production connection strings.

Use user secrets, environment variables, deployment secrets, or a secure secret store for sensitive configuration.
