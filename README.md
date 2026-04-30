## dotnet-template

Opinionated **.NET 10 Web API template** built around:

- **Clean Architecture** (dependencies point inward; domain at the center)
- **CQRS with MediatR** (features as commands/queries + handlers)
- **Mapster** for object mapping with DI-injected `IMapper`
- **Fast, testable APIs** (thin controllers; unit + integration tests)

Requires **.NET SDK 10** (projects target `net10.0`).

## Architecture overview

```
┌──────────────────────────────────────┐
│            WebApi (Host)             │  Controllers, Contracts, Composition Root
│  ┌────────────────────────────────┐  │
│  │        Infrastructure          │  │  Repositories, DTOs, External Services
│  │  ┌──────────────────────────┐  │  │
│  │  │       Application        │  │  │  Use Cases (Commands/Queries), Abstractions
│  │  │  ┌────────────────────┐  │  │  │
│  │  │  │      Domain        │  │  │  │  Entities, Business Logic, Value Objects
│  │  │  └────────────────────┘  │  │  │
│  │  └──────────────────────────┘  │  │
│  └────────────────────────────────┘  │
└──────────────────────────────────────┘
```

**Dependencies point inward only.** Outer layers reference inner layers, never the reverse.

## Project structure

### `src/Template.Domain` — Entities & Business Logic

The innermost layer. **Zero dependencies** — no project references, no infrastructure packages.

Contains:
- **Models** — Domain entities with encapsulated state and behavior

```
Domain/
├── Models/
│   ├── TestModel.cs          ← Entity with business logic (MarkAsProcessed, ProcessedMessage)
│   └── TenantContext.cs      ← Value object: tenant ID + optional username
└── AssemblyReference.cs       ← Assembly marker
```

Domain models protect their invariants through private setters and behavior methods.
External code must go through public methods to change state — this ensures
business rules are enforced by the domain itself, not by handlers or controllers.

### `src/Template.Application` — Use Cases & Abstractions

Orchestrates domain logic. References **Domain only**.

Contains:
- **Features** — Commands, Queries, Handlers, Responses (CQRS with MediatR)
- **Abstractions** — Port interfaces implemented by Infrastructure

```
Application/
├── Abstractions/
│   ├── ITestRepository.cs            ← Port: defines what the use case needs from persistence
│   └── ITenantApplicationContext.cs   ← Port: scoped tenant context abstraction
├── Constants/
│   └── TenantHeaders.cs             ← Header name constants shared across layers
├── Features/
│   └── Test/
│       ├── CreateTest.cs          ← Command + Response + Handler
│       ├── GetTest.cs             ← Query + Response + Handler
│       └── TestMappingConfig.cs   ← Mapster config (Domain → Application Response)
├── AssemblyReference.cs
└── ServiceCollectionExtensions.cs
```

Handlers depend on abstractions (`ITestRepository`, `IMapper`) — never on concrete
infrastructure. This is the core of dependency inversion.

### `src/Template.Infrastructure` — Adapters & Persistence

Implements the abstractions defined in Application. References **Application** (gets Domain transitively).

Contains:
- **Persistence** — Repository implementations (adapters)
- **Dto** — Persistence DTOs (flat representations for storage)
- **MultiTenancy** — Scoped tenant context and outbound header propagation

```
Infrastructure/
├── Persistence/
│   └── TestRepository.cs          ← Adapter: implements ITestRepository with in-memory store
├── Dto/
│   └── TestDto.cs                 ← Persistence DTO, never exposed outside Infrastructure
├── MultiTenancy/
│   ├── TenantApplicationContext.cs ← Scoped implementation of ITenantApplicationContext
│   └── TenantContextHandler.cs    ← DelegatingHandler: propagates x-tenant-id to downstream HTTP calls
└── ServiceCollectionExtensions.cs
```

Repositories store DTOs internally and map to/from domain models at the boundary.
Domain models are reconstructed through their public API (calling behavior methods),
not by setting private properties directly. This preserves domain invariants.

### `src/Template.WebApi` — HTTP API Host

The outermost layer and composition root. References **Application + Infrastructure**.

