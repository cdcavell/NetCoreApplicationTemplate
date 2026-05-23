# ADR-0002: Use Centralized Application Middleware Pipeline

## Status

Accepted

## Context

ASP.NET Core middleware order is security-sensitive and behavior-sensitive. A template that includes forwarded headers, exception handling, HTTPS, static files, routing, CORS, rate limiting, authentication, authorization, request logging, security headers, health checks, and endpoint mapping needs a predictable pipeline structure.

If middleware registration is scattered throughout `Program.cs`, consuming applications can accidentally reorder important components or make startup code harder to review. This is especially risky for a reusable template because future applications inherit the startup pattern.

The template needs a middleware approach that is:

- Easy to scan.
- Centralized enough to preserve ordering.
- Extensible by consuming applications.
- Testable through integration tests.
- Clear about where security-sensitive middleware belongs.
- Consistent with the project's extension-method style.

## Decision

Use a centralized application middleware pipeline extension for template-owned middleware ordering.

The application startup code delegates template-owned middleware registration to a central pipeline extension. Feature-specific middleware can still be implemented in focused extension methods, but their relative ordering is controlled from the centralized pipeline.

## Consequences

Positive consequences:

- `Program.cs` remains concise and easier to review.
- Middleware ordering is documented in one primary place.
- Security-sensitive order is less likely to drift accidentally.
- Consuming applications can see the intended baseline sequence before adding application-specific middleware.
- Integration tests can validate behavior against the real configured pipeline.
- Future template features have a clear place to participate in startup ordering.

Trade-offs and risks:

- The centralized pipeline must be kept readable as more features are added.
- Consuming applications may need guidance on where to insert custom middleware.
- Too much abstraction could hide important startup behavior if documentation is not maintained.
- Changes to the central pipeline can have broad effects and should be reviewed carefully.

## Alternatives Considered

- Keep all middleware registration directly in `Program.cs`: more explicit in one file, but it can become noisy and easier to misorder over time.
- Let each feature register itself independently without centralized ordering: modular, but dangerous for middleware whose position affects security or behavior.
- Use separate startup classes: familiar to older ASP.NET Core patterns, but less aligned with modern minimal-hosting style.

## Related References

- [`docs/articles/middleware.md`](../articles/middleware.md)
- [`docs/articles/forwarded-headers.md`](../articles/forwarded-headers.md)
- [`docs/articles/security-headers.md`](../articles/security-headers.md)
- [`docs/articles/rate-limiting.md`](../articles/rate-limiting.md)
