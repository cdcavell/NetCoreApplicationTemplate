# Authorization

## Role and Permission Authorization Policies

The application includes baseline authorization policy patterns for authenticated users, role-based access, and permission-based access.

Default policy names:

| Policy | Purpose |
|---|---|
| `ProjectTemplate.AuthenticatedUser` | Requires an authenticated user. |
| `ProjectTemplate.Role.Administrator` | Requires a normalized role claim. |
| `ProjectTemplate.Permission.ManageApplication` | Requires a normalized permission claim. |

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
      "administrator"
    ],
    "ManageApplicationPermissions": [
      "application.manage"
    ]
  }
}
```
Example usage:
```csharp
[Authorize(Policy = ApplicationAuthorizationPolicyNames.AdministratorRole)]
public IActionResult AdminOnly()
{
    return View();
}
```
