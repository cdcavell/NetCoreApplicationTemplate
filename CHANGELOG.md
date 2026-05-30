# Changelog

All notable changes to this project are documented in this file.

This project follows Semantic Versioning using the format `MAJOR.MINOR.PATCH`.

## 0.5.8 - 2026-05-30

### Added

* Added improved CI coverage report generation for the current `ProjectTemplate.*` assemblies.
* Added coverage report filtering to exclude test assemblies, build output, generated `.g.cs` files, `bin`, and `obj` content.
* Added generated ReportGenerator HTML coverage output to the published DocFX documentation site under `/coverage`.
* Added direct hosted coverage report links from the root `README.md` and `PACKAGE-README.md`.
* Added bounded persisted string canonicalization before EF Core save operations.
* Added normalization for added entity string values and modified string properties before auditing, concurrency stamping, and persistence.
* Added static raw SQL safety audit coverage to detect unsafe raw SQL APIs and manual command construction patterns.
* Added normalized lookup columns for external login provider name and email values.
* Added persisted string lookup tests for whitespace trimming, provider-name casing, provider-user-id case sensitivity, and Unicode Form C normalization.
* Added UTC timestamp persistence conventions for system and audit timestamps.
* Added millisecond precision normalization for `*Utc` timestamp values before save.
* Added EF Core timestamp precision configuration for UTC timestamp properties.
* Added timestamp persistence tests for external login and audit records.
* Added model validation coverage for persisted decimal precision and scale configuration.
* Added EF Core migration coverage for migration `Up` / `Down` operations, migration target model metadata, and the current `ApplicationDbContext` model snapshot.

### Changed

* Updated coverage threshold enforcement to read the expected `Cobertura.xml` report directly.
* Updated documentation publishing to copy the generated coverage report into `docs/_site/coverage`.
* Updated README coverage badge behavior so the visible badge links directly to the published test coverage report.
* Updated external login lookup behavior to use normalized provider-name comparison while preserving case-sensitive provider user IDs.
* Documented persisted string comparison rules and SQLite / SQL Server collation expectations.
* Documented the distinction between persisted string canonicalization, EF Core parameterization, and context-specific output encoding.
* Documented timestamp persistence versus display-time conversion guidance for SQLite and SQL Server.
* Documented recommended decimal precision and scale patterns for future template consumers.
* Clarified provider differences between SQL Server and SQLite decimal behavior.

### Fixed

* Fixed CI coverage assembly filtering that still referenced the older `Template.Web` naming pattern.
* Fixed coverage report generation so it reports usable application assembly coverage instead of noisy or empty coverage output.
* Fixed documentation workflow coverage publishing so the generated report is available through the public DocFX site.
* Fixed PowerShell-based multi-line coverage commands so they run under `pwsh` during workflow execution.

### Security

* Strengthened persistence defense-in-depth by canonicalizing stored string values before EF Core persistence.
* Strengthened raw SQL safety guardrails with static test coverage against unsafe raw SQL usage patterns.
* Clarified that SQL injection protection remains based on EF Core parameterization and safe query construction, not string canonicalization alone.
* Clarified that output encoding remains context-specific and is not replaced by persistence normalization.


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
