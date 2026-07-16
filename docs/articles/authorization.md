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

Endpoints that are intentionally public must opt out explicitly by using `[AllowAnonymous]`, `.AllowAnonymous()`, or an equivalent endpoint convention. The base application currently makes authentication entry points and infrastructure health probes explicit anonymous exceptions.

The authentication-disabled template variant generated with `--authProvider none` sets both of the following values to `false`:

```json
"Authentication": {
  "Enabled": false
},
"Authorization": {
  "RequireAuthenticatedUserByDefault": false
}
```

Application startup validation rejects an inconsistent configuration where authentication is disabled while the fallback authorization policy still requires an authenticated user.

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

### Public endpoint review

Review endpoints that must remain publicly reachable, including:

- Login, external challenge, callback, and access-denied endpoints.
- Health check endpoints used by infrastructure, reverse proxies, or orchestration platforms.
- Public documentation or landing pages.
- API endpoints that intentionally allow anonymous access.
- Static files and browser assets, which are served by static-file middleware rather than routed endpoint authorization metadata.

Use `[AllowAnonymous]` intentionally for routed endpoints that should remain public.

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
