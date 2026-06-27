# Authentication

## Default Authentication Posture

The base application enables the application authentication module and local cookie authentication by default.

By default:

- `ProjectTemplate:Authentication:Enabled` is `true`.
- The default authenticate, challenge, and sign-in schemes use `Cookies`.
- Local cookie authentication is enabled.
- External providers such as OpenID Connect, SAML2, Microsoft, Google, and GitHub are disabled.

This gives applications a working local authentication baseline while keeping external identity provider integration opt-in.

To enable an external provider, keep application authentication enabled and set only the required provider configuration to enabled. For example, OIDC requires `ProjectTemplate:Authentication:Providers:OpenIdConnect:Enabled` to be set to `true` along with valid authority, client ID, and client secret values.

Before enabling any real provider in production, review the [Production Authentication Hardening Checklist](authentication-hardening.md). Generated provider settings are starter configuration and must be bound to the consuming application's production URLs, provider registrations, claims contract, token policy, secret-management approach, session behavior, and MFA expectations.

### OpenID Connect

The application includes standards-based OpenID Connect authentication support.
External OIDC provider integration is disabled by default. To enable it, configure the `ProjectTemplate:Authentication` section and set both authentication and the OpenID Connect provider to enabled.

```json
"ProjectTemplate": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "OpenIdConnect",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "OpenIdConnect": {
        "Enabled": true,
        "Scheme": "OpenIdConnect",
        "DisplayName": "OpenID Connect",
        "Authority": "https://login.example.com",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-oidc",
        "ResponseType": "code",
        "SaveTokens": true,
        "Scopes": [
          "openid",
          "profile",
          "email"
        ]
      }
    }
  }
}
```
_Do not commit real client secrets to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

## SAML2

The application includes standards-based SAML2 authentication support.
External SAML2 provider integration is disabled by default. To enable it, configure the `ProjectTemplate:Authentication` section and set both authentication and the Saml2 provider to enabled.
```json
"ProjectTemplate": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "Saml2",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "Saml2": {
        "Enabled": true,
        "Scheme": "Saml2",
        "DisplayName": "SAML2",
        "EntityId": "https://localhost:5001/saml2",
        "MetadataUrl": "https://idp.example.com/metadata",
        "ModulePath": "/Saml2/Acs",
        "LoadMetadata": true,
        "RequireSignedAssertions": true,
        "ValidateCertificates": true
      }
    }
  }
}
```
_Do not commit real certificates, private keys, or real IdP metadata to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

## Microsoft External Provider

The application includes Microsoft external authentication support through `Microsoft.AspNetCore.Authentication.MicrosoftAccount`.

The Microsoft provider is disabled by default and only registers when:

`ProjectTemplate:Authentication:Providers:Microsoft:Enabled`

is set to `true`.

```json
"ProjectTemplate": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "Microsoft",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "Microsoft": {
        "Enabled": true,
        "Scheme": "Microsoft",
        "DisplayName": "Microsoft",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-microsoft",
        "Scopes": []
      }
    }
  }
}
```
_Do not commit real client IDs, client secrets, certificates, tokens, or provider credentials to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

## Google External Provider

The application includes Google external authentication support through `Microsoft.AspNetCore.Authentication.Google`.

The Google provider is disabled by default and only registers when:

`ProjectTemplate:Authentication:Providers:Google:Enabled`

is set to `true`.

```json
"ProjectTemplate": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "Google",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "Google": {
        "Enabled": true,
        "Scheme": "Google",
        "DisplayName": "Google",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-google",
        "Scopes": [
          "profile",
          "email"
        ]
      }
    }
  }
}
```
_Do not commit real client IDs, client secrets, certificates, tokens, or provider credentials to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

## GitHub External Provider
The application includes GitHub external authentication support through `AspNet.Security.OAuth.GitHub`.

The GitHub provider is disabled by default and only registers when:

`ProjectTemplate:Authentication:Providers:GitHub:Enabled`

is set to `true`.

