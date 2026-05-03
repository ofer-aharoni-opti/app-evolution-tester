# Domain logic and invariants

## Tenant context

- `x-tenant-id` is **required** on every request **unless** the path matches `/swagger` or `/healthcheck` **or** the endpoint carries `[AllowWithNoTenantHeader]`. Missing/invalid → `400 ProblemDetails`. Valid values are integers `> 0`.
- `x-user-name` is required only when the endpoint carries `[RequireUserNameHeader]`. Missing/empty → `400 ProblemDetails`.
- `ITenantApplicationContext.Initialize(tenantId, userName)` is called **once per request** by `TenantContextMiddleware`. After init, `Context` is the only safe accessor; do not read headers further down the stack.
- `TenantContextHandler` (outbound `DelegatingHandler`) only forwards a header if it is **not already present** on the outgoing request — caller-set values win.

## Domain entities — construction and mutation

- All domain models in `Template.Domain.Models` use `init`/`private set` and expose state changes through methods. **Never** mutate via reflection or repository back-doors.
- `ZubiModel` and `ZabaModel`:
  - `Create(name, description)` is the only way to build a new instance from scratch — generates `Id = Guid.NewGuid()` and `CreatedAt = DateTime.UtcNow`.
  - `Hydrate(id, name, description, createdAt, updatedAt)` is for repositories restoring persisted state and **must not** be called from handlers or controllers.
  - `UpdateDetails(name, description)` is the only way to mutate name/description after creation; it sets `UpdatedAt = DateTime.UtcNow`. `UpdatedAt` stays `null` until the first real update — readers can use this to distinguish "created" from "modified".
  - `name` is required: `null`/whitespace throws `ArgumentException`. `name` is trimmed; `description` defaults to `string.Empty` when `null`.
- `TestModel` exposes `MarkAsProcessed()` which sets `IsProcessed = true` and stamps `ProcessedAt = DateTime.UtcNow`. The derived `ProcessedMessage` is `$"handled:{Value}"`.

## Persistence semantics (in-memory)

- All repositories store DTOs (`TestDto`, `ZubiDto`, `ZabaDto`) in a `ConcurrentDictionary` and rebuild domain models via the entity's `Hydrate` factory at read time.
- `AddAsync` is upsert by id (`_store[id] = dto`).
- `UpdateAsync(model)` returns `false` if the row does not pre-exist (no implicit insert); otherwise overwrites and returns `true`. This keeps update/upsert semantics distinct.
- `DeleteAsync(id)` returns `true` only if a row was actually removed.
- `GetAllAsync` snapshots `_store.Values` before projecting, to avoid surfacing concurrent modifications mid-enumeration.

## API behavior

- `CreateTest` (V1) response: `(Message, IsProcessed, ProcessedAt as DateTime?)` is reshaped at the WebApi boundary into `CreateTestResponse(Message, ProcessedAt as string)` — the `string` form is for ISO formatting, and `IsProcessed` is intentionally dropped from the public surface.
- `CreateTest` V2 (`POST /api/v2/test`) is a deliberate breaking change: drops `ProcessedAt` from the response (`CreateTestResponseV2` exposes only `Message`).
- `GetTest` falls back to a fresh `TestModel { Value = request.Value }` when nothing is persisted — a missing record is **not** an error.
- `Zubi` / `Zaba` `POST` returns `201 Created` with a `Location` header pointing at the `GET {id}` route (versioned `1.0`). `PUT` and `GET {id}` return `404` when the id is unknown. `DELETE` returns `204 NoContent` on success, `404` if id was unknown.

## API versioning

- Default version is `1.0` and is assumed when unspecified. URL segment is the only version reader.
- `api-supported-versions` and `api-deprecated-versions` headers are emitted on every response (`ReportApiVersions = true`).
- `ConfigureSwaggerVersions` discovers all declared versions and emits one Swagger doc per version.

## Logging

- Log level by status code: `>=500` → Error, `>=400` → Warning, else Information. Caught exceptions are also Error.
- Log context is enriched with `TenantId` (and `UserName` if provided) for the duration of the request.
- `/swagger` and `/healthcheck` are excluded from request logging.
