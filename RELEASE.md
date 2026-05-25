# Release Checklist

Use this checklist before publishing a stable template package or creating the future `v1.0.0` release.

## 1. Pre-release validation

- Confirm `main` is green in CI.
- Confirm the version in `Directory.Build.props`, `NetCoreApplicationTemplate.Template.csproj`, `CITATION.cff`, README examples, and package documentation is aligned.
- Run the release build quality commands documented in `docs/articles/build-quality.md`.
- Run the template smoke-test workflow on the release candidate branch or tag.
- Confirm the template package ID is still `CDCavell.NetCoreApplicationTemplate`.
- Confirm `docs/articles/public-surface-v1.md` reflects the final `v1.0.0` package identity, template symbols, generated structure, configuration keys, endpoint conventions, middleware ordering, and publishing conventions.
- Confirm `docs/articles/v1-upgrade-notes.md` is current before tagging `v1.0.0`.

## 2. NuGet package identity and publish gate

The intended stable package identity is:

```text
CDCavell.NetCoreApplicationTemplate
```

Reserve the package identity by publishing the first validated package to NuGet.org before the stable `v1.0.0` tag, or document a decision to use a different registry.

Recommended order:

1. Configure the repository secret `NUGET_API_KEY` with the least privilege available for package publication.
2. Configure the `template-package-publish` GitHub environment with required reviewers.
3. Run the `Publish Template Package` workflow manually with `skip_publish` enabled to confirm packing and artifact generation.
4. Run a dry-run publication path against GitHub Packages or another safe feed.
5. Review the generated `.nupkg` metadata before publishing to NuGet.org.
6. Publish the first NuGet.org package only after manual approval.

## 3. Package signing and contributor trust

NuGet package signing is deferred for `v1.0.0` unless a repository signing certificate, signing owner, timestamping approach, and signing policy are added before release.

External contributors are not expected to sign NuGet package artifacts. Official packages are produced only by the repository owner / maintainer-controlled release workflow. If package signing is introduced later, generated `.nupkg` artifacts should be signed by a project-controlled release certificate, not by individual contributors.

External contributions must enter through pull requests, pass required CI checks, and receive maintainer review before merge. Verified signed commits are encouraged, but package publication remains restricted to the protected release workflow.

The publish workflow keeps the manual approval gate in place so unsigned package publication remains intentional rather than automatic.

## 4. Zenodo archival sequence

Zenodo archives GitHub releases created after the GitHub integration is enabled. Enable integration before the DOI-bearing release.

Recommended order:

1. Enable the GitHub repository in Zenodo.
2. Confirm `CITATION.cff` and `.zenodo.json` metadata are current.
3. Create a dry-run release tag after Zenodo integration is enabled.
4. Review the generated Zenodo DOI record and metadata.
5. Correct `CITATION.cff` or `.zenodo.json` if the DOI record is incomplete or misleading.
6. Create the stable `v1.0.0` release only after the dry-run DOI metadata is accepted.
7. Add the Zenodo DOI badge and copyable citation block to README after the DOI is available.

## 5. Final release order

Recommended stable release order:

1. Merge release-readiness work to `main`.
2. Confirm CI, CodeQL, template smoke tests, documentation build, and package workflow dry-run pass.
3. Confirm NuGet package identity reservation or documented registry decision.
4. Confirm package signing remains deferred or verify the configured signing certificate and signing policy.
5. Confirm Zenodo integration is enabled.
6. Create a dry-run release tag and verify NuGet/Zenodo outputs.
7. Correct metadata if necessary.
8. Tag `v1.0.0`.
9. Approve protected publish environments only after reviewing generated artifacts.
10. Update README with final NuGet install command, Zenodo DOI badge, and copyable citation block.

## 6. Rollback notes

- If NuGet publication fails before package upload, fix the workflow or secret and rerun.
- If a package is published with incorrect metadata, publish a corrected patch version rather than overwriting history.
- If Zenodo metadata is wrong, update repository metadata before creating the stable DOI-bearing release.
- If the release tag is wrong, prefer a corrected follow-up tag unless no public artifacts were created.
