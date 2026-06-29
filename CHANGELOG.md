# Changelog

All notable changes to this project are documented in this file.

This project follows Semantic Versioning using the format `MAJOR.MINOR.PATCH`.

## 2.2.0 - 2026-06-29

### Added

* Added `IApplicationSaveChangesPipeline` and `ApplicationSaveChangesPipeline` to move EF Core save preparation out of `ApplicationDbContext` and into an application-owned persistence pipeline.
* Added `ApplicationSaveChangesInterceptor` as the composite EF Core save lifecycle interceptor for invoking the save pipeline through `SavingChanges` / `SavingChangesAsync` and `SavedChanges` / `SavedChangesAsync` hooks.
* Added EF Core save pipeline documentation covering default pipeline order, extension seams, audit lifecycle safety, and the decision to keep a composite interceptor by default.
* Added ADR 0004 documenting the decision to keep the composite SaveChanges interceptor until a concrete consumer or maintenance need justifies specialized interceptors.
* Added tests that verify sync and async save-pipeline invocation through `ApplicationDbContext`.
* Added branch-focused tests for `ApplicationSaveChangesInterceptor`, including constructor null-guard, non-`ApplicationDbContext`, and bounded after-save follow-up branches.

### Changed

* Reduced repeated EF Core `ChangeTracker` inspection by materializing Added/Modified/Deleted entries once and reusing that list across string canonicalization, lookup normalization, timestamp normalization, concurrency stamping, and audit entry creation.
* Reduced `ApplicationDbContext` save overrides to optimistic-concurrency exception handling around EF Core's native save flow.
* Kept `ConcurrencyStamp` as the default application-managed optimistic concurrency token and documented why that remains the portable SQLite / SQL Server baseline.
* Updated package icon and favicon image assets used for repository, NuGet package, and documentation branding.
* Updated release metadata, package README examples, template packaging docs, citation metadata, and Zenodo metadata for `2.2.0`.

### Notes

* This is a minor release because it introduces and documents a clearer EF Core save-pipeline extension seam while preserving the stable `2.x` package identity, template short name, template options, and default scaffold behavior.
* The default generated scaffold continues to use the same package identity, template identity, authentication options, data-access options, and local SQLite development path.
* SQL Server-only consumers may still replace the application-managed concurrency token with provider-native rowversion behavior when appropriate, but the template default remains provider-portable.

## 2.1.0 - 2026-06-27

### Added

* Added optional application and domain layer guidance for consumers who outgrow the default `Web` / `Infrastructure` split.
* Added production authentication hardening guidance covering provider configuration, HTTPS/proxy behavior, redirect and callback URLs, cookie security, claims translation, token handling, session behavior, and provider smoke testing.
* Added middleware ordering rationale and documented order-sensitive invariants for the centralized application pipeline.
* Added a template-owned `IApplicationAuditStore` seam for application audit records.
* Added `ProjectTemplate:DataAccess:Auditing:StorageMode` with `Local` as the built-in default and `Outbox` / `ExternalSink` as explicit extension-mode names.
* Added focused tests for default local audit store registration, audit storage mode configuration, custom audit store behavior, and sync no-op save behavior.

### Changed

* Routed synchronous and asynchronous audit record creation through the audit store seam while preserving local EF Core audit storage by default.
* Short-circuited `ApplicationDbContext.SaveChanges` and `SaveChangesAsync` when the EF Core change tracker has no pending changes.
* Refreshed configuration, data-access, public-surface, and example appsettings documentation to align with the audit storage configuration shape.
* Updated article index and documentation navigation coverage for newer guidance pages.
* Reformatted documentation image assets for consistent repository and documentation display.

### Notes

* This is a minor release because it adds optional extension points and documentation while preserving the stable `2.x` package identity, template short name, template options, and default scaffold behavior.
* Local audit storage remains the default. `Outbox` and `ExternalSink` modes require a consuming application to register a custom `IApplicationAuditStore` implementation.
* Existing consumers do not need to change configuration unless they intentionally adopt a non-local audit storage mode.

## 2.0.1 - 2026-06-26

