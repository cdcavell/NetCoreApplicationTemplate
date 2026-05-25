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

The template registers named policies but does not force every endpoint to require authorization by default.

Application authors should apply authorization intentionally at the controller, Razor Page, endpoint, folder, or convention level. This keeps the base template runnable while still providing clear policy names for protected application areas.

For production applications, review whether a fallback policy or broader default authorization convention should be added by the consuming application.

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