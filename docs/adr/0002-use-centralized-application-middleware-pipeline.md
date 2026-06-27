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

The current baseline is implemented by `UseApplicationPipeline()` in `src/ProjectTemplate.Web/Extensions/PipelineExtensions.cs`.

## Ordering Invariants

The following ordering rules are intentional. They should not be changed casually because moving one step can affect request identity, logging accuracy, error handling, security posture, or endpoint behavior.

1. **Forwarded headers must run before request-dependent middleware.**
   Reverse proxy and load-balancer headers need to be applied before middleware that reads scheme, host, path base, remote IP, or client IP. This keeps logging, redirects, rate-limiting decisions, generated URLs, and security decisions aligned with the externally observed request.

2. **Structured request logging should run after forwarded headers and near the start of the pipeline.**
   Request logs should capture the corrected request identity while still wrapping most downstream behavior. Moving logging later can hide failures that occur before the logging middleware is reached.

3. **Centralized exception and status-code handling should wrap most application behavior.**
   Error handling belongs early enough to catch downstream exceptions and normalize error responses. Moving it after endpoint execution, authentication, authorization, or custom business middleware would reduce the protection provided by the centralized failure path.

4. **Security headers should be applied before normal response-producing middleware.**
   Security headers should be available on ordinary application responses and many error responses. Moving security headers late can miss responses that are generated earlier in the pipeline.

5. **HTTPS redirection should run before routing and endpoint execution.**
   Redirect behavior should be decided before endpoint-specific work runs. It also relies on forwarded-header correction being applied first when the application is hosted behind a reverse proxy or TLS-terminating load balancer.

6. **Static files should remain before routing unless endpoint-aware static-file behavior is intentionally introduced.**
   The template uses the conventional static-file placement for Razor Pages/MVC UI assets. Moving static files after authentication or authorization changes whether static assets are protected by default and should be treated as a deliberate behavior change.

7. **Routing must run before endpoint-aware middleware.**
   CORS, rate limiting, authentication, and authorization may depend on endpoint metadata. Routing establishes the selected endpoint context that those components use.

8. **CORS should run after routing and before authentication/authorization.**
   This placement lets endpoint metadata influence CORS behavior while still allowing cross-origin preflight handling before authentication challenges or authorization failures are produced.

9. **Rate limiting should run after routing when endpoint-specific policies are used.**
   Endpoint-aware rate-limiting policies require routing metadata. Moving rate limiting before routing changes the policy surface to mostly global behavior.

10. **Authentication must run before authorization.**
    Authorization decisions require the user principal created by authentication. Reversing these calls can cause valid users to appear anonymous to authorization policy evaluation.

11. **Controller and Razor Page endpoint mapping should remain at the end of the template-owned pipeline.**
    Endpoint mapping should occur after the middleware that prepares request identity, error behavior, security headers, routing metadata, rate limiting, and authorization behavior.

12. **Health checks are mapped explicitly from `Program.cs`.**
    `Program.cs` calls `MapApplicationHealthChecks()` after `UseApplicationPipeline()` to keep health-check endpoint registration visible at startup. Consumers that add custom health behavior should preserve the intended health-check access model and avoid accidentally placing health endpoints behind unrelated UI or business authorization requirements unless that is intentional.

## Review Guidance for Future Changes

Before changing the pipeline order, reviewers should ask:

- Does this change alter which scheme, host, path base, or client IP downstream middleware sees?
- Does it change whether centralized error handling wraps a failure path?
- Does it change whether security headers are emitted on normal, redirected, static-file, error, or endpoint responses?
- Does it move endpoint-aware middleware before routing?
- Does it change CORS preflight behavior?
- Does it change which requests are rate limited before authentication or authorization?
- Does it alter whether health-check endpoints remain reachable by the intended callers?
- Does it require updates to integration tests, documentation, or deployment guidance?

Pipeline changes that affect any of those questions should be treated as behavior changes, not simple refactoring.

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
