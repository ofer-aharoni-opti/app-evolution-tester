# Conventions

## CQRS / feature structure
- Features live in `Application/Features/{EntityName}/` — vertical slice per entity
- Each feature file is named for the operation: `CreateZubi.cs`, `GetZubi.cs`, `ListZubis.cs`, `UpdateZubi.cs`, `DeleteZubi.cs`
- Inside each file: `public static class CreateZubi` containing nested `Command`, `Response`, and private `Handler`
- `Command`/`Query` records implement `IRequest<Response>`
- `Handler` is `private sealed class` — not exposed outside the feature class
- Handler constructor uses primary constructors for dependency injection

## Mapping
- Mapster with DI (`IMapper`) — **not** static `TypeAdapter.Map`
- Mapping configs implement `IRegister` and are co-located with types: `ZubiMappingConfig.cs` next to the feature files
- Two scan points: Application assembly and WebApi assembly
- Infrastructure does NOT use Mapster — repository mapping is manual to preserve domain encapsulation

## Domain model conventions
- Private setters everywhere; state changes only through behavior methods
- Factory static methods: `Create(...)` for new, `Hydrate(...)` for reconstruction from storage
- `Hydrate` is for repository use only — never call from handlers or controllers
- `string.IsNullOrWhiteSpace` guard on required string fields; throws `ArgumentException`
- `Description` defaults to `string.Empty` (never null)

## Repository conventions
- Repositories store DTOs internally (`ConcurrentDictionary<key, Dto>`)
- Manual domain↔DTO mapping at repository boundary
- `ToDto`/`ToModel` private static methods in the repository class
- `UpdateAsync` returns `bool` — false when key not found (no upsert)
- `DeleteAsync` returns `bool` — false when key not found

## Controller conventions
- Thin controllers — no business logic
- Use `IMediator.Send(command)` for all operations
- Return Application response directly when shape matches API; introduce WebApi contract only when shapes diverge
- CRUD: GET list → 200, GET by id → 200 or 404, POST → 201 + Location, PUT → 200 or 404, DELETE → 204 or 404
- Route: `api/v{version:apiVersion}/[controller]` with kebab-case transformer
- All controllers are `sealed`; use primary constructors for `IMediator`/`IMapper`

## Error handling
- `RequestLoggingMiddleware` catches unhandled exceptions → returns 500 ProblemDetails + Error log
- `TenantContextMiddleware` returns 400 ProblemDetails for invalid headers
- Handlers return `null` for "not found" — controllers translate to 404
- No custom exception types or global exception filters currently in use

## Logging
- Serilog with `OptiLogger` (Optimove wrapper) for production
- Debug console logger for `#if DEBUG` builds
- `TenantId` and `UserName` enriched into log context per request
- Log level: Error ≥500, Warning 400–499, Info otherwise

## Testing patterns
- Unit tests: mock repositories/mappers via interfaces; test handler logic in isolation
- Integration tests: `WebApplicationFactory`-based, full HTTP round-trip, in-memory store
- `Template.Testing.Common` holds shared helpers (e.g., `ConfigurationExtensions`)

## Naming
- Entities: `{Name}Model` in Domain, `{Name}Dto` in Infrastructure, `{Name}Repository` in Infrastructure
- Interfaces: `I{Name}Repository`, `I{Name}ApplicationContext`
- Feature files: verb + noun (`CreateZubi`, `GetZubi`, `ListZubis`, `UpdateZubi`, `DeleteZubi`)
- Contracts: `Create{Name}Request`, `Create{Name}Response`, versioned as `Create{Name}ResponseV2`
