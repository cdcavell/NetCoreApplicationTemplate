## Data Access

### EF Core, SQLite, and Database Updates

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
SQLite is used as the default development provider because it is lightweight, file-based, and does not require a separate database server.

EF Core migrations are stored in the infrastructure project because `ProjectTemplate.Infrastructure` owns the `ApplicationDbContext`, entities, and EF Core configuration.

The web project is used as the startup project because it provides application configuration, dependency injection, provider setup, and connection-string resolution.

### EF Core CLI Tool
The dotnet ef command requires the EF Core command-line tool.

Check whether the tool is available:
```bash
dotnet ef --version
```
If the command is not found, install or update the tool:
```bash
dotnet tool install --global dotnet-ef
```
Or update an existing global installation:
```bash
dotnet tool update --global dotnet-ef
```

### Add a Migration
Create a new migration from the repository root:
```bash
dotnet ef migrations add MigrationName `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output-dir Data/Migrations
```
Replace `MigrationName` with a descriptive name, such as:
```bash
dotnet ef migrations add AddExternalLoginAccounts `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --output-dir Data/Migrations
```
### Update the Local Database
Apply pending migrations to the configured local SQLite database:
```bash
dotnet ef database update `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
```
This creates or updates the local SQLite database using the `ApplicationDatabase` connection string resolved by the startup project.

### Verify Pending Migrations
List available migrations:
```bash
dotnet ef migrations list `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
```
Generate a SQL script for review:
```bash
dotnet ef migrations script `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext
  --output migration.sql
```
### Connection String Resolution
Migration commands use the startup project to resolve configuration.

For this application, that means the connection string comes from:
```bash
src/ProjectTemplate.Web/appsettings.json
src/ProjectTemplate.Web/appsettings.{Environment}.json
user secrets
environment variables
other configured providers
```
By default, the application resolves:
```bash
ConnectionStrings:ProjectTemplateDatabase
```
For local development, the default SQLite value is:
```bash
Data Source=application-dev.db
```
Applications can override this value through normal ASP.NET Core configuration sources.

For example, an environment variable can override the connection string:
```bash
ConnectionStrings__ApplicationDatabase=Data Source=custom-application-dev.db
```
### Automatic Startup Migrations

The application does not automatically run EF Core migrations during application startup.

This is intentional.

Automatic startup migration execution can be useful for small local development scenarios, but it can be unsafe in production because multiple application instances may start at the same time, schema changes may require review, and failed migrations can prevent the application from starting cleanly.

For now, database migration execution should remain an explicit developer or deployment action.

A future issue may add an opt-in startup migration feature for development-only or controlled hosting scenarios, but it should include clear safeguards before being used.

### Recommended Production Posture

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
```bash
dotnet ef migrations script `
  --project src/ProjectTemplate.Infrastructure `
  --startup-project src/ProjectTemplate.Web `
  --context ApplicationDbContext `
  --idempotent `
  --output migration.sql
```
The `--idempotent` option is useful for deployment scenarios where the target database may already have some migrations applied.

### SQLite Development Flow
A common local development flow is:
```bash
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

### Troubleshooting
If dotnet ef is not recognized, install or update the EF Core CLI tool:
```bash
dotnet tool install --global dotnet-ef
```
If the startup project cannot be built, run:
```bash
dotnet build --configuration Release
```
If the connection string cannot be found, confirm that `ConnectionStrings:ApplicationDatabase` exists in the startup project configuration.

If migrations are not discovered, confirm that the command uses:
```bash
--project src/ProjectTemplate.Infrastructure
--startup-project src/ProjectTemplate.Web
--context ApplicationDbContext
```
If SQLite provider configuration fails, confirm that the infrastructure project references the SQLite provider package and that the data access registration uses the configured ApplicationDatabase connection string.

Future database providers, such as SQL Server, can be added by extending the data access registration configuration.
SQLite remains the default development provider. SQL Server can be selected through configuration. Because EF Core migrations are provider-specific, production SQL Server deployments should generate and maintain SQL Server-compatible migrations before applying database updates.

### External Login Account Linking Persistence

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
