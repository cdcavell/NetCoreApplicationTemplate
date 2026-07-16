# Authorization

## Role and Permission Authorization Policies

The application includes baseline authorization policy patterns for authenticated users, role-based access, and permission-based access.

Default policy names:

| Policy | Purpose |
|---|---|
| `application.AuthenticatedUser` | Requires an authenticated user. |
| `application.Role.Administrator` | Requires a normalized role claim. |
| `application.Permission.ManageApplication` | Requires a normalized permission claim. |

Default normalized claim types:

| Claim Type | Purpose |
|---|---|
| `application:role` | Role claim used by role-based policies. |
| `application:permission` | Permission claim used by permission-based policies. |

Configuration example:

```json
"ProjectTemplate": {
  "Authorization": {
    "RequireAuthenticatedUserByDefault": true,
    "RoleClaimType": "application:role",
    "PermissionClaimType": "application:permission",
    "AdministratorRoles": [
      "Administrator"
    ],
    "ManageApplicationPermissions": [
      "application.manage"
    ]
  }
}
```

## Default Authorization Posture

The default scaffold is closed by default for routed endpoints. When `ProjectTemplate:Authorization:RequireAuthenticatedUserByDefault` is `true`, ASP.NET Core registers a fallback authorization policy that requires an authenticated user for endpoints without authorization metadata.

This protects newly added controller actions, Razor Pages, and other routed endpoints when a developer does not attach an `[Authorize]` attribute or named policy. Stronger role, permission, claim, or custom policies still apply where they are declared explicitly.

Endpoints that are intentionally public must opt out explicitly by using `[AllowAnonymous]`, `.AllowAnonymous()`, or an equivalent endpoint convention.

The authentication-disabled template variant generated with `--authProvider none` sets application authentication, cookie authentication, and the fallback requirement to `false`. Application startup validation rejects an inconsistent configuration where authentication is disabled while the fallback authorization policy still requires an authenticated user.

### Deliberate opt-out

A consuming application that intentionally wants unannotated routed endpoints to remain public can disable the fallback policy:

```json
"ProjectTemplate": {
  "Authorization": {
    "RequireAuthenticatedUserByDefault": false
  }
}
```

This is an application-wide security posture change. Review all endpoint surfaces before disabling it, and prefer explicit anonymous metadata for isolated public routes.

## Endpoint Access Classification

The generated application's routed endpoint contract is intentionally narrow and reviewable.

| Endpoint or endpoint group | Classification | Rationale |
|:---|:---|:---|
| `GET /Account/Login` | Explicitly anonymous | Unauthenticated users must be able to enter the authentication flow. |
| `GET /External/Challenge` | Explicitly anonymous | Starts a configured external-provider challenge after validating the provider and local return URL. |
| External-provider callback and remote authentication paths | Middleware-owned | OAuth, OpenID Connect, SAML2, and similar handlers process their configured callback paths before controller authorization. Consumers must preserve provider callback configuration and avoid mapping conflicting application endpoints. |
| `GET /Account/AccessDenied` | Explicitly anonymous | Prevents authorization failure handling from creating a login or access-denied redirect loop. |
| `GET /Home/Error/{statusCode?}` | Explicitly anonymous | Centralized exception and status-code handling must be able to render a terminal response for anonymous requests. |
| `GET /health`, `/health/live`, `/health/ready` | Explicitly anonymous, deployment-specific exposure | Infrastructure probes require stable unauthenticated access by default. Production deployments should restrict network reachability through ingress, firewall, service-mesh, or load-balancer policy. |
| `POST /Account/Logout` | Explicitly authenticated | Logout requires `[Authorize]` and anti-forgery validation. It is not part of the anonymous allowlist. |
| Starter Razor Page `/` | Authenticated by fallback | The starter application surface demonstrates the closed-by-default posture. |
| Sample application-information API routes | Authenticated by fallback | Sample APIs are not public merely because they are diagnostic or informational. |
| Future controllers, Razor Pages, minimal APIs, diagnostics, or sample endpoints | Authenticated by fallback | New routed endpoints remain protected unless a deliberate anonymous decision is made and tested. |
| Static files and browser assets | Static-file middleware | `UseStaticFiles` runs before routing and is not governed by endpoint authorization metadata. Protect sensitive files by not placing them under the public web root. |

The integration test `AnonymousEndpointContractTests.RoutedEndpoints_ExposeOnlyReviewedAnonymousAllowlist` enumerates routed application endpoints and fails when the anonymous metadata set changes without an explicit contract update.

## Named Policy Usage Examples

Require any authenticated user:

```csharp
[Authorize(Policy = ApplicationAuthorizationPolicyNames.AuthenticatedUser)]
public IActionResult SecurePage()
{
    return View();
}
```

Require an administrator role:

```csharp
[Authorize(Policy = ApplicationAuthorizationPolicyNames.AdministratorRole)]
public IActionResult AdminOnly()
{
    return View();
}
```

Require the manage-application permission:

```csharp
[Authorize(Policy = ApplicationAuthorizationPolicyNames.ManageApplicationPermission)]
public IActionResult ManageApplication()
{
    return View();
}
```

## Sample Policy Purposes

| Policy constant | Policy name | Recommended use |
|:---|:---|:---|
| `ApplicationAuthorizationPolicyNames.AuthenticatedUser` | `application.AuthenticatedUser` | Protect pages or endpoints that require any signed-in user. |
| `ApplicationAuthorizationPolicyNames.AdministratorRole` | `application.Role.Administrator` | Protect administrative UI or operations. |
| `ApplicationAuthorizationPolicyNames.ManageApplicationPermission` | `application.Permission.ManageApplication` | Protect management actions that should be permission-driven rather than purely role-driven. |

## Claims Normalization Relationship

Authorization policies are designed to work with the claims transformation layer documented in [Authentication](authentication.md).

External providers often emit different role, group, permission, or scope claim names. The template can normalize these claims into application-owned claim names so authorization policies can remain stable across providers.

See [Runtime Readiness Baseline](runtime-readiness.md) for the consolidated release-readiness view.
