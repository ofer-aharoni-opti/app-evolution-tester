# Business Logic

## Domain models and invariants

### TestModel (`src/Template.Domain/Models/TestModel.cs`)
- Has `Value` (string, required), `IsProcessed` (bool), `ProcessedAt` (DateTime?)
- `ProcessedMessage` derived property: `"handled:{Value}"`
- `MarkAsProcessed()` sets `IsProcessed = true` and stamps `ProcessedAt = DateTime.UtcNow`
- State transition is one-way: no unprocess operation exists

### ZubiModel (`src/Template.Domain/Models/ZubiModel.cs`)
- Has `Id` (Guid), `Name` (string), `Description` (string), `CreatedAt` (DateTime), `UpdatedAt` (DateTime?)
- **Name is required**: `SetDetails` throws `ArgumentException` if name is null/whitespace
- Name is always trimmed; Description defaults to empty string if null
- `Create(name, description)` factory: generates new `Guid`, stamps `CreatedAt = DateTime.UtcNow`
- `Hydrate(id, name, description, createdAt, updatedAt)` factory: reconstructs from persisted state — for repository use only, never call from handlers
- `UpdateDetails(name, description)` sets `UpdatedAt = DateTime.UtcNow`; `UpdatedAt` is null until first update

### ZabaModel (`src/Template.Domain/Models/ZabaModel.cs`)
- Identical structure and invariants to `ZubiModel` (same factory pattern, same validation)
- Separate entity for separate CRUD resource (`/zaba` vs `/zubi`)

### TenantContext (`src/Template.Domain/Models/TenantContext.cs`)
- Value object: `TenantId` (int) + optional `UserName` (string?)
- Carried in scoped `ITenantApplicationContext` through the request pipeline

## Multi-tenancy rules
- Every request must include `x-tenant-id` header (positive integer) except:
  - `/swagger*` and `/healthcheck` paths
  - Endpoints decorated with `[AllowWithNoTenantHeader]`
- `x-user-name` header is optional globally but **required** on endpoints decorated with `[RequireUserNameHeader]`
- Missing or invalid tenant header → 400 ProblemDetails (not 401 — this is tenant routing, not auth)
- Tenant context is scoped to the request; populated by `TenantContextMiddleware`
- Outbound HTTP calls via any registered `HttpClient` automatically carry `x-tenant-id` and `x-user-name` via `TenantContextHandler` DelegatingHandler

## CRUD semantics (Zubi / Zaba)
- GET by id: returns 404 when not found (handler returns null → controller translates)
- POST: creates new resource, returns 201 with `Location` header pointing to the new resource
- PUT: updates existing; returns 404 if id not found; `UpdatedAt` always set on success
- DELETE: returns 204 on success, 404 if not found; `DeleteAsync` returns bool to distinguish

## Test feature
- `GetTest` echoes the provided `value` (fallback: creates a new model if not in store)
- `CreateTest` marks the model as processed and stores it; Application response differs from API contract
- `GET /api/v1/test/error` simulates random failures (returns 400 when `DateTime.Now.Second % 45 == 0`)
- `GET /api/v1/test/zibi` is an alias for the standard GET — echoes value
