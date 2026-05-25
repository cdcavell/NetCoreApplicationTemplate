# Maintainers

NetCoreApplicationTemplate currently operates under a solo-maintainer model.

## Current Maintainer

| Maintainer | Role |
|:---|:---|
| [@cdcavell](https://github.com/cdcavell) | Repository owner, release owner, package publishing owner, documentation owner |

## Maintainer Responsibilities

The maintainer is responsible for:

- Reviewing and merging pull requests.
- Maintaining branch protection expectations for `main`.
- Reviewing workflow, security, release, and template packaging changes.
- Approving package publication through protected GitHub environments.
- Managing NuGet package identity and publish credentials.
- Managing GitHub release and Zenodo archival sequencing.
- Reviewing vulnerability reports and coordinating security fixes.
- Keeping release, support, and contribution documentation current.
- Maintaining CODEOWNERS coverage and required Code Owner review for protected branches.
- Reviewing stale approval behavior when branch protection settings change.

## Release Cadence

This project does not promise a fixed release calendar.

Expected release behavior:

- Patch releases may be created for security fixes, packaging corrections, documentation-critical fixes, or small compatible improvements.
- Minor releases may be created for compatible template improvements, new supported options, or expanded documentation.
- Major releases may be created for breaking template behavior, supported framework changes, major packaging changes, or significant governance changes.
- Pre-1.0 releases remain preview-quality and may change more frequently.
- `v1.0.0` represents the first stable support baseline for consumers.

Release timing depends on issue readiness, CI health, package validation, documentation readiness, and maintainer availability.

## Publishing Ownership

Official release artifacts are published only by the maintainer-controlled release workflow.

Publishing ownership includes:

- NuGet package publication for `CDCavell.NetCoreApplicationTemplate`.
- GitHub Releases and release notes.
- Zenodo archival metadata and DOI-bearing GitHub releases.
- GitHub Container Registry publication when container releases are used.
- Release checklist approval documented in [RELEASE.md](RELEASE.md).

External contributors are not expected to sign or publish package artifacts. If NuGet package signing is introduced later, signing should use a project-controlled signing certificate and protected release workflow.

## Branch Protection Expectations

The `main` branch is the stable integration branch and should be protected.

Expected `main` branch controls include:

- Changes flow through pull requests instead of direct pushes.
- Pull requests targeting `main` require Code Owner review when owned paths are changed.
- Stale pull request approvals are dismissed when new reviewable commits are pushed.
- Required status checks must pass before merge.
- Branches should be current enough to merge cleanly.
- Linear history or squash/rebase merge strategy should be preserved according to repository settings.
- Administrative bypass should be avoided for normal development.
- Workflow, release, security, package, and governance changes should receive deliberate maintainer review.
- General required approvals are intentionally not enabled while the repository operates under a solo-maintainer model.
- Required pull request approvals may be enabled later when additional maintainers are added.
- Required approval of the most recent reviewable push is not currently enabled.

## Adding Maintainers

Additional maintainers should not be added casually. Before expanding maintainership, review:

- Repository permissions.
- Branch protection rules.
- Environment protection rules.
- NuGet and GitHub Packages publishing permissions.
- Secret access.
- CODEOWNERS coverage.
- Security reporting and release ownership expectations.

Update this file when maintainer ownership changes.
