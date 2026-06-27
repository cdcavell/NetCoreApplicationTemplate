# Production Authentication Hardening Checklist

The template provides authentication foundations and generated starter configuration, but production authentication remains provider-specific and environment-specific.

Treat this checklist as a required review step before deploying any generated application that enables cookie authentication, OpenID Connect, SAML2, Microsoft, Google, GitHub, or another external provider.

The template can provide safe defaults, startup validation, and clear extension points. It cannot decide real redirect URIs, provider trust boundaries, claim semantics, MFA policy, token storage policy, or secret-management policy for a consuming application.

## Baseline Principle

Generated authentication configuration is a starting point, not a production authorization model.

Before deployment, confirm:

- Every enabled provider has production-owned settings.
- Placeholder local-development values have been replaced.
- Environment-specific URLs match the externally visible host names.
- Secrets, certificates, and tokens are managed outside source control.
- Claims are translated into application-owned meanings before they drive authorization.
- Login, logout, session timeout, and MFA expectations are explicitly tested.

## HTTPS, Reverse Proxy, and Forwarded Headers

Review these items when the application is hosted behind a reverse proxy, ingress controller, load balancer, TLS terminator, or platform gateway.

- Confirm production traffic uses HTTPS end to end or through a trusted TLS termination boundary.
- Confirm forwarded-header middleware is configured for the trusted proxy network or known proxy addresses.
- Confirm the application sees the correct external scheme, host, and path base after forwarded-header processing.
- Confirm generated redirect URLs use the public HTTPS origin rather than an internal container, private host, or HTTP URL.
- Confirm login, callback, logout, and access-denied flows work through the same public URL users will use in production.
- Confirm security headers, HTTPS redirection, and authentication callbacks behave correctly behind the proxy.

## Redirect, Callback, ACS, and Logout URLs

Authentication flows are especially sensitive to URL mismatch.

For each enabled provider, confirm:

- OIDC redirect URIs are registered exactly with the identity provider.
- OAuth callback paths are registered exactly with the provider.
- SAML assertion consumer service (ACS) URLs match the deployed service provider metadata.
- Post-logout redirect URLs are explicitly allowed by the provider when the provider supports logout callback validation.
- Local development callback URLs are separate from production callback URLs.
- Wildcard redirect URI registration is avoided unless the provider and deployment model require it and the risk is understood.
- Return URLs accepted by the application remain local or otherwise strictly validated.
- Provider metadata, entity IDs, realm values, callback paths, and module paths are reviewed per environment.

## Cookie Security

Review the application cookie and any external sign-in cookies before production deployment.

- Confirm authentication cookies are marked `Secure` in production.
- Confirm cookies are `HttpOnly` unless a specific non-cookie authentication design requires otherwise.
- Confirm `SameSite` behavior is compatible with the selected provider flow.
- Confirm cookie names do not collide with other applications on the same parent domain.
- Confirm cookie domain and path settings do not expose authentication cookies more broadly than intended.
- Confirm sliding expiration, absolute expiration, and session timeout expectations are documented.
- Confirm logout clears the intended local cookie state.
- Confirm access-denied responses do not expose sensitive authorization details.

## Claims Translation and Normalization

External providers rarely agree on claim names, identifier semantics, group formats, or role formats. Do not wire raw external claims directly into application authorization without a translation strategy.

Before production, define an application-owned claims contract:

- Identify the stable subject claim that represents the user across sessions.
- Prefer provider-stable identifiers over display names, email addresses, or mutable usernames for user identity.
- Normalize provider-specific subject claims into an application-owned subject claim such as `application:subject`.
- Normalize display name, email, role, group, and permission claims into application-owned claim names before policy evaluation.
- Decide whether original provider claims should be preserved for diagnostics or removed after transformation.
- Normalize role and group casing, whitespace, prefixes, tenant identifiers, and provider-specific URI formats.
- Avoid assuming that email means identity proof unless the provider explicitly marks the email as verified and the application trusts that provider assurance.
- Map external groups to application roles deliberately instead of treating every external group as an application role.
- Document how claim conflicts are handled when multiple providers or tenants can authenticate the same user.
- Add tests for claims translation rules that affect authorization.

The template's claims transformation layer can normalize provider claims into application-owned names such as `application:subject`, `application:name`, `application:email`, `application:role`, `application:group`, and `application:permission`. Review [Authentication](authentication.md#claims-transformation-and-normalization) for the baseline behavior.

## Token, Secret, and Certificate Handling

Provider credentials and tokens are production secrets.

