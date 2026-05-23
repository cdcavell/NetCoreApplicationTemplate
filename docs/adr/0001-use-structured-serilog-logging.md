# ADR-0001: Use Structured Serilog Logging

## Status

Accepted

## Context

The template is intended to provide a production-oriented ASP.NET Core baseline. Applications created from it need useful logs for local development, troubleshooting, request correlation, operational review, and future deployment environments.

Basic framework logging is useful, but the template also needs a consistent structured logging approach that can be extended by consuming applications without scattering logging setup across startup code.

The logging approach should support:

- Request and application diagnostics.
- Correlation and request identifiers.
- Console output for local and hosted environments.
- File output for simple deployments and local inspection.
- Environment and thread enrichment.
- Centralized configuration through application settings.
- A clean path toward future sink changes without redesigning the application startup pattern.

## Decision

Use Serilog as the structured logging foundation for the template.

Logging is configured centrally through application configuration and startup extension patterns rather than ad hoc setup across the application. The template keeps logging defaults practical for local development while allowing consuming applications to replace or extend sinks for production hosting environments.

## Consequences

Positive consequences:

- Logs can carry structured properties such as request path, request ID, correlation ID, source context, and environment information.
- Console logging works well for local development, containers, and hosted platforms.
- File logging provides a simple baseline for development and small deployments.
- Consuming applications can tune minimum levels, overrides, sinks, templates, retention, and enrichment through configuration.
- The template has a consistent observability starting point before optional telemetry export is added.

Trade-offs and risks:

- Serilog adds a third-party logging dependency.
- File logging may not be appropriate for every production environment.
- Production deployments must review log retention and sensitive-data exposure.
- Consuming applications may need organization-specific sinks such as Seq, Application Insights, OpenTelemetry collectors, or centralized log platforms.

## Alternatives Considered

- Use only built-in Microsoft.Extensions.Logging: simpler dependency posture, but less complete as a structured logging baseline for the template goals.
- Use another structured logging library: possible, but Serilog is widely used in ASP.NET Core applications and fits the template's configuration-driven approach.
- Defer logging decisions to consuming applications: flexible, but would weaken the template as a production-oriented baseline.

## Related References

- [`docs/articles/logging.md`](../articles/logging.md)
- [`docs/articles/configuration.md`](../articles/configuration.md)
- `src/ProjectTemplate.Web/appsettings.json` repository source file
