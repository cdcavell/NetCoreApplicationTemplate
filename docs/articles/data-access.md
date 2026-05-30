# Data Access

## EF Core, SQLite, and Database Updates

The application includes an initial EF Core data access foundation using SQLite as the default local development provider.

SQLite is used as the default development provider because it is lightweight, file-based, and does not require a separate database server.

The SQLite connection string is configured in:

```text
src/ProjectTemplate.Web/appsettings.json
```
The default connection string uses a local SQLite database file:
```json
"ConnectionStrings": {
  "ApplicationDatabase": "Data Source=application-dev.db"
}
```

## Data Access Configuration

Data access behavior is configured under `ProjectTemplate:DataAccess`:

```json
"ProjectTemplate": {
  "DataAccess": {
    "Provider": "Sqlite",
    "ConnectionStringName": "ApplicationDatabase",
    "Auditing": {
      "Enabled": true
    }
  }
}
```

The configuration values are:
|Setting|Purpose|
|:------|:------|
|`Provider`|Selects the data access mode. Supported values are `Sqlite`, `SqlServer`, `None`, and `Disabled`. The default local development provider is `Sqlite`.|
|`ConnectionStringName`|Identifies which named connection string should be resolved from `ConnectionStrings` when data access is enabled. This value is not required when `Provider` is `None` or `Disabled`.|
|`Auditing:Enabled`|Enables or disables EF Core audit record creation during `SaveChanges` and `SaveChangesAsync` when EF Core data access is enabled.|

By default, the application uses:

```text
ProjectTemplate:DataAccess:Provider = Sqlite
ProjectTemplate:DataAccess:ConnectionStringName = ApplicationDatabase
ProjectTemplate:DataAccess:Auditing:Enabled = true
```

With the default configuration, the application resolves:

```text
ConnectionStrings:ApplicationDatabase
```

EF Core migrations are stored in the infrastructure project because `ProjectTemplate.Infrastructure` owns the `ApplicationDbContext`, entities, and EF Core configuration.

The web project is used as the startup project because it provides application configuration, dependency injection, provider setup, and connection-string resolution.

## Disabled Data Access Mode

Applications that do not need the template's EF Core data access layer can opt out explicitly:

```json
"ProjectTemplate": {
  "DataAccess": {
    "Provider": "None"
  }
}
```

`Disabled` is also accepted as an alias:

```json
"ProjectTemplate": {
  "DataAccess": {
    "Provider": "Disabled"
  }
}
```

When data access is disabled, the application does not register:

- `ApplicationDbContext`
- `IDbContextFactory<ApplicationDbContext>`
- EF-backed application services that require `ApplicationDbContext`

Disabled mode also does not require `ProjectTemplate:DataAccess:ConnectionStringName` to resolve to an existing connection string. This mode is appropriate for lightweight applications, workers, external modules, static front ends, or services that use a separate persistence strategy.

Disabled mode should not be used by applications that need EF Core migrations, audit records, external login account persistence, or any service that depends on `ApplicationDbContext`.

## EF Core CLI Tool
The dotnet ef command requires the EF Core command-line tool.

Check whether the tool is available:
```powershell
dotnet ef --version
```
If the command is not found, install or update the tool:
```powershell
dotnet tool install --global dotnet-ef
```
Or update an existing global installation:
```powershell
dotnet tool update --global dotnet-ef
```

