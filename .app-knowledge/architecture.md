# Architecture

## Stack

- **.NET 10** Web API (`net10.0`), SDK `10.0.100` rollForward `latestMajor` ([global.json](../global.json)).
- **MediatR** (CQRS dispatch), **Mapster** + `MapsterMapper.IMapper` (DI-injected, scoped).
- **Asp.Versioning** with `UrlSegmentApiVersionReader`.
- **Serilog** via Optimove `OptiLogger.Init`; debug-build console sink overrides config.
- **Swashbuckle** with per-version docs (`ConfigureSwaggerVersions`) and a tenant-headers operation filter.
- **Container**: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`. Build stage `mcr.microsoft.com/dotnet/sdk:10.0`. Datadog `CORECLR_*` env vars and `LD_PRELOAD` are wired in the final image.

No database engine is used — repositories are in-memory `ConcurrentDictionary`.

## Layers (dependencies point inward only)

```
WebApi  →  Infrastructure  →  Application  →  Domain
```

| Project | Path | References | Role |
|---|---|---|---|
| `Template.Domain` | [src/Template.Domain](../src/Template.Domain) | none | Entities + invariants. `TestModel`, `ZubiModel`, `ZabaModel`, `TenantContext`. |
| `Template.Application` | [src/Template.Application](../src/Template.Application) | Domain | CQRS slices under `Features/{Feature}/`, ports under `Abstractions/`, per-feature Mapster `IRegister` configs. |
| `Template.Infrastructure` | [src/Template.Infrastructure](../src/Template.Infrastructure) | Application | Adapters: in-memory repositories, `TenantApplicationContext`, outbound `TenantContextHandler` (`DelegatingHandler`). |
| `Template.WebApi` | [src/Template.WebApi](../src/Template.WebApi) | Application + Infrastructure | Composition root, controllers, contracts, middlewares, Swagger, kebab-case routing. |

`extension(IServiceCollection services)` blocks (C# 14 extension members) are used for the per-layer `Add…` registration helpers in each layer's `ServiceCollectionExtensions.cs`.

## Feature slices (current)

- **Test** — `GET /api/v{1,2}/test`, `GET /api/v1/test/error` (random 400 for chaos), `GET /api/v1/test/zibi`, `POST /api/v1/test` (V1 returns `Message + ProcessedAt`, V2 omits `ProcessedAt`). `POST` requires `x-user-name`.
- **Zubi** — full CRUD at `/api/v1/zubi` (GET list, GET by id, POST, PUT, DELETE).
- **Zaba** — full CRUD at `/api/v1/zaba` (same shape as Zubi).

Routes are kebab-cased via `KebabCaseParameterTransformer` registered in `AddApiControllers`. JSON uses `JsonNamingPolicy.CamelCase`.

## Pipeline (Program.cs, in order)

1. `RequestLoggingMiddleware` — wraps everything; logs method/path/status/elapsed and converts uncaught exceptions to `ProblemDetails 500`.
2. `UseCors("DefaultCors")`
3. `UseVersionedSwaggerUI`
4. `UseRouting`
5. `TenantContextMiddleware` — runs after routing so it can read endpoint metadata (`AllowWithNoTenantHeader`, `RequireUserNameHeader`).
6. `UseHttpsRedirection`
7. `MapControllers`
8. `MapHealthChecks("/healthcheck")`

## Multi-tenancy

- Inbound: `x-tenant-id` header is parsed as `int > 0`; otherwise `400 ProblemDetails`. Optional `x-user-name`. Both names live in `TenantHeaders` constants.
- Scoped `ITenantApplicationContext` is initialized per request; `Serilog.Context` is enriched with `TenantId` (and `UserName` when present).
- Skip rules: paths starting with `/swagger` or `/healthcheck`, or endpoints carrying `[AllowWithNoTenantHeader]`.
- Outbound: register an `HttpClient` with `TenantContextHandler` to auto-forward both headers on downstream calls.

## Tests

| Project | Subject |
|---|---|
| `Template.Domain.UnitTests` | Domain behavior (no scenarios committed yet). |
| `Template.Application.UnitTests` | `CreateTest`, `GetTest` handlers. |
| `Template.Infrastructure.UnitTests` | `TenantApplicationContext`. |
| `Template.WebApi.UnitTests` | `RequestLoggingMiddleware`. |
| `Template.IntegrationTests` | In-process via `Microsoft.AspNetCore.Mvc.Testing` (`TestEndpointsTests`). |
| `Template.Testing.Common` | Shared test config helpers. |

CI runs `dotnet dotcover test Template.sln --filter "FullyQualifiedName!~IntegrationTests"`; integration tests run via `docker-compose.integration-tests.yml` (service `template-integration-test`). See [ci.yml](../ci.yml).

## CI/CD workflows ([.github/workflows](../.github/workflows))

- `template-microservice-pr.yaml`, `template-microservice-ci.yaml`, `template-microservice-promote.yaml` — service build/promote pipelines.
- `app-knowledge-builder.yml` — runs Claude with the `app-knowledge-builder` skill on PRs merged to `dev`, commits results back.
