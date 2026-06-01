# Release Checklist and v1.0.0 Release Candidate Runbook

Use this checklist before publishing the stable `v1.0.0` template package and associated release artifacts.

## v1.0.0 release candidate runbook

Use this runbook before creating the stable `v1.0.0` tag or any future stable release tag that publishes public artifacts.

### 1. Release branch preparation

&emsp;**1.** Confirm `main` is green in CI before creating the release branch.<br />
&emsp;**2.** Create a release branch from the current `main` head.<br />

```powershell
    git checkout main
    git pull
    git checkout -b release/v1.0.0
```
&emsp;**3.** Confirm all planned release-readiness issues are merged before opening the release PR.<br />
&emsp;**4.** Avoid new feature work on the release branch unless it directly addresses release validation, documentation, packaging, or blocking defects.<br />
&emsp;**5.** Use a release-candidate tag, dry-run workflow, or pre-release GitHub Release before creating the stable DOI-bearing release when release automation, NuGet publication, Zenodo metadata, signing, SBOM generation, or documentation publication has changed since the previous release.<br />

### 2. Release gate validation

Complete these checks before tagging a stable release:
- Confirm CI passes on the release branch.
- Confirm CodeQL/security scanning passes.
- Confirm dependency/audit scanning has no unresolved release-blocking findings.
- Run `./scripts/Validate-VersionConsistency.ps1`.
- Confirm the version in `Directory.Build.props`, `NetCoreApplicationTemplate.Template.csproj`, `CITATION.cff`, README examples, package documentation, and the latest `CHANGELOG.md` heading is aligned.
- Confirm tag/version agreement checks added for release drift prevention are part of the release gate.
- Run the release build quality commands documented in `docs/articles/build-quality.md`.
- Run the template smoke-test workflow against the release branch or release-candidate tag.
- Confirm scaffolded output matches `eng/scaffold-manifest.default.json`.
- Confirm scaffolded output contains no maintainer-only files.
- Confirm the template package ID remains CDCavell.NetCoreApplicationTemplate.

### 3. Artifact confirmation

Before approving publication, review generated artifacts from the release workflow:
- `.nupkg`
- `.snupkg`, if produced
- Container image digest, if containers are published
- SBOM, if produced
- Provenance or attestation file, if produced
- Checksum files, if produced
- Package metadata
- Generated release notes
- GitHub Actions artifacts
- Generated documentation artifacts
- DOI / Zenodo archive record, when available

Confirm artifact names, versions, repository URLs, license metadata, authorship metadata, and package descriptions are accurate before publication.

### 4. Package signing policy confirmation

Before publication, confirm one of the following is true:
- Package signing remains intentionally deferred for this release.
- A project-controlled signing certificate, timestamping approach, and signing policy are configured and documented.

External contributors are not expected to sign release packages. Official packages should be produced only by the maintainer-controlled release workflow.

### 5. GitHub Release, Zenodo, and documentation validation

After creating the release candidate or stable release:

&emsp;**1.** Confirm the GitHub Release points to the expected tag.<br />
&emsp;**2.** Confirm attached artifacts are present and versioned correctly.<br />
&emsp;**3.** Confirm Zenodo receives the intended release only after integration is enabled.<br />
&emsp;**4.** Confirm Zenodo metadata matches `CITATION.cff` and `.zenodo.json`.<br />
&emsp;**5.** Confirm the DocFX documentation workflow completes.<br />
&emsp;**6.** Confirm the published GitHub Pages documentation loads successfully.<br />    
&emsp;**7.** Confirm release badges and documentation links in README resolve correctly.<br />
&emsp;**8.** Confirm the v1.0 migration guide is current.<br />
&emsp;**9.** Confirm the production deployment checklist is current.<br />
&emsp;**10.** Confirm release notes link to the migration guide and production deployment checklist.<br />

### 6. Clean-environment post-release validation

After package publication, validate the published package from a clean environment rather than relying only on the repository workspace.

