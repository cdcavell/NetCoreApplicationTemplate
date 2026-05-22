# Configuration

This section will document application configuration strategy.

Planned areas:

- appsettings.json.
- appsettings.Development.json.
- User secrets.
- Environment variables.
- Provider-specific configuration.
- Options pattern.
- Strongly typed settings.
- Validation on startup.
- Sensitive configuration handling.

## Configuration Validation

The application uses strongly typed options classes for application-owned configuration under the `ProjectTemplate` section.

Validated configuration areas include:

- `ProjectTemplate:SecurityHeaders`
- `ProjectTemplate:ForwardedHeaders`
- `ProjectTemplate:RateLimiting`

Invalid startup-sensitive values fail application startup rather than being silently corrected. This helps catch unsafe or malformed production configuration before the application begins serving requests.
