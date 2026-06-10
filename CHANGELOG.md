# Changelog

All notable changes to this project are documented in this file.

This project follows Semantic Versioning using the format `MAJOR.MINOR.PATCH`.

## 1.0.2 - 2026-06-09

### Changed

* Updated pinned GitHub Actions workflow dependencies used by CI, CodeQL analysis, documentation publishing, release automation, version consistency validation, container publishing, and template package publishing.
* Updated `actions/checkout` from `6.0.2` to `6.0.3`.
* Updated `actions/setup-dotnet` from `5.2.0` to `5.3.0`.
* Updated `github/codeql-action` from `4.36.0` to `4.36.2`.

### Maintenance

* Normalized GitHub workflow file formatting after dependency updates without changing template source code, generated scaffold behavior, package metadata, or runtime application behavior.


## 1.0.1 - 2026-06-01

### Fixed

* Add the Infrastructure project to template primaryOutputs so Visual Studio loads all generated projects when creating a new solution from the package.

## 1.0.0 - 2026-06-01

### Added

* Added the first stable `1.0.0` release baseline for the NetCoreApplicationTemplate package and repository.
* Added final `v1.0.0` release-readiness closure tracking for release validation, documentation and metadata consistency, supply-chain/security evidence, consumer package validation, and intentional deferral review.
* Added final release-candidate validation coverage for build, format, test, pack, coverage-report generation, and coverage-gate confirmation.
* Added stable-release validation expectations for clean template installation, scaffolded project restore, build, test, and consumer documentation review.

### Changed

* Promoted the project from the `0.5.x` hardening stream to the stable `1.0.0` release line.
* Consolidated the broad `v1.0.0` readiness epic into a concise final closure issue set so remaining release work is visible from one tracking view.
* Updated release readiness posture around version consistency, package metadata, release notes, documentation links, citation metadata, and release artifact validation.
* Confirmed NuGet package author signing remains intentionally deferred while the repository is solo-maintained, with official packages produced only through the maintainer-controlled release workflow.
* Confirmed stable release guidance for GitHub Release publication, NuGet package validation, Zenodo archival sequencing, DocFX documentation publication, and clean-environment post-release smoke testing.

### Security

* Carried forward the global coverage gate and security-critical per-file coverage gate as part of the stable release baseline.
* Preserved security-critical coverage expectations for centralized Problem Details handling, request classification, actor resolution, security headers, forwarded headers, rate limiting, persistence normalization, timestamp handling, and `ApplicationDbContext` save hooks.
* Preserved release-governance controls for dependency review, vulnerability scanning, SBOM/provenance evidence, protected publish environments, and manual maintainer approval.
* Preserved the release-blocking rule that any deferred work must not affect package correctness, publication safety, security posture, citation metadata, or consumer smoke-test behavior.

### Validation

* Locally validated Release build, formatting, test execution, and template package generation before the stable release-candidate review.
* Confirmed the local coverage-gate verification path using ReportGenerator Cobertura output and the security-critical coverage threshold script.

## 0.5.9 - 2026-05-31

### Added

* Added `AGENTS.md` repository instructions for GitHub Copilot coding agents and other AI-assisted contributors.
* Added a security-critical coverage gate that evaluates ReportGenerator Cobertura output against file-level line and branch coverage thresholds.
* Added `eng/security-critical-coverage.json` to define protected source files and minimum security-critical coverage expectations.
* Added `eng/Assert-SecurityCriticalCoverage.ps1` to enforce security-critical coverage thresholds in CI.
* Added focused contract coverage for `ProblemDetailsExceptionHandler`, including exception-to-status/title mappings and safe production response behavior.
* Added Problem Details customization coverage for extension and request-classification behavior.
* Added branch coverage for `HttpContextCurrentActorAccessor`, including authenticated subject claims, authenticated name identifier fallback, whitespace handling, remote IP fallback, and unknown actor behavior.
* Added focused branch coverage for `ApplicationDbContext` save hooks, audit stamping, string normalization, timestamp normalization, direct audit-record saves, and concurrency-related persistence behavior.
* Added additional `ApplicationDbContext` branch-gap tests for auditing-disabled and save-pipeline scenarios.
* Added direct coverage for persistence normalization helpers, including timestamp precision trimming, UTC conversion, string comparison normalization, whitespace handling, and Unicode normalization behavior.
* Added enabled-path authentication provider coverage for OpenID Connect, SAML2, Google, and GitHub registration.
* Added provider option coverage for external authentication provider extensions, including null-argument validation, option binding, callback paths, scopes, and SAML2 provider configuration.
* Added branch coverage for API versioning configuration validation.
* Added branch coverage for OpenTelemetry registration, tracing, metrics, OTLP exporter configuration, and service-version behavior.
* Added branch coverage for forwarded header configuration and request logging behavior.
* Added README badge updates for NuGet total downloads, static Zenodo DOI display, and static MIT license presentation.
* Added documentation examples for an opt-in fallback authorization policy and named authorization policy usage.

### Changed

* Updated CI to enforce both the global coverage threshold and the new security-critical per-file coverage gate.
* Updated build-quality documentation to explain the security-critical coverage gate, protected-file configuration, and expectations for lowering protected thresholds.
* Hardened HTTP current actor resolution so authenticated users resolve from the `sub` claim first, then `ClaimTypes.NameIdentifier`, before falling back to remote IP or `Unknown`.
* Clarified that `/health/ready` provides the readiness endpoint shape but does not prove database, cache, queue, or external dependency readiness unless those checks are explicitly registered.
* Updated the production deployment checklist to require service-specific readiness dependency checks before normal production traffic is allowed.
* Clarified that fallback authorization is opt-in and should be reviewed carefully for public endpoints such as login, callback, health checks, static assets, and intentionally anonymous API endpoints.
* Clarified the NuGet package signing policy: packages are not author-signed while the repository remains solo-maintained and are published only through the maintainer-controlled release workflow.
* Clarified that published NuGet.org packages rely on NuGet.org repository signing rather than a separate project-managed author-signing certificate.

### Security

* Strengthened release-governance documentation around NuGet package publication, solo-maintainer ownership, protected release workflows, manual approval, and repository-signed NuGet.org package distribution.
* Strengthened CI regression protection for security-sensitive source files by adding file-level coverage requirements for error handling, request classification, actor resolution, security headers, forwarded headers, rate limiting, persistence normalization, timestamp handling, and `ApplicationDbContext` save hooks.
* Strengthened centralized error-handling confidence by pinning Problem Details mappings and safe production sanitization behavior with focused tests.
* Strengthened audit attribution behavior by verifying authenticated actor resolution and fallback behavior for HTTP request contexts.
* Strengthened authentication-provider confidence by covering enabled registration paths and provider option behavior for supported external authentication providers.
* Strengthened production readiness guidance by clarifying that readiness probes must include the dependency checks required by each deployed service.

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
