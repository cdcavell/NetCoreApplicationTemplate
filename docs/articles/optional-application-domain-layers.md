# Optional Application and Domain Layers

The default template intentionally keeps the generated solution compact:

```text
ProjectTemplate.Web
        |
        v
ProjectTemplate.Infrastructure
```

That shape is enough for many small-to-medium internal applications, prototypes that may grow into production systems, and line-of-business applications whose business rules are still simple.

This guide describes an optional growth path for consumers who outgrow the starter structure. It is guidance only. The base template does not require Clean Architecture, CQRS, MediatR, DDD, or additional projects.

## When the Default Structure Is Sufficient

Keep the default `Web` / `Infrastructure` split when the application is still easy to understand and change.

The default structure is usually sufficient when:

- The application mostly serves Razor Pages, MVC controllers, API endpoints, or health checks.
- Business rules are simple and close to request/response behavior.
- Validation is mostly input validation rather than complex business policy.
- Data access is straightforward and owned by the application.
- The team can still find use cases, persistence behavior, and tests without friction.
- Adding more projects would mostly create ceremony rather than reduce complexity.

Do not add layers only because a pattern says every application should have them. Extra projects add build cost, dependency-management work, naming decisions, and review overhead. They should earn their place.

## When to Add Optional Layers

Consider introducing `ProjectTemplate.Application` and/or `ProjectTemplate.Domain` when the application begins to show real domain complexity.

Useful signals include:

- Business rules are duplicated across pages, controllers, jobs, or services.
- Controllers or page models are becoming orchestration-heavy.
- Authorization, validation, persistence, and domain decisions are blending together.
- Use cases need to be tested without booting the web host.
- Multiple UI/API entry points need to call the same business workflow.
- External integrations, messaging, or background jobs need to trigger the same application behavior as web requests.
- Domain events or business state transitions need a clearer home.
- The team needs stronger boundaries before adding more features.

When those signals appear, add only the layer that solves the current problem.

## Optional Target Structure

A larger application may grow toward this optional structure:

```text
src/
├── ProjectTemplate.Web/
│   └── ASP.NET Core host, endpoints, UI, authentication, authorization, and composition root
│
├── ProjectTemplate.Application/
│   └── Use cases, application services, commands, queries, DTOs, validators, and ports/interfaces
│
├── ProjectTemplate.Domain/
│   └── Domain entities, value objects, domain services, domain events, and business rules
│
└── ProjectTemplate.Infrastructure/
    └── EF Core, repositories/adapters, external services, messaging, files, email, and provider implementations
```

This is not the generated default. It is an optional extension model for consumers who need more separation.

## Dependency Direction

A common dependency direction is:

```text
ProjectTemplate.Web
        |
        v
ProjectTemplate.Application
        |
        v
ProjectTemplate.Domain

ProjectTemplate.Infrastructure
        |
        v
ProjectTemplate.Application and/or ProjectTemplate.Domain abstractions
```

In this model:

- `Web` is still the composition root.
- `Web` references `Application` so controllers, pages, APIs, and background entry points can invoke use cases.
- `Application` references `Domain` so use cases can coordinate domain behavior.
- `Domain` should not reference `Web`, `Infrastructure`, EF Core, ASP.NET Core, provider SDKs, or configuration packages.
- `Infrastructure` implements persistence, external services, and provider adapters behind interfaces owned by `Application` or, when appropriate, the domain model.
- `Web` wires the concrete `Infrastructure` implementations into dependency injection.

Avoid circular references. If a dependency cycle appears, it usually means an abstraction belongs in `Application` or `Domain` instead of `Infrastructure` or `Web`.

## Suggested Responsibilities

| Concern | Suggested home when optional layers are added |
| --- | --- |
| Razor Pages, MVC controllers, API endpoints | `ProjectTemplate.Web` |
| Authentication provider registration | `ProjectTemplate.Web` |
| Authorization policy registration | `ProjectTemplate.Web`, with policy inputs from `Application` or `Domain` when useful |
| Request/response models tied to HTTP | `ProjectTemplate.Web` |
| Use cases and workflows | `ProjectTemplate.Application` |
| Commands and queries | `ProjectTemplate.Application` |
| Application DTOs/contracts | `ProjectTemplate.Application` |
| Application validators | `ProjectTemplate.Application` |
| Ports/interfaces for persistence or external services | `ProjectTemplate.Application` |
| Domain entities and value objects | `ProjectTemplate.Domain` |
| Domain rules and invariants | `ProjectTemplate.Domain` |
| Domain services | `ProjectTemplate.Domain` |
| Domain events | `ProjectTemplate.Domain` |
| EF Core `DbContext`, migrations, entity configuration | `ProjectTemplate.Infrastructure` |
| Repository/adaptor implementations | `ProjectTemplate.Infrastructure` |
| Email, files, queues, HTTP clients, provider SDKs | `ProjectTemplate.Infrastructure` |

