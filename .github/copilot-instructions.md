# Copilot Instructions — dotnet-template

## Project overview

.NET 10 Web API template using Clean Architecture, CQRS (MediatR), Mapster, and API versioning. Solution file: `Template.sln`. All projects target `net10.0`.

## Architecture & layer rules

Dependencies point inward only:

```
WebApi → Infrastructure → Application → Domain
```

- **Domain** — zero project/package dependencies. Entities, value objects, business logic.
- **Application** — references Domain only. Use cases (commands/queries), port interfaces, MediatR handlers.
- **Infrastructure** — references Application (gets Domain transitively). Adapters, persistence, DTOs, multi-tenancy.
- **WebApi** — references Application + Infrastructure. Controllers, contracts, middleware, composition root.

Never add a reference that violates this direction (e.g. Application must not reference Infrastructure or WebApi).

## C# style conventions

### General

- **File-scoped namespaces** always (`namespace X;`), never block-scoped.
- **Nullable reference types** are enabled. Use `?` for nullable types; use `null!` only where initialization is guaranteed later (e.g. module initializers).
- **`sealed`** on all concrete types unless inheritance is intentionally needed — entities, handlers, records, DTOs, repositories, filters, attributes, transformers, test classes.
- **Primary constructors** for dependency injection in controllers, middleware, handlers, and options classes.
- **`_camelCase`** for private fields (e.g. `_store`).
- **`ct`** as the parameter name for `CancellationToken` in controllers, repositories, interfaces, and helpers (with `= default` on interface methods). MediatR handlers use **`cancellationToken`** to match the `IRequestHandler` interface signature.

### Records vs classes

- **`sealed record`** (positional) for: commands, queries, responses, DTOs, API request/response contracts, value objects.
- **`sealed class`** for: domain entities with behavior, repositories, middleware, controllers, DI extension classes, test classes.

### Domain entities

- Public properties with **`private set`** for mutable state and **`init`** or **`required`** for immutable data.
- State transitions through **behavior methods** (e.g. `MarkAsProcessed()`), not public setters.
- Derived values as **expression-bodied properties**.
- **XML doc comments** (`///`) on the type and important members.

### CQRS features (Application layer)

Each use case lives in `Application/Features/{Feature}/` as a **single file** with a static outer class:

```csharp
public static class CreateTest
{
    public sealed record Command(...) : IRequest<Response>;
    public sealed record Response(...);
    private sealed class Handler(...) : IRequestHandler<Command, Response> { ... }
}
```

- The outer class is `public static`, named after the use case.
- `Command` / `Query` is a `sealed record` implementing `IRequest<Response>`.
- `Response` is a `sealed record` — the application-level output port.
- `Handler` is **`private sealed class`** with a **primary constructor** for dependencies.

### Mapster mapping configs

- One `IRegister` class per feature, co-located with the types it maps.
- Application layer: `{Feature}MappingConfig.cs` — Domain → Application response.
- WebApi layer: `{Feature}ContractMappingConfig.cs` — Application response → API contract.
- Infrastructure does **not** use Mapster; repository mapping is manual to respect domain encapsulation.

### Port / Adapter interfaces

- Ports in `Application/Abstractions/` as `public interface`.
- Adapters in `Infrastructure/` as `sealed class` implementing the port.

### Controllers

- Inherit `ControllerBase`, decorated with `[ApiController]`, `[Route("api/v{version:apiVersion}/[controller]")]`, and `[ApiVersion]`.
- Primary constructor with `IMediator` and `IMapper`.
- Return `ActionResult<T>` — return the Application `Response` directly when it matches the API shape; introduce a WebApi contract only when the shape diverges.
- Use `[MapToApiVersion("X.0")]` for versioned endpoints.

### API contracts (WebApi)

- Live in `WebApi/Contracts/{Feature}/`.
- `sealed record` for both requests and responses.
- Introduce a response contract **only** when the API shape differs from the Application response (different types, omitted fields, versioning, pagination).
- Introduce a request contract when the API input shape differs from the command, or when API-specific attributes are needed.

### DI registration

- Each layer has a `ServiceCollectionExtensions` static class using **C# extension block syntax**:

```csharp
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication() { ... }
    }
}
```

- Methods return `IServiceCollection` for chaining.

### Attributes

- Custom marker attributes are `public sealed class` with an empty body (`;`).
- Decorated with `[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]`.

### Middleware

- Primary constructor taking `RequestDelegate next` (and `ILogger<T>` if needed).
- Entry point is `InvokeAsync`. Scoped services can be injected as method parameters.
- Static `SkippedPathPrefixes` array + `ShouldSkip` private static helper for path exclusion.
- Error responses use `ProblemDetails` + `WriteAsJsonAsync`.

## Testing conventions

### Unit tests

- **xUnit** with `[Fact]`.
- Test class is `public sealed class`.
- Method naming: `Handle_ExpectedBehavior` (underscore-separated phrases).
- Handlers are private nested types — access them via reflection (`GetNestedType("Handler", BindingFlags.NonPublic)` + `Activator.CreateInstance`).
- Use real in-memory repositories from Infrastructure for handler tests.
- `TestInitializer` with `[ModuleInitializer]` scans Mapster configs and exposes a static `Mapper`.

### Integration tests

- Boot API in-memory via `Microsoft.AspNetCore.Mvc.Testing`.
- Test class inherits `BaseApiTest`, is `public sealed class`.
- Method naming: **snake_case** descriptions (e.g. `Get_test_returns_echo`).
- Use `GetFromJsonAsync`, `PostAsJsonAsync`, `ReadFromJsonAsync` with contract types.

## Build & run

```bash
dotnet test              # run all tests
dotnet run --project src/Template.WebApi   # start the API
```

## Comments policy

- Use **XML doc comments** (`///`) on domain types and their important members.
- Use **`//` line comments** sparingly for architectural reasoning (why, not what).
- Do not add comments that merely narrate what the code does.
