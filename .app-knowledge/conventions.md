# Conventions

## CQRS slices (vertical-slice + MediatR)

- One static class per use case in `Application/Features/{Feature}/{UseCase}.cs` containing the public `Command`/`Query`, the public `Response`, and a **private** `Handler`. Pattern is uniform across `Test`, `Zubi`, `Zaba`.
- Commands and queries are `sealed record` with `: IRequest<Response>`. Responses are `sealed record`.
- Handlers take dependencies via primary constructors (`(IRepo repository, IMapper mapper)`).
- MediatR registers handlers from the Application assembly only:
  `services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>())`.

## Mapping (Mapster)

- `IMapper` is **scoped**, `TypeAdapterConfig` is **singleton**. WebApi scans both `Application` and `WebApi` assemblies for `IRegister` configs.
- Configs are **co-located, per-feature**:
  - `Application/Features/{Feature}/{Feature}MappingConfig.cs` — Domain → Application Response.
  - `WebApi/Contracts/{Feature}/{Feature}ContractMappingConfig.cs` — Application → WebApi contract.
- **Infrastructure does not use Mapster.** Repositories map `Model ↔ DTO` manually so that domain models are reconstructed only via their public API (`Hydrate`/`Create`), preserving encapsulation. Do not introduce Mapster in Infrastructure.

## Models, DTOs, Contracts — when to add what

| Layer | Type | Add when |
|---|---|---|
| Domain | Entity | Always — one per aggregate. |
| Application | Command/Query/Response | Always — one per use case. |
| Infrastructure | DTO | When the persistence shape differs from the domain (here, always — flat record with public init properties). |
| WebApi | Request contract | API input differs from the command, or you need API-only attributes. Otherwise the controller can bind directly to the command. |
| WebApi | Response contract | API shape differs from `Application.Response` (different types, omitted fields, versioning). Otherwise return `Application.Response` directly. |

`TestController.Get` is the canonical "no contract needed" case; `TestController.Post` (V1 + V2) is the canonical "contract reshapes the response" case.

## Routing and JSON

- URL versioning only: `[Route("api/v{version:apiVersion}/[controller]")]`. Default version `1.0` is assumed.
- Controller route tokens are **kebab-cased** by `KebabCaseParameterTransformer` ([Routing/](../src/Template.WebApi/Routing/)). JSON property naming is **camelCase**.
- Add a new version: `[ApiVersion("X.0")]` on the controller, `[MapToApiVersion("X.0")]` on the new action, plus a versioned response contract if the shape diverges.

## Errors

- Validation/precondition failures from middleware → `ProblemDetails` JSON (currently `400` for tenant header issues, `500` for uncaught exceptions). `services.AddProblemDetails()` is registered globally.
- Repository "not found" surfaces as a `null`/`false` return; **handlers do not throw** for missing rows — controllers translate to `404`.
- Domain invariant violations throw `ArgumentException` (e.g. empty `name` in `ZubiModel.SetDetails`). Currently these will bubble up to `RequestLoggingMiddleware` and become `500` — there is no global validation filter.

## DI registration

- Each layer exposes an `extension(IServiceCollection services)` block in its `ServiceCollectionExtensions.cs`. The composition root in `Program.cs.ConfigureServices` calls them in order: `AddProblemDetails`, `AddHealthChecks`, `AddMemoryCache`, `AddApplication`, `AddApiControllers`, `AddApiVersioningConfiguration`, `AddCorsConfiguration`, `AddMapster`, `AddInfrastructure`, `AddSwagger`.
- All repositories are registered as **singletons** (in-memory store needs cross-request lifetime). `ITenantApplicationContext` is **scoped**. `TenantContextHandler` is **transient** (DelegatingHandler convention).

## Tenant-header attributes

- `[AllowWithNoTenantHeader]` — apply on actions or controllers that must work without `x-tenant-id` (e.g. unauthenticated/public endpoints). Default is "tenant required".
- `[RequireUserNameHeader]` — apply on actions that must also have `x-user-name`.

## Testing

- xUnit-style projects under [tests/](../tests). Naming: `{Project}.UnitTests`, plus `Template.IntegrationTests` for in-process API tests via `Microsoft.AspNetCore.Mvc.Testing` and `Template.Testing.Common` for shared helpers.
- Integration tests are **excluded from the unit-test CI pass** (`--filter "FullyQualifiedName!~IntegrationTests"`) and run separately via Docker Compose.

## Logging

- Use Serilog through DI's `ILogger<T>`. Don't call `Log.Logger` directly outside of `Program.cs` startup/teardown.
- Don't log inside hot middleware paths beyond what `RequestLoggingMiddleware` already provides.