## Business Rules

Keep simple request validation close to the web boundary when it only protects model binding or user input shape.

Move business rules into `Application` or `Domain` when they represent policy that should be consistent across entry points. For example:

- A required field on a Razor Page view model can remain in `Web`.
- A rule that determines whether an order may be submitted belongs in `Application` or `Domain`.
- A rule that protects an invariant inside an entity or value object belongs in `Domain`.
- A rule that coordinates persistence, authorization inputs, and external notifications usually belongs in `Application`.

The dividing line is reuse and meaning. If the rule describes the business, move it out of the HTTP layer. If it describes a request shape, it can stay near the request.

## Commands, Queries, and DTOs

Consumers may organize use cases as command/query handlers, application services, or simple methods. The template does not require a specific pattern.

Possible `Application` organization:

```text
ProjectTemplate.Application/
├── Abstractions/
├── Common/
├── Features/
│   └── Users/
│       ├── CreateUserCommand.cs
│       ├── CreateUserResult.cs
│       ├── GetUserDetailsQuery.cs
│       └── UserDetailsDto.cs
├── Validation/
└── DependencyInjection.cs
```

Use DTOs when data crosses an application boundary. Avoid exposing EF Core entities directly as application contracts when the model is likely to evolve independently from persistence.

## Domain Events

Domain events are optional. Add them only when they clarify a business state change or decouple follow-up behavior.

A domain event can live in `Domain` when it describes something meaningful that already happened in the domain, such as:

```text
UserRegistered
OrderSubmitted
InvoiceApproved
```

Application-layer code can collect and dispatch those events after persistence succeeds. Infrastructure can provide the dispatcher, message bus adapter, outbox implementation, or notification mechanism.

Avoid adding a messaging framework before there is a clear need for asynchronous behavior or cross-boundary event handling.

## Service Registration

Continue the existing extension-method convention when adding optional layers.

Example registration shape:

```csharp
// Program.cs
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
```

Possible application-layer extension:

```csharp
namespace ProjectTemplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register use cases, validators, domain event dispatch abstractions, or application services.
        return services;
    }
}
```

Possible infrastructure extension:

```csharp
namespace ProjectTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext, repositories, external adapters, and provider implementations.
        return services;
    }
}
```

Keep `Web` as the composition root so deployment-specific configuration, provider selection, and ASP.NET Core startup remain centralized.

## Testing Optional Layers

Adding layers should make testing easier, not weaker.

Recommended test growth path:

- Keep existing web integration tests that protect startup, middleware, authentication, authorization, error handling, and data access wiring.
- Add `ProjectTemplate.Application.Tests` when use cases contain meaningful branching, validation, or orchestration.
- Add `ProjectTemplate.Domain.Tests` when domain entities, value objects, invariants, or domain services contain meaningful rules.
- Use focused tests for application/domain behavior without booting the full web host.
- Keep at least a small number of end-to-end or integration tests to prove the web host composes the optional layers correctly.

Do not replace integration tests with unit tests only. The template's value comes partly from proving that startup, middleware, provider configuration, and runtime wiring still work together.

## MediatR, CQRS, and DDD

Consumers may add MediatR, CQRS, DDD patterns, or another application architecture style if those patterns solve a real complexity problem.

The base template does not require them because:

- Many applications do not need a mediator pipeline.
- CQRS can be overkill for simple CRUD or workflow screens.
- DDD terminology can add confusion if the domain model is still mostly data-oriented.
- The template should not force consumers into one methodology before the application earns that structure.

A good middle path is to start with ordinary application services and introduce commands, queries, domain events, or mediator behavior only when they reduce complexity.

## Migration Path From the Default Template

A safe incremental path is:

1. Keep the default `Web` / `Infrastructure` structure while the application is simple.
2. Move repeated use-case logic from controllers/page models into application services.
3. Add `ProjectTemplate.Application` when use cases need a clear home.
4. Move business rules that are independent of HTTP and persistence into `ProjectTemplate.Domain`.
5. Move infrastructure implementations behind interfaces owned by `Application` or `Domain`.
6. Add focused tests for the new layer while preserving web integration coverage.
7. Update documentation so future maintainers understand the chosen layering model.

The goal is not to maximize layer count. The goal is to keep the codebase understandable as the application grows.

## Summary

The default template remains intentionally lightweight. Optional `Application` and `Domain` layers are a growth path, not a requirement.

Add them when they clarify real use cases, protect business rules, improve testing, or reduce coupling. Do not add them merely to satisfy a pattern.
