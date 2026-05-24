# Security Policy

## Supported Versions

Security fixes are applied to the current active development line unless otherwise noted in a release announcement.

| Version | Supported |
|:---|:---|
| Current release | Yes |
| Older releases | Best effort |

## Reporting a Vulnerability

Please report suspected security issues privately instead of opening a public GitHub issue.

Use the repository owner's GitHub profile contact options or GitHub's private vulnerability reporting feature if it is enabled for this repository.

When reporting a vulnerability, include:

- A clear description of the issue.
- Steps to reproduce the behavior.
- The affected version, branch, or commit if known.
- Any relevant logs, screenshots, configuration details, or proof-of-concept notes.
- Whether the issue affects default application behavior or only a specific consuming application configuration.

## Disclosure Expectations

Please allow reasonable time for review and remediation before publicly discussing a suspected vulnerability.

This project is a reusable application. Security reports should distinguish between:

- Issues in the application's default behavior.
- Issues introduced by a consuming application's custom configuration or deployment environment.
- General dependency vulnerabilities already tracked by upstream packages or GitHub alerts.

## Security Scope

Areas especially relevant to this application include:

- Authentication and authorization configuration.
- Security headers.
- Forwarded header handling.
- Rate limiting.
- Error handling and Problem Details responses.
- Data access configuration.
- Secret handling and configuration examples.
- GitHub Actions workflow behavior.

Do not include production secrets, private keys, tokens, passwords, or sensitive personal data in a report.

## Secrets Incident Response Playbook

If a credential, token, key, certificate, connection string, or other secret is suspected of being committed, disclosed, logged, or exposed through GitHub Actions:

1. Treat the secret as compromised immediately.
2. Revoke or rotate the secret at the issuing system before relying on repository cleanup.
3. Review recent GitHub Actions runs, repository events, package publishing events, and release activity for unexpected use.
4. Remove the exposed value from the current tree.
5. If the value exists in git history, rewrite history only after confirming the operational impact and coordinating protected branch updates.
6. Invalidate or replace any artifacts, packages, releases, or container images that may have been produced with the exposed secret.
7. Document whether the finding was a confirmed secret, a rotated secret, or a false positive.
8. Add or adjust preventive controls such as GitHub secret scanning, push protection, `.gitignore` rules, example configuration cleanup, or workflow permission tightening.
9. Avoid posting the exposed value in public issues, pull requests, commit messages, screenshots, or logs.

Secrets scan reports should be stored outside tracked source control, preferably under ignored local paths such as `artifacts/security/`.

## Repository Secrets and Publish Permissions

Repository and environment secrets must be scoped to the narrowest workflow that requires them.

| Secret or permission | Intended use | Scope expectation |
|:---|:---|:---|
| `GITHUB_TOKEN` | Built-in workflow token | Use explicit workflow or job-level permissions. Default to `contents: read` unless a job needs more. |
| `NUGET_API_KEY` | Future NuGet package publishing | Store only as a GitHub Actions secret or protected environment secret. Do not expose to pull requests from forks. Rotate after suspected exposure. |
| Package publishing permissions | Future package release workflow | Limit to release or manual publish workflows. Do not grant publish permissions to normal CI validation jobs. |
| Release permissions | Future GitHub release automation | Grant `contents: write` only to the workflow/job that creates or updates releases. |

Plaintext credentials are not allowed in workflow files, repository files, examples, documentation, screenshots, or committed logs.