- Store client secrets, signing certificates, private keys, API tokens, and provider credentials in user secrets for local development or a production secret store for deployment.
- Do not commit real provider credentials, certificates, private keys, tokens, or production metadata secrets to source control.
- Confirm provider secrets can be rotated without rebuilding the application.
- Review whether tokens are saved at all. If `SaveTokens` is enabled, document why tokens need to be retained and where they are protected.
- Avoid persisting refresh tokens unless the application has a clear need, encryption strategy, retention policy, and revocation plan.
- Confirm token lifetime, cookie lifetime, refresh lifetime, and session timeout settings are aligned.
- Confirm logs do not include tokens, authorization codes, SAML assertions, cookies, client secrets, or sensitive query strings.
- Confirm certificate validation remains enabled for production SAML and metadata retrieval scenarios.

## Provider Metadata and Environment Configuration

Each deployment environment should have provider configuration that matches that environment.

- Confirm authority, metadata URL, issuer, audience, entity ID, and tenant settings are production-specific.
- Confirm discovery metadata is loaded from trusted provider endpoints.
- Confirm issuer, audience, signature, assertion, and certificate validation settings match provider guidance.
- Confirm development, test, staging, and production providers are separated when possible.
- Confirm disabled providers can keep placeholders without failing startup, while enabled providers fail fast if required values are missing.
- Confirm provider-specific app registrations only include the redirect/callback/logout URLs that are required.
- Confirm stale app registrations, old secrets, and unused callback URLs are removed from provider portals.

## Login, Logout, Session, and MFA Behavior

Authentication is not production-ready until the full user session lifecycle is tested.

- Confirm login challenge starts the expected provider flow.
- Confirm callback handling succeeds through the deployed public URL.
- Confirm access-denied behavior is safe and understandable.
- Confirm local logout clears the local application session.
- Confirm provider logout behavior is understood. Some providers require explicit federated logout configuration; others only clear the local application cookie.
- Confirm session timeout behavior matches business and security requirements.
- Confirm concurrent session expectations are documented if the application needs them.
- Confirm account-disabled, user-removed, group-removed, and role-changed scenarios are handled according to the application's risk model.
- Confirm MFA or step-up authentication expectations are owned by the external provider when the application delegates those controls.
- Confirm high-risk actions have a step-up strategy if the business process requires fresh MFA, stronger assurance, or recent authentication.

## Local Development Versus Production

Local development settings should not silently become production settings.

- Keep local callback URLs separate from production callback URLs.
- Keep local client IDs and secrets separate from production app registrations.
- Use local self-signed or development certificates only in development.
- Do not weaken production certificate validation to make local development easier.
- Do not carry permissive local CORS, cookie, redirect, or provider settings into production.
- Confirm `appsettings.Development.json`, user secrets, environment variables, and deployment configuration are clearly separated.

## Minimum Provider Smoke-Test Matrix

Run this matrix for every enabled provider before production release.

| Scenario | Expected result |
| --- | --- |
| Anonymous request to protected page | Redirects or challenges through the configured default challenge scheme. |
| Successful login | User returns to the application with the expected local session cookie. |
| Invalid callback or unknown provider | Request fails safely without leaking provider secrets or stack traces. |
| Valid local return URL | User returns only to an allowed local application URL. |
| External or malformed return URL | Request is rejected or falls back safely. |
| Claims transformation | Required application-owned claims are present and normalized. |
| Missing required authorization claim | User is denied without receiving privileged access. |
| Logout | Local cookie state is cleared and post-logout behavior is understood. |
| Session timeout | Expired sessions require reauthentication. |
| Reverse-proxy deployment URL | Redirect/callback/logout URLs use the public HTTPS origin. |
| Provider secret rotation | New secret works and old secret is retired according to provider policy. |
| MFA or step-up requirement | Provider-owned MFA behavior is enforced where required. |

## Release Review Sign-Off

Before publishing or deploying a generated application with authentication enabled, record the following decisions in the consuming application's deployment notes:

- Enabled providers and their production app registration identifiers.
- Approved redirect, callback, ACS, and logout URLs.
- Cookie lifetime and session timeout policy.
- Claims translation contract and role/group normalization rules.
- Secret, certificate, and token storage location.
- Provider metadata validation expectations.
- MFA or step-up ownership.
- Smoke-test results for every enabled provider.

A generated application can be production-oriented without being production-complete. Authentication becomes production-complete only after the consuming application binds the starter configuration to its real provider, deployment, claims, session, and security policies.