## Add a Migration
Create a new migration from the repository root:
```powershell
dotnet ef migrations add MigrationName `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output-dir Data/Migrations
```
Replace `MigrationName` with a descriptive name, such as:
```powershell
dotnet ef migrations add AddExternalLoginAccounts `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output-dir Data/Migrations
```
## Update the Local Database
Apply pending migrations to the configured local SQLite database:
```powershell
dotnet ef database update `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
```
This creates or updates the local SQLite database using the `ApplicationDatabase` connection string resolved by the startup project.

## Verify Pending Migrations
List available migrations:
```powershell
dotnet ef migrations list `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
```
Generate a SQL script for review:
```powershell
dotnet ef migrations script `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output migration.sql
```
## Connection String Resolution

Migration commands use the startup project to resolve configuration.

For this application, configuration may come from:

```powershell
src/ProjectTemplate.Web/appsettings.json
src/ProjectTemplate.Web/appsettings.{Environment}.json
user secrets
environment variables
other configured providers
```

The application reads the configured connection string name from:

```text
ProjectTemplate:DataAccess:ConnectionStringName
```

By default, this value is:

```text
ApplicationDatabase
```

The application then resolves the matching named connection string from:

```text
ConnectionStrings:ApplicationDatabase
```

For local development, the default SQLite value is:
```csharp
Data Source=application-dev.db
```

Applications can override either the selected connection string name or the connection string value through normal ASP.NET Core configuration sources.

For example, an environment variable can override the connection string value:

```powershell
ConnectionStrings__ApplicationDatabase=Data Source=custom-application-dev.db
```

An environment variable can also select a different configured connection string name:

```powershell
ProjectTemplate__DataAccess__ConnectionStringName=ApplicationSqlServer
```

When `ConnectionStringName` is set to `ApplicationSqlServer`, the application resolves:

```text
ConnectionStrings:ApplicationSqlServer
```

When `Provider` is set to `None` or `Disabled`, connection string resolution is skipped.

## EF Core Auditing

The template includes a baseline EF Core audit trail.

`ApplicationDbContext.SaveChanges` and `SaveChangesAsync` are overridden to generate audit records for tracked entity changes when auditing is enabled.

Audit records may include:

- Entity/table name
- Entity state
- Key values
- Original values
- Current values
- Actor information
- Application context
- UTC timestamp

Auditing is enabled by default:

```json
"ProjectTemplate": {
  "DataAccess": {
    "Auditing": {
      "Enabled": true
    }
  }
}
```

To disable audit record creation:
```json
"ProjectTemplate": {
  "DataAccess": {
    "Auditing": {
      "Enabled": false
    }
  }
}
```

When the application starts, the data access startup log records the configured provider, connection string name, and EF Core auditing status. If data access is disabled, the startup log reports that EF Core services were not registered.

When auditing is enabled, audit records are written to the application database. The template does not include automatic pruning, retention, archival, legal hold, masking, export, or purge behavior for audit records.

Consuming applications are responsible for deciding how audit records are retained, archived, masked, purged, or moved to long-term storage. Before enabling auditing in production, review whether audited values may contain sensitive or regulated data.

## Persisted String Canonicalization

The template canonicalizes string scalar values for added entities and modified string properties before EF Core persists changes.

Canonicalization decodes bounded HTML/entity encoding and normalizes Unicode into a stable representation before save. This prevents common single-encoded or double-encoded values from being stored inconsistently while preserving raw special characters such as apostrophes, quotation marks, ampersands, and accented characters.

This behavior is intended for persistence consistency and defense-in-depth. It is not a replacement for SQL injection protection.

The primary SQL injection protections remain:

- EF Core parameterized commands.
- Avoiding manually concatenated SQL.
- Validating application input according to domain rules.
- Keeping output encoding context-specific.

The template does not blanket HTML-encode values before database storage. Razor/UI output encoding and any API-specific encoding rules remain the responsibility of the output layer.

## Optimistic Concurrency

The template includes baseline optimistic concurrency detection for entities that inherit from `DataEntity`.

Each data entity includes a `ConcurrencyStamp` value. EF Core configures this value as a concurrency token. When an entity is updated or deleted, EF Core includes the original concurrency token value in the database update check. If another context or application instance has already changed the row, the update affects zero rows and EF Core throws `DbUpdateConcurrencyException`.

The template uses an application-managed string concurrency stamp instead of a database-generated SQL Server `rowversion`. This keeps the default behavior provider-safe for SQLite local development while still supporting SQL Server-oriented production paths.

`ApplicationDbContext.SaveChanges` and `SaveChangesAsync` refresh the concurrency stamp for modified entities before saving. Concurrency conflicts are logged and rethrown. The template does not automatically retry, merge, or overwrite conflicting changes.

Consuming applications should decide how to handle conflicts based on domain needs. Common options include showing the user a reload-and-retry message, re-querying the current database values, or implementing an application-specific merge workflow.


## Automatic Startup Migrations

The application does not automatically run EF Core migrations during application startup.

This is intentional.

Automatic startup migration execution can be useful for small local development scenarios, but it can be unsafe in production because multiple application instances may start at the same time, schema changes may require review, and failed migrations can prevent the application from starting cleanly.

For now, database migration execution should remain an explicit developer or deployment action.

A future issue may add an opt-in startup migration feature for development-only or controlled hosting scenarios, but it should include clear safeguards before being used.

## Recommended Production Posture

Production database updates should be handled outside normal application startup.

Recommended options include:
- CI/CD pipeline migration steps.
- Reviewed SQL migration scripts.
- Manual DBA-approved migration execution.
- Dedicated deployment jobs that run before the application is released.
- Environment-specific connection strings supplied through deployment secrets or secure configuration.

Before applying migrations to production:
__1.__ Review the generated migration.
__2.__ Generate and inspect the SQL script.
__3.__ Back up the target database when appropriate.
__4.__ Apply the migration through a controlled deployment process.
__5.__ Confirm the application version and database schema are compatible.

Example script generation command:
```powershell
dotnet ef migrations script `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --idempotent `
  --output migration.sql