Recommended smoke test:
```powershell
mkdir C:\Temp\NetCoreApplicationTemplate-v1-smoke
cd C:\Temp\NetCoreApplicationTemplate-v1-smoke

dotnet new uninstall CDCavell.NetCoreApplicationTemplate
dotnet new install CDCavell.NetCoreApplicationTemplate

dotnet new netcoreapp-template -n SmokeTestApp
cd SmokeTestApp

dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

Then confirm:

- The generated solution builds cleanly.
- Tests pass.
- No maintainer-only files are scaffolded.
- README/package usage instructions match the published package behavior.
- The generated app can be started locally using documented instructions.

### 7. Container smoke validation

If containers are published for v1.0.0, validate them after publication:

- Confirm image tags match the release version.
- Confirm image digest/checksum information is available where applicable.
- Pull the published image from a clean environment.
- Start the container using documented configuration.
- Confirm the health endpoint or root smoke check responds as expected.
- Confirm logs do not expose secrets or release-only sensitive values.

Skip this section only when no container image is published for the release.

### 8. Rollback and hotfix guidance

Stable public artifacts should not be overwritten after publication.

Use this guidance for critical post-release issues:

- If the release fails before public publication, fix the workflow or release branch and rerun validation.
- If the GitHub Release is created but no downstream public artifact has been published, correct the release or recreate the tag only if it will not confuse consumers.
- If a NuGet package is published with incorrect metadata or behavior, publish a corrected patch version.
- If Zenodo metadata is incorrect before the DOI-bearing stable release, correct repository metadata before creating the stable release.
- If Zenodo has already archived the stable release, prefer a follow-up corrective release rather than rewriting history.
- If documentation is incorrect but package artifacts are valid, patch the documentation and note the correction in the next release notes when appropriate.
- If a security issue is discovered after release, create a hotfix branch from the release tag, apply the minimum safe fix, run the full release gate, and publish a patch release.

## NuGet package identity and publish gate

The stable package identity is:

```text
CDCavell.NetCoreApplicationTemplate
```

Confirm package identity ownership and publish access.

Recommended order:

1. Configure the repository secret `NUGET_API_KEY` with the least privilege available for package publication.
2. Configure the `template-package-publish` GitHub environment with required reviewers.
3. Run the `Publish Template Package` workflow manually with `skip_publish` enabled to confirm packing and artifact generation.
4. Run a dry-run publication path against GitHub Packages or another safe feed.
5. Review the generated `.nupkg` metadata before publishing to NuGet.org.
6. Publish the first NuGet.org package only after manual approval.

## Package signing and contributor trust

NuGet package signing is deferred for `v1.0.0` unless a repository signing certificate, signing owner, timestamping approach, and signing policy are added before release.

External contributors are not expected to sign NuGet package artifacts. Official packages are produced only by the repository owner / maintainer-controlled release workflow. If package signing is introduced later, generated `.nupkg` artifacts should be signed by a project-controlled release certificate, not by individual contributors.

External contributions must enter through pull requests, pass required CI checks, and receive maintainer review before merge. Verified signed commits are encouraged, but package publication remains restricted to the protected release workflow.

The publish workflow keeps the manual approval gate in place so unsigned package publication remains intentional rather than automatic.

Before each stable release, confirm that [`SECURITY.md`](SECURITY.md) still reflects the current package-signing posture and that any trigger condition requiring a signing-policy review has been evaluated.

## Zenodo archival sequence

Zenodo archives GitHub releases created after the GitHub integration is enabled. Enable integration before the DOI-bearing release.

Recommended order:

1. Enable the GitHub repository in Zenodo.
2. Confirm `CITATION.cff` and `.zenodo.json` metadata are current.
3. Create a dry-run release tag after Zenodo integration is enabled.
4. Review the generated Zenodo DOI record and metadata.
5. Correct `CITATION.cff` or `.zenodo.json` if the DOI record is incomplete or misleading.
6. Create the stable `v1.0.0` release only after the dry-run DOI metadata is accepted.
7. Add the Zenodo DOI badge and copyable citation block to README after the DOI is available.

## Final release order

Recommended stable release order:

1. Merge release-readiness work to `main`.
2. Confirm CI, CodeQL, template smoke tests, scaffold manifest validation, documentation build, version consistency validation, and package workflow dry-run pass.
3. Confirm NuGet package identity reservation or documented registry decision.
4. Confirm package signing remains deferred or verify the configured signing certificate and signing policy.
5. Confirm Zenodo integration is enabled.
6. Create a dry-run release tag and verify NuGet/Zenodo outputs.
7. Correct metadata if necessary.
8. Tag `v1.0.0`.
9. Confirm tag-triggered version consistency validation passes before approving protected publish environments.
10. Approve protected publish environments only after reviewing generated artifacts.
11. Confirm or update README with final NuGet install command, Zenodo DOI badge, and copyable citation block.

## Rollback notes

- If NuGet publication fails before package upload, fix the workflow or secret and rerun.
- If a package is published with incorrect metadata, publish a corrected patch version rather than overwriting history.
- If Zenodo metadata is wrong, update repository metadata before creating the stable DOI-bearing release.
- If the release tag is wrong, prefer a corrected follow-up tag unless no public artifacts were created.

## Version source of truth

Before publishing a stable release, confirm every public version marker agrees with the intended release version.

| Surface | Source / location | Expected v1.0.0 behavior |
|---|---|---|
| Assembly/package version | `Directory.Build.props` | `VersionPrefix`, `AssemblyVersion`, and `FileVersion` match the release version. |
| Template package metadata | Template package project file | Package metadata resolves to the same version as the release tag. |
| Git tag | GitHub release tag | Stable releases use `vMAJOR.MINOR.PATCH`, for example `v1.0.0`. |
| NuGet package | Published `.nupkg` metadata | Package version matches the Git tag without the leading `v`. |
| Container image | Published image tags | Version tag matches the Git tag without the leading `v`; digest is recorded. |
| Changelog | `CHANGELOG.md` | Latest heading matches the release version and date. |
| Citation metadata | `CITATION.cff` and `.zenodo.json` | Metadata reflects the stable release and DOI/archive expectations. |
| GitHub Release | Release title/body/assets | Release notes, attached assets, and links match the release tag. |
| Documentation | README and DocFX pages | Install commands, badges, release links, and citation guidance match the published release. |

## Changelog and release note process

Before creating a stable release:

1. Add a new `CHANGELOG.md` section above the previous release.
2. Use the heading format `## MAJOR.MINOR.PATCH - YYYY-MM-DD`.
3. Group entries under headings such as `Added`, `Changed`, `Fixed`, and `Security`.
4. Confirm the changelog includes user-facing package, container, documentation, metadata, and security-governance changes.
5. Use the changelog section as the starting point for GitHub Release notes.
6. Add links from the GitHub Release notes to the migration guide, production deployment checklist, package page, documentation site, and DOI/archive record when available.
7. Do not describe unpublished artifacts as available until they have been validated after publication.

## v1.0.0 blocker issue confirmation

Before tagging `v1.0.0`, confirm all release-blocking issues tracked from completed parent issue #138 and final closure tracker #271 are closed, merged, or explicitly deferred with maintainer approval.

Minimum confirmation:

- Parent issue #138 has no unresolved v1.0.0 blockers.
- Final closure tracker #271 and its linked release-hardening issues are closed or explicitly deferred.
- Deferred items do not affect package correctness, publication safety, security posture, citation metadata, or consumer smoke-test behavior.
- The final release PR references #138 and #271 and summarizes the remaining release risk, if any.

## Maintainer review points for first public publication

The first stable package and first stable container publication require explicit maintainer review before protected publish environments are approved.

Review before NuGet publication:

- Package ID is correct.
- Version is correct.
- README/package README render correctly.
- Repository URL, license, authorship, tags, and description are correct.
- The `.nupkg` installs successfully in a clean environment.
- No maintainer-only files are included in scaffolded output.

Review before container publication:

- Image tags are correct.
- Image digest is captured.
- SBOM and provenance/attestation evidence are attached when produced.
- Vulnerability scan results are reviewed.
- Runtime smoke test passes.
- Logs do not expose secrets or release-only sensitive values.
