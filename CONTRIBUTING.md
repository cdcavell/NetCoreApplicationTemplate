# Contributing

Thank you for helping improve the .NET Core Application Template.

This project is intended to remain a clean, reusable, production-oriented ASP.NET Core baseline. Contributions should preserve that goal by keeping changes focused, documented, and easy to review.

## Contribution Principles

Contributions should favor:

- Secure-by-default behavior.
- Clear middleware and startup organization.
- Maintainable configuration patterns.
- Strong validation and test coverage for runtime behavior.
- Documentation for decisions that affect future template users.
- Small pull requests with a clear issue relationship.

Avoid mixing unrelated work in a single pull request. Documentation-only, dependency-only, test-only, and runtime behavior changes should usually be kept separate.

## Issue Workflow

The project backlog uses the following general flow:

| Status | Meaning |
|:---|:---|
| `Backlog` | Idea or task has been captured but is not ready for active work. |
| `Ready` | Issue is understood well enough to begin. |
| `In Progress` | Work has started on a branch. |
| `Blocked` | Work cannot continue until a decision, dependency, or fix is available. |
| `In Review` | Pull request has been opened and is waiting for validation or review. |
| `Done` | Pull request has been merged or the issue has otherwise been completed. |

Recommended flow:

1. Confirm the issue is still relevant.
2. Move or treat the issue as `Ready` before starting work.
3. Create a branch from the current `main` branch.
4. Keep commits focused on the issue.
5. Open a pull request when the work is ready for review.
6. Include `Closes #<issue-number>` when the pull request fully completes the issue.
7. Merge only after validation passes and the scope is reviewed.

If work is exploratory or only partially addresses an issue, use `Related to #<issue-number>` instead of a closing keyword.

## Branch Naming

Use short branch names that include the issue number and the type of work.

Recommended patterns:

```text
feature/issue-<issue-number>-short-description
fix/issue-<issue-number>-short-description
docs/issue-<issue-number>-short-description
refactor/issue-<issue-number>-short-description
test/issue-<issue-number>-short-description
chore/issue-<issue-number>-short-description
```

Examples:

```text
feature/issue-42-add-sample-endpoint
fix/issue-43-correct-header-order
docs/issue-44-update-data-access-docs
refactor/issue-45-simplify-options-validation
test/issue-46-add-rate-limit-tests
chore/issue-47-update-dependencies
```

Use `docs/` for documentation-only changes and `chore/` for maintenance work such as dependency or workflow updates.

## Commit Messages

Use concise imperative commit messages.

Recommended examples:

```text
Add application README scaffold #12
Configure Serilog request logging #3
Implement security header middleware #7
Add EF Core SQLite provider #19
Document deployment guidance #38
Add Dependabot configuration #56
```

Guidelines:

- Start with a verb such as `Add`, `Fix`, `Update`, `Document`, `Configure`, or `Refactor`.
- Include the issue number when practical.
- Keep the subject line focused on the actual change.
- Avoid vague messages such as `misc fixes` or `updates`.

## Pull Request Expectations

Pull requests should include:

- A short summary of the change.
- A validation or testing section.
- A closing issue reference when the work completes an issue.
- Notes about behavior changes, migration steps, or deployment impact when relevant.

Recommended pull request structure:

```markdown
## Summary

- Added ...
- Updated ...
- Documented ...

## Validation

- Ran `dotnet build --configuration Release`.
- Ran `dotnet test --configuration Release`.

Closes #<issue-number>
```

Documentation-only changes may use a lighter validation note:

```markdown
## Validation

- Documentation-only change.
- Reviewed links and DocFX navigation scope.
```

## Validation Expectations

For runtime code changes, run:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

For formatting-sensitive changes, run:

```powershell
dotnet format --verify-no-changes --verbosity minimal
```

For documentation changes, validate that:

- New pages are included in DocFX navigation when needed.
- Links work in the published documentation structure.
- Examples do not include production secrets.
- Runtime behavior is not changed unintentionally.

## Documentation Expectations

Update documentation when a change affects:

- Configuration behavior.
- Middleware ordering.
- Security defaults.
- Authentication or authorization behavior.
- Data access setup.
- Deployment expectations.
- GitHub workflow or release process.
- Template packaging behavior.

Long-term architectural decisions should be captured in an Architecture Decision Record under `docs/adr`.

## Dependency Update Expectations

Dependency update pull requests should be reviewed like other pull requests.

Before merging dependency updates:

- Confirm CI passes.
- Review release notes for major updates.
- Treat security updates as higher priority.
- Be careful with authentication, EF Core, middleware, logging, telemetry, and GitHub Actions updates.
- Prefer separate review for major updates that may affect runtime behavior.

## Branch Cleanup

After a pull request is merged, delete the remote branch unless it is intentionally kept for follow-up work.

For local cleanup:

```powershell
git fetch --prune
git branch --merged
```

Delete stale local branches only after confirming the work has been merged or is no longer needed.

## Security Notes

Do not commit:

- Production connection strings.
- Client secrets.
- API keys.
- Signing keys or certificates.
- Private endpoint values.
- Generated secrets or tokens.

Use environment variables, user secrets, or approved secret stores for sensitive values.

## Solo-Maintainer Hardening Profile

This repository operates a solo-maintainer hardening profile through v1.x.

While the project has a single maintainer and no external contributors, the repository relies on protected branches, pull requests, CI validation, CodeQL, Dependency Review, Dependabot, explicit workflow permissions, and documented release/security procedures.

The following controls should be enabled before merging the first external pull request or before adding any repository collaborator:

- Signed commit requirement on protected branches.
- Required pull request reviewer approval.
- Code Owners review for governance files, workflow files, release files, security policy files, and template packaging files.
- Review of branch protection settings for `main` and any long-lived development branch.
- Review of repository secrets, environment secrets, and package publishing permissions.

Trigger conditions for moving beyond the solo-maintainer profile include:

- First external pull request.
- First repository collaborator.
- First automated package publishing workflow.
- First automated container image publishing workflow.
- A confirmed credential exposure.
- Repeated high-risk Dependabot, CodeQL, or Dependency Review findings.

## GitHub Actions Supply-Chain Policy

GitHub Actions workflows should follow these rules:

- Use explicit workflow or job-level `permissions`.
- Default to `contents: read` unless a job requires additional permissions.
- Do not place plaintext credentials in workflow files.
- Use repository or environment secrets for publish credentials.
- Pin third-party and first-party actions to full commit SHAs when practical.
- Preserve a version comment beside each pinned action SHA for maintainability.
- Let Dependabot monitor GitHub Actions updates.
- Review action updates before merging, especially actions with write permissions, release permissions, package publishing permissions, or identity-token permissions.

## Related Documentation

- [GitHub Workflow](docs/articles/github-workflow.md)
- [Configuration](docs/articles/configuration.md)
- [Deployment Notes](docs/articles/deployment.md)
- [Architecture Decision Records](docs/adr/index.md)
