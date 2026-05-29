# Changelog

All notable changes to this project are documented in this file.

This project follows Semantic Versioning using the format `MAJOR.MINOR.PATCH`.

## 0.5.7 - 2026-05-29

### Added

* Added Infrastructure-owned data access registration via `AddApplicationInfrastructureDataAccess(...)`.
* Added non-web dependency injection support for resolving `ApplicationDbContext` and `IDbContextFactory<ApplicationDbContext>` from the Infrastructure layer.
* Added `SystemCurrentActorAccessor` for non-HTTP audit contexts.
* Added explicit `none` / disabled data access provider support for generated applications that do not need EF Core registration.
* Added generated README and package README examples for default scaffolds, `--authProvider none`, `--dbProvider sqlserver`, and combined non-default template variants.
* Added additional consumer-facing links from package documentation to the repository, published docs, template-packaging guidance, changelog, license, and releases.

### Changed

* Moved shared EF Core setup into the Infrastructure layer while keeping Web-specific HTTP actor wiring in the Web project.
* Updated disabled data access behavior to skip EF Core registration and avoid connection-string resolution when data access is disabled.
* Updated v1.0 readiness documentation, DocFX navigation, migration guidance, and public documentation surfaces for clearer discoverability.
* Updated package and documentation references from `0.5.6` to `0.5.7`.
* Updated OpenTelemetry service version handling to resolve from assembly metadata when no explicit configuration value is provided.

### Fixed

* Fixed `--dbProvider none` template behavior so the generated configuration maps to `ProjectTemplate:DataAccess:Provider = None`.
* Removed stale hardcoded OpenTelemetry `ServiceVersion` values from shipped `appsettings.json` files.
* Preserved stable NuGet install guidance while keeping versioned local `.nupkg` install examples aligned with `0.5.7` validation.

## 0.5.6 - 2026-05-28

### Added

* Added optimistic concurrency handling for EF Core entities that inherit from `DataEntity`.
* Added a provider-safe `ConcurrencyStamp` concurrency token for SQLite-oriented local development and SQL Server-oriented production paths.
* Added EF Core migration support for the shared data-entity concurrency stamp.
* Added tests proving stale entity updates are detected for both synchronous and asynchronous save paths.
* Added scaffold validation coverage for generated template output, including default application configuration and NuGet configuration.

### Changed

* Updated `ApplicationDbContext.SaveChanges` and `SaveChangesAsync` to refresh concurrency stamps during modified entity saves.
* Updated concurrency conflict behavior to surface diagnosable `DbUpdateConcurrencyException` failures rather than allowing silent last-writer-wins overwrites.
* Updated data-access, template-packaging, telemetry, release, and public-surface documentation for the 0.5.6 release.
* Updated test platform package references to the latest 18.6.0 test SDK/platform packages.

### Fixed

* Improved generated-template completeness by ensuring expected scaffold content is included and verified.

## 0.5.5 - 2026-05-26

### Added

- Added explicit package signing policy guidance for release governance.
- Added startup validation for application authorization configuration.
- Added User Secrets support for the generated web project to support local development secret management.
- Added configuration validation guidance for v1.0 readiness.

### Changed

- Clarified that NuGet package signing is currently deferred until a project-controlled certificate, timestamping approach, signing owner, and verification policy are documented.
- Clarified that external contributors do not publish or sign release packages directly.
- Updated configuration documentation to distinguish local development secrets from production secrets.
- Cross-linked production deployment guidance with startup validation and secret-management expectations.

### Security

- Strengthened supply-chain and release-governance documentation around package publication, maintainer approval, protected release workflows, and future signing-policy trigger conditions.
- Strengthened secure-by-default configuration behavior by failing startup when required authorization settings are invalid.

## 0.5.4 - 2026-05-25

### Fixed

- Bumped the template package version metadata to `0.5.4`.
- Added validation to ensure the generated `.nupkg` version matches the Git release tag.
- Prevented stale package metadata from silently republishing an older NuGet package during a new release.

### Changed

- Continued release pipeline hardening for template package publishing, container publishing, image signing, SBOM generation, vulnerability scan evidence, and provenance attestation.

## 0.5.3 - 2026-05-25

### Fixed

- Fixed container release evidence publishing when the GitHub release already exists.
- Updated release evidence upload logic to use explicit GitHub CLI repository context and `$LASTEXITCODE` checks.
- Preserved signed container image publishing, SBOM, vulnerability scan, and provenance attestation behavior.

### Notes

- No new NuGet package was published for this release because the template package metadata still resolved to `0.5.2`.
- The latest NuGet package after this release remained `0.5.2`.

## 0.5.2 - 2026-05-25

### Fixed

- Published the NuGet template package for the `0.5.x` release stream.
- Added explicit GitHub repository context for container release evidence publishing.
- Fixed release evidence publishing when GitHub CLI commands run outside a checked-out repository.

## 0.5.1 - 2026-05-25

### Fixed

- Corrected pinned GitHub Actions references used by release publishing workflows.
- Added the Zenodo Concept DOI badge to the README.
- Updated README release metadata for the `0.5.x` release stream.

## 0.5.0 - 2026-05-25

### Added

- Added Zenodo archival metadata and DOI support for repository citation.
- Added release publishing support for the template package.
- Added container publishing, signing, SBOM, vulnerability scan, and provenance evidence workflow support.

### Changed

- Updated package and release metadata for `0.5.0`.
- Prepared the repository for DOI-bearing archived releases and NuGet package distribution.

## 0.4.2 - 2026-05-23

### Added

- Added repository governance files and issue / pull request templates.
- Added a default Razor Pages landing page.
- Added an application preview image for the README.

### Changed

- Expanded the Project Structure documentation into a fuller architecture overview.
- Strengthened README positioning, repository governance, and citation guidance.
- Updated project version metadata for `0.4.2`.

## 0.4.1 - 2026-05-23

### Changed

- Updated the README latest release section after publishing `v0.4.1`.

## 0.4.0 - 2026-05-21

### Changed

- Continued release-readiness work for the reusable ASP.NET Core application template.
- Kept repository metadata, documentation, and release tracking aligned with the public release stream.

## Earlier Releases

Earlier release notes remain available from the GitHub Releases page.

Future release sections should be added above the previous release and grouped under headings such as `Added`, `Changed`, `Fixed`, and `Security` when applicable.
