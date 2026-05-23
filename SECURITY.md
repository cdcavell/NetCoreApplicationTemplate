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
- Whether the issue affects default template behavior or only a specific consuming application configuration.

## Disclosure Expectations

Please allow reasonable time for review and remediation before publicly discussing a suspected vulnerability.

This project is a reusable template. Security reports should distinguish between:

- Issues in the template's default behavior.
- Issues introduced by a consuming application's custom configuration or deployment environment.
- General dependency vulnerabilities already tracked by upstream packages or GitHub alerts.

## Security Scope

Areas especially relevant to this template include:

- Authentication and authorization configuration.
- Security headers.
- Forwarded header handling.
- Rate limiting.
- Error handling and Problem Details responses.
- Data access configuration.
- Secret handling and configuration examples.
- GitHub Actions workflow behavior.

Do not include production secrets, private keys, tokens, passwords, or sensitive personal data in a report.