Contains:
- **Controllers** — Thin, delegate to MediatR; versioned via `[ApiVersion]`
- **Contracts** — API-specific request/response models (only when needed), per-version variants
- **Middlewares** — Request logging, tenant context extraction
- **Attributes** — Endpoint metadata (`AllowWithNoTenantHeader`, `RequireUserNameHeader`)
- **Swagger** — Versioned doc generation and tenant header operation filters
- **Routing** — Kebab-case route transformation
- **Program.cs** — Composition root that wires all layers

```
WebApi/
├── Attributes/
│   ├── AllowWithNoTenantHeader.cs   ← Opt-out: skip tenant requirement
│   └── RequireUserNameHeader.cs     ← Opt-in: require x-user-name header
├── Controllers/
│   └── TestController.cs
├── Contracts/
│   └── Test/
│       ├── CreateTestRequest.cs
│       ├── CreateTestResponse.cs       ← V1 response (Message + ProcessedAt)
│       ├── CreateTestResponseV2.cs     ← V2 response (Message only — breaking change)
│       └── TestContractMappingConfig.cs
├── Middlewares/
│   ├── RequestLoggingMiddleware.cs ← Logs method/path/status/elapsed; returns ProblemDetails on 500
│   └── TenantContextMiddleware.cs ← Extracts tenant from headers, enriches log context
├── Swagger/
│   ├── ConfigureSwaggerVersions.cs      ← Generates per-version Swagger docs
│   └── TenantHeadersOperationFilter.cs  ← Adds tenant headers to Swagger UI
├── Program.cs
└── ServiceCollectionExtensions.cs
```

## Models, DTOs, and Contracts

Each layer has its own model type with a clear purpose:

| Layer | Type | Purpose | Example |
|---|---|---|---|
| **Domain** | Entity / Model | Business logic + state | `TestModel` |
| **Application** | Command / Query / Response | Use case input/output | `CreateTest.Command`, `CreateTest.Response` |
| **Infrastructure** | DTO | Flat persistence representation | `TestDto` |
| **WebApi** | Contract | API-specific request/response | `CreateTestRequest`, `CreateTestResponse`, `CreateTestResponseV2` |

### When to introduce a WebApi response contract

The Application `Response` is the use case output port. The controller can return it directly
when its shape matches the API — **no extra contract needed**.

Introduce a WebApi response contract **only when the API shape diverges**:
- Different property types (e.g. `DateTime?` → `string` for ISO formatting)
- Omitting internal fields (e.g. dropping `IsProcessed` from the API surface)
- API versioning, pagination wrappers, HATEOAS links

**Example — no contract needed (GET):**

The controller returns `GetTest.Response` directly because it matches the API shape exactly.

**Example — contract needed (POST):**

`CreateTest.Response` has `(Message, IsProcessed, ProcessedAt as DateTime?)`.
The API should expose `CreateTestResponse(Message, ProcessedAt as string)` —
different shape, so a WebApi contract is introduced with a Mapster mapping config.

### When to introduce a WebApi request contract

A request contract decouples the API input from the Application command.
Use one when:
- The API input shape differs from the command
- You need API-specific attributes (Swagger annotations, validation)
- The command carries framework types (e.g. `: IRequest<Response>`) that shouldn't be exposed

For simple cases where the shapes match, binding directly to the command is also valid.

## CQRS conventions (MediatR)

Features live in `Application/Features/**` and follow a vertical slice pattern:

- **Query**: `record ... : IRequest<Response>` for reads
- **Command**: `record ... : IRequest<Response>` for writes
- **Handler**: private `IRequestHandler<..., ...>` with injected dependencies
- **Response**: application-level DTO returned to the caller
- **MappingConfig**: per-feature Mapster config (co-located, not global)

