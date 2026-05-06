# Architecture

## Stack
- **.NET 10** Web API (`net10.0`)
- **Clean Architecture** — dependencies point inward; Domain at center
- **CQRS** via **MediatR** — features as Commands/Queries + Handlers
- **Mapster** for object mapping with DI-injected `IMapper`
- **Serilog** + `OptiLogger` (Optimove internal logger wrapper)
- **Asp.Versioning** — URL-segment API versioning
- **xUnit** for unit and integration tests; `Microsoft.AspNetCore.Mvc.Testing` for in-process integration tests

## Layer diagram

```
WebApi (Host)               ← Controllers, Contracts, Middlewares, Composition Root
  └─ Infrastructure         ← Repositories, DTOs, MultiTenancy adapters
       └─ Application       ← Use Cases (Commands/Queries/Handlers), Port interfaces
            └─ Domain       ← Entities, Business Logic, Value Objects (zero dependencies)
```

Dependency rule: outer layers reference inner layers only. Domain has **no project references**.

## Projects

| Project | Layer | Key responsibility |
|---|---|---|
| `Template.Domain` | Domain | `TestModel`, `ZubiModel`, `ZabaModel`, `ZibiModel`, `TenantContext` |
| `Template.Application` | Application | Feature handlers, port interfaces (`ITestRepository`, `IZubiRepository`, `IZabaRepository`, `IZibiRepository`, `ITenantApplicationContext`) |
| `Template.Infrastructure` | Infrastructure | In-memory repositories (`ConcurrentDictionary`), `TenantApplicationContext`, `TenantContextHandler` (DelegatingHandler) |
| `Template.WebApi` | Host | Controllers, Middlewares, Swagger config, composition root (`Program.cs`) |

## Test projects

| Project | Scope |
|---|---|
| `Template.Domain.UnitTests` | Domain entity behavior |
| `Template.Application.UnitTests` | Handler logic |
| `Template.Infrastructure.UnitTests` | Repository / adapter logic |
| `Template.WebApi.UnitTests` | Middleware tests |
| `Template.IntegrationTests` | Full HTTP round-trip via `WebApplicationFactory` |
| `Template.Testing.Common` | Shared test helpers |

## Persistence
All repositories are **in-memory** (`ConcurrentDictionary`). No database. Registered as **singletons** in DI, so data lives for the process lifetime. Swapping to a real DB requires creating a new adapter in Infrastructure and changing one `AddSingleton` call.

## Deployment surface
- Dockerfile at repo root — builds the WebApi
- `docker-compose.local.yml` — local dev
- `docker-compose.integration-tests.yml` — integration test runner
- `.deploy/values.yaml` — Helm values (Kubernetes deployment)
- CI/CD: `.github/workflows/template-microservice-ci.yaml`, `template-microservice-pr.yaml`, `template-microservice-promote.yaml`

## API versioning
URL segment: `/api/v{version}/[controller]`. Default: 1.0. Currently `TestController` exposes v1 and v2; `ZubiController`, `ZabaController`, and `ZibiController` are v1 only.

## Middleware pipeline order
```
RequestLoggingMiddleware → UseCors → UseSwagger → UseRouting → TenantContextMiddleware → MapControllers
```