### Changed

* Refreshed post-2.0 documentation to align README, DocFX navigation, package guidance, release guidance, support policy, maintainer guidance, security policy, telemetry notes, runtime readiness notes, and Docker documentation with the current `NetCoreApplicationTemplate` package identity.
* Replaced repository/package branding assets with an original NetCoreApplicationTemplate icon that better reflects the project as a secure, extensible ASP.NET Core application baseline.
* Updated documentation-site branding to use the new icon assets without disrupting DocFX navigation layout.
* Confirmed NuGet Trusted Publishing workflow permissions include the required GitHub Actions OIDC token permission for public package publication.

### Notes

* This is a patch release focused on documentation, package metadata, release-readiness cleanup, branding assets, and trusted-publishing readiness.
* No generated scaffold behavior, template short name, template options, or public package identity changes are included.

## 2.0.0 - 2026-06-25

### Breaking Changes

* Renamed the NuGet package from `CDCavell.NetCoreApplicationTemplate` to `NetCoreApplicationTemplate`.
* Consumers should update package installation commands and references to use the new package ID.

### Changed

* Updated the public NuGet package identity to the simplified project-centered name `NetCoreApplicationTemplate`.
* Updated package metadata to align with the new package ID.
* Updated release metadata for the `2.0.0` package line.
* Switched NuGet.org publication to **NuGet Trusted Publishing**, removing the need for a long-lived NuGet API key for public package publishing.
* Updated the package publishing workflow to use GitHub Actions OIDC authentication for NuGet.org publishing.

### Migration Notes

Replace package installation commands such as:

```powershell
dotnet new install CDCavell.NetCoreApplicationTemplate
```

with:

```powershell
dotnet new install NetCoreApplicationTemplate
```

If the old package is already installed locally, uninstall it first:

```powershell
dotnet new uninstall CDCavell.NetCoreApplicationTemplate
dotnet new install NetCoreApplicationTemplate
```

### Notes

The previous `CDCavell.NetCoreApplicationTemplate` package should be treated as the legacy package identity and deprecated on NuGet with alternate package guidance pointing to `NetCoreApplicationTemplate`.

This release does not change the project’s core purpose: providing a production-oriented ASP.NET Core application template with structured logging, security headers, forwarded headers, rate limiting, centralized error handling, authentication-ready architecture, and EF Core-ready structure.

## 1.0.4 - 2026-06-22

### Fixed

* Added `SQLitePCLRaw.bundle_e_sqlite3` package reference to replace deprecated SQLite bundle usage.
* Updated SQLite dependency path to use the supported bundled native SQLite provider.
* Resolved dependency warning related to deprecated SQLite library usage.

### Notes

* This is a dependency maintenance release.
* No application behavior, public APIs, or template structure were intentionally changed.

## 1.0.3 - 2026-06-15

### Changed

* Updated centrally managed NuGet package versions used by the template, tests, persistence layer, authentication integrations, observability support, configuration abstractions, logging configuration, and test dependencies.
* Updated Microsoft ASP.NET Core authentication and MVC testing packages from `10.0.8` to `10.0.9`.
* Updated Microsoft Entity Framework Core packages from `10.0.8` to `10.0.9`.
* Updated `Microsoft.Extensions.Configuration.Abstractions` from `10.0.8` to `10.0.9`.
* Updated OpenTelemetry hosting/exporter packages from `1.15.3` to `1.16.0`.
* Updated `Serilog.Settings.Configuration` from `10.0.0` to `10.0.1`.
* Updated `System.Drawing.Common` from `10.0.8` to `10.0.9`.

### Maintenance

* Keeps the stable `1.0.x` line current with dependency maintenance only.
* No template source code, generated scaffold structure, package identity, or documented runtime behavior changes are included in this patch release.

## 1.0.2 - 2026-06-09

### Changed

* Updated application and package metadata for the stable `1.0.x` line.
* Refreshed release documentation and package validation guidance.
* Updated README release references and template installation examples.

### Maintenance

* Prepared the repository for the `1.0.2` package release.
* No generated scaffold behavior changes are included in this release.
