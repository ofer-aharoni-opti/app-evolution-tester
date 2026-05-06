# Important — Must-Not-Break

## Dependency direction
The inward-only dependency rule is a hard invariant. Domain must never reference Application, Infrastructure, or WebApi. Application must never reference Infrastructure or WebApi. Violating this collapses the architecture.

## Domain encapsulation
- Never set domain model properties directly from outside the model (all setters are private).
- Always use `Create(...)` for new instances; `Hydrate(...)` for reconstruction from storage — never bypass these.
- `Hydrate` must only be called from repositories. Calling it from handlers/controllers breaks timestamp integrity.

## Tenant header enforcement
- `TenantContextMiddleware` runs **after** `UseRouting` so endpoint metadata (attributes) is resolved. Moving it before `UseRouting` would break `[AllowWithNoTenantHeader]` and `[RequireUserNameHeader]` detection.
- `RequestLoggingMiddleware` must remain **first** — it wraps the entire pipeline to capture final status codes including those set by `TenantContextMiddleware`.

## Repository registration as singletons
- `ITestRepository`, `IZubiRepository`, `IZabaRepository` are registered as **singletons** because the in-memory stores (`ConcurrentDictionary`) must survive across requests. Changing to scoped would cause data loss on each request.
- When replacing with a real DB, switch to scoped/transient as appropriate, but update DI registration accordingly.

## ZubiRepository / ZabaRepository `UpdateAsync` semantics
- Returns `false` (no update) when the key doesn't exist — this is not an upsert. The handler uses this to return null → 404. Changing to upsert would silently create resources on PUT.

## `UpdatedAt` null-safety in Mapster
- `UpdateZubi.Response.UpdatedAt` is non-nullable `DateTime`.
- The Mapster config explicitly maps `src.UpdatedAt!.Value` — this is safe only because `UpdateDetails` always sets `UpdatedAt` before `UpdateAsync` is called. Never call `UpdateAsync` on a model without first calling `UpdateDetails`.

## Simulated error endpoint
- `GET /api/v1/test/error` returns 400 when `DateTime.Now.Second % 45 == 0`. This is intentional for testing the evolution tracker — do not remove as a "dead code" cleanup unless confirmed.

## `OptiLogger` dependency
- `Program.cs` uses `Optimove.Infrastructure.Logger.OptiLogger.Init(...)`. This is an Optimove-internal package. Removing or changing logging setup without access to that package will break the build.