```
The `--idempotent` option is useful for deployment scenarios where the target database may already have some migrations applied.

## SQLite Development Flow
A common local development flow is:
```powershell
dotnet restore
dotnet build --configuration Release
dotnet ef database update `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
dotnet test --configuration Release
```
To recreate the local SQLite database from scratch, stop the application, delete the local `.db` file, and run `dotnet ef database update` again.

Only do this for disposable local development databases.

## Troubleshooting
If dotnet ef is not recognized, install or update the EF Core CLI tool:
```powershell
dotnet tool install --global dotnet-ef
```
If the startup project cannot be built, run:
```powershell
dotnet build --configuration Release
```
If the connection string cannot be found, confirm that `ConnectionStrings:ApplicationDatabase` exists in the startup project configuration.

If migrations are not discovered, confirm that the command uses:
```powershell
--project src/ProjectTemplate.Infrastructure
--startup-project src/ProjectTemplate.Web
--context ApplicationDbContext
```

If data access provider configuration fails, confirm that `ProjectTemplate:DataAccess:Provider` is configured. Use `Sqlite` or `SqlServer` when EF Core data access is required. Use `None` or `Disabled` only when the application intentionally does not need EF Core registrations.

Future database providers, such as SQL Server, can be added by extending the data access registration configuration.
SQLite remains the default development provider. SQL Server can be selected through configuration. Because EF Core migrations are provider-specific, production SQL Server deployments should generate and maintain SQL Server-compatible migrations before applying database updates.

## External Login Account Linking Persistence

The application includes an optional EF Core persistence model for applications that need to link external provider identities to local application users.

This is different from claims-only sign-in.

Claims-only sign-in uses the external provider claims from the current authentication session and does not require a local account-linking table. This is sufficient when the application only needs to authenticate the current request.

Local account linking is useful when an application needs to associate one or more external identities with a local application user profile, preserve account-link audit history, support provider migration, or allow multiple providers to sign in to the same local account.

The external login persistence model stores:

- Local user ID
- Provider name
- Provider user ID
- Display name
- Email
- Created, updated, and last-login timestamps

Provider tokens are not stored by default. Applications that need token persistence should add that behavior intentionally and review the security, encryption, rotation, and retention requirements before enabling it.
