# Changelog

All notable changes to this project are documented in this file.

This project follows Semantic Versioning using the format `MAJOR.MINOR.PATCH`.

## 0.5.3 - 2026-05-25

### Fixed

- Fixed container release evidence publishing when the GitHub release already exists.
- Updated release evidence upload logic to use explicit GitHub CLI repository context and `$LASTEXITCODE` checks.
- Preserved signed container image publishing, SBOM, vulnerability scan, and provenance attestation behavior.

## 0.5.2 - 2026-05-25

### Fixed

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
- 
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