```json
"ProjectTemplate": {
  "Authentication": {
    "Enabled": true,
    "DefaultScheme": "Cookies",
    "DefaultChallengeScheme": "GitHub",
    "DefaultSignInScheme": "Cookies",
    "Providers": {
      "GitHub": {
        "Enabled": true,
        "Scheme": "GitHub",
        "DisplayName": "GitHub",
        "ClientId": "",
        "ClientSecret": "",
        "CallbackPath": "/signin-github",
        "Scopes": [
          "profile",
          "email"
        ]
      }
    }
  }
}
```
_Do not commit real client IDs, client secrets, certificates, tokens, or provider credentials to source control. Use user secrets, environment variables, deployment secrets, or a secure secret store._

## Authentication Provider Startup Validation

Authentication provider configuration is validated during application startup.

Provider-specific values are only required when that provider is enabled. Disabled providers may keep placeholder or empty values so the base application remains safe to run without external identity-provider setup.

When a provider is enabled, startup validation fails fast if required values are missing. Validation messages identify the missing configuration key, but do not log configured secret values.

Validated providers include:

- OpenID Connect
- SAML2
- Microsoft
- Google
- GitHub

This prevents partially configured authentication providers from failing later during runtime login flows.

## Baseline Authentication Endpoints

The application provides minimal account and external authentication endpoints:

| Endpoint | Purpose |
|---|---|
| `GET /Account/Login` | Displays the baseline login page and available registered external providers. |
| `POST /Account/Logout` | Signs out of the local cookie session. Requires anti-forgery validation. |
| `GET /Account/AccessDenied` | Displays a safe access denied response. |
| `GET /External/Challenge` | Starts an external authentication challenge for a registered provider scheme. |

`/External/Challenge` accepts a `provider` value and an optional `returnUrl`.

Return URLs are validated as local URLs before redirecting to avoid open redirect vulnerabilities. Unknown provider schemes are rejected safely. Provider secrets, tokens, cookies, and sensitive query-string values should not be logged.

## External Social Provider Strategy and OpenIddict Client Evaluation

The application currently uses provider-specific ASP.NET Core authentication handlers for Microsoft, Google, and GitHub. This keeps the implementation simple, scheme-based, and consistent with the existing authentication module structure.

Current provider-specific packages remain supported and are the active implementation path for this application. They are disabled by default, registered only when enabled, validated during startup, and configured through:

```text
ProjectTemplate:Authentication:Providers:Microsoft
ProjectTemplate:Authentication:Providers:Google
ProjectTemplate:Authentication:Providers:GitHub
```
OpenIddict Client was evaluated as a future external social provider architecture. OpenIddict Client provides a broader OAuth 2.0/OpenID Connect client stack with web-provider integrations for many external providers, including GitHub, Microsoft, and Google. It also provides stronger long-term capabilities such as OpenID Connect support, stateful client behavior, replay protections, discovery support, token introspection/revocation support, and resilient backchannel behavior.

However, adopting OpenIddict Client would be an architectural migration rather than a direct package swap. A migration would need to account for:
- OpenIddict client/core service registration.
- Token/state storage requirements.
- Provider-specific redirect endpoint design.
- Callback endpoint/controller handling.
- Existing `/External/Challenge` behavior.
- Existing startup validation behavior.
- Existing tests and documentation.
- Compatibility with Microsoft, Google, GitHub, and future providers.

OpenIddict Client may be implemented as the preferred candidate for a future broader social-provider architecture if the application later needs a unified OAuth/OIDC client model across many providers or advanced token-handling features.

Any future migration would be handled through a dedicated implementation issue and should preserve the existing working Microsoft, Google, and GitHub behavior until a replacement path is fully tested.

## Claims Transformation and Normalization

The application includes an optional claims transformation layer that normalizes provider-specific claims into application-owned claim names.

External identity providers often use different claim names for the same concept. For example, one provider may emit `sub`, another may emit `nameidentifier`, and another may use a SAML claim URI. The claims transformation layer allows these inputs to be mapped into consistent application claim names such as:

- `application:subject`
- `application:name`
- `application:email`
- `application:role`
- `application:group`
- `application:permission`

Original provider claims are preserved by default. They are only removed when `ProjectTemplate:Authentication:ClaimsTransformation:RemoveOriginalClaims` is explicitly set to `true`.