MediatR handlers are registered from the Application assembly:
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>());
```

## Mapster (object mapping)

Mapster is configured with the DI approach:
- `TypeAdapterConfig` registered as singleton
- `IMapper` (via `ServiceMapper`) registered as scoped
- Both Application and WebApi assemblies are scanned for `IRegister` configs

```csharp
services.AddSingleton(config);
services.AddScoped<IMapper, ServiceMapper>();
```

Mapping configs are **per-feature**, co-located with the types they map:
- `Application/Features/Test/TestMappingConfig.cs` — Domain → Application
- `WebApi/Contracts/Test/TestContractMappingConfig.cs` — Application → WebApi

Infrastructure does **not** use Mapster. Repository mapping is manual to respect
domain encapsulation (private setters, behavior methods).

## Multi-tenancy

Every request (except healthcheck, swagger, and endpoints marked with `[AllowWithNoTenantHeader]`)
must include an `x-tenant-id` header. The `TenantContextMiddleware` validates the header, initializes
a scoped `ITenantApplicationContext`, and enriches the Serilog log context with `TenantId`.

Endpoints that also require a username can be decorated with `[RequireUserNameHeader]`.

**Middleware pipeline order** (in `Program.cs`):
```
RequestLoggingMiddleware → UseCors → UseSwagger → UseRouting → TenantContextMiddleware → MapControllers
```

`RequestLoggingMiddleware` wraps the entire pipeline so it captures the final status code
and elapsed time of every request (see [Request logging](#request-logging)).
`TenantContextMiddleware` runs after `UseRouting` so endpoint metadata (attributes) is available.

**Outbound propagation**: `TenantContextHandler` (a `DelegatingHandler`) automatically forwards
`x-tenant-id` and `x-user-name` to any downstream HTTP calls made via `HttpClient`.

## CORS

CORS is configured via `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [],
  "AllowedOriginSuffixes": [".optimove.net"],
  "DevAllowedOrigins": ["http://localhost:4500"]
}
```

- `AllowedOrigins` — exact origin matches
- `AllowedOriginSuffixes` — suffix-based matching (e.g., any `*.optimove.net` subdomain)
- `DevAllowedOrigins` — only allowed in Development or when `ALLOW_CORS=true` env var is set

## API versioning

URL-based versioning via [`Asp.Versioning`](https://github.com/dotnet/aspnet-api-versioning):

```
/api/v{version}/[controller]   →   /api/v1/test, /api/v2/test
```

Configuration:
- **Default version**: 1.0 (assumed when unspecified)
- **Version reader**: URL segment (`UrlSegmentApiVersionReader`)
- **`api-supported-versions` / `api-deprecated-versions`** response headers are included automatically

Adding a new version:
1. Add `[ApiVersion("X.0")]` to the controller
2. Add `[MapToApiVersion("X.0")]` to the new action method
3. Create a versioned response contract if the shape diverges (e.g. `CreateTestResponseV2`)

Swagger automatically discovers all declared versions and generates a separate doc per version
(`/swagger/v1/swagger.json`, `/swagger/v2/swagger.json`, …) via `ConfigureSwaggerVersions`.

## Request logging

`RequestLoggingMiddleware` is the first middleware in the pipeline. It:
- Measures elapsed time with `Stopwatch.GetTimestamp()`
- Logs `HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms`
- Selects log level by status code: **Error** for 500+, **Warning** for 400+, **Information** otherwise
- On unhandled exceptions: returns a `ProblemDetails` JSON response (500) and logs at Error
- Skips `/swagger` and `/healthcheck` paths to reduce noise

## Port / Adapter pattern

The core of dependency inversion:

1. **Application** defines interfaces (ports): `ITestRepository`
2. **Infrastructure** implements them (adapters): `TestRepository`
3. **WebApi** wires them in DI: `services.AddSingleton<ITestRepository, TestRepository>()`

To swap persistence (e.g. from in-memory to PostgreSQL), create a new adapter in
Infrastructure and change one line of DI registration. Application and Domain stay untouched.

## Testing

### Unit tests

| Project | Tests | References |
|---|---|---|
| `Template.Domain.UnitTests` | Domain entity behavior | Domain |
| `Template.Application.UnitTests` | Handler logic | Application + Infrastructure |
| `Template.Infrastructure.UnitTests` | Repository / adapter logic | Infrastructure |

### Integration tests

Boot the API in-memory via `Microsoft.AspNetCore.Mvc.Testing`:

- `tests/Template.IntegrationTests/Tests/TestEndpointsTests.cs`

Exercises HTTP endpoints, model binding, validation, and end-to-end MediatR dispatch.

## Common commands

Run all tests:

```bash
dotnet test
```

Run the API:

```bash
dotnet run --project src/Template.WebApi
```
