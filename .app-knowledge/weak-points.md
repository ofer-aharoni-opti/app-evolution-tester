# Weak points and gotchas

## Persistence is in-memory only

All three repositories use a `ConcurrentDictionary`. Implications:

- **Process restart wipes everything.** Treat this as a template, not a service. Any consumer of the template must replace the adapter before going live.
- **No cross-instance consistency.** Horizontal scaling will split state across pods.
- The repository singletons are shared across all tenants — there is **no tenant scoping in the store**. A `ZubiModel` created under tenant 7 is visible to tenant 8. Multi-tenant safety lives only in the surrounding HTTP plumbing today.

## Domain validation surfaces as 500

`ZubiModel.SetDetails`/`ZabaModel.SetDetails` throw `ArgumentException` for empty `name`. There is no MVC model validation filter or MediatR pipeline behavior that catches these — they propagate to `RequestLoggingMiddleware` and become `500 ProblemDetails`. Clients see a server error for what is really a `400`. Adding FluentValidation or a global exception-mapping filter is a natural next step.

## Test-only chaos endpoint is in production code

`GET /api/v1/test/error` returns `400` randomly when `DateTime.Now.Second % 49 == 0`. Useful for exercising error logging in dev/CI but should not exist on a real service — gate it behind environment, an attribute, or remove before forking the template for a real workload.

## TestController has accumulated demo cruft

`TestController` carries: `Get`, `GetError`, `GetZibi` (a duplicate of `Get`), `Post` (V1), `PostV2`. The `zibi` route adds nothing functionally and exists as a demo of routing. Remove these when adapting the template.

## Test coverage is thin

- No domain-model unit tests (`Template.Domain.UnitTests` ships with only generated `obj/` artifacts).
- Only `RequestLoggingMiddleware` is covered in `Template.WebApi.UnitTests`; `TenantContextMiddleware` — the most security-relevant component — has no committed unit tests.
- Integration tests cover only `Test*` endpoints (`TestEndpointsTests.cs`); Zubi and Zaba have **no integration coverage** despite being full CRUD surfaces.

## Two `IntegrationWebApplicationFactory` files

`tests/Template.IntegrationTests/IntegrationWebApplicationFactory.cs` and `tests/Template.IntegrationTests/Infrastructure/IntegrationWebApplicationFactory.cs` both exist. Likely a leftover from a refactor. Worth checking which one is referenced before adding new integration tests.

## Datadog APM is wired unconditionally in the Dockerfile

The final image sets `CORECLR_*` profiler env vars and `LD_PRELOAD` pointing at `/app/datadog/...` files that the Dockerfile **does not copy in**. Either the image is expected to be built by a Datadog-aware base layer, or the runtime fails to find the profiler and silently disables instrumentation. Verify before claiming APM works.

## CORS configuration

- `AllowCredentials()` combined with `AllowAnyHeader()`/`AllowAnyMethod()` is broad. If any endpoint should not accept credentialed cross-origin requests, this is too permissive.
- `ALLOW_CORS=true` is a kill-switch that opens dev origins in any environment. Make sure it's never set in prod.

## API version coupling in `CreatedAtAction`

Both Zubi and Zaba `Create` hardcode `version = "1.0"` in the route values. When a v2 surface is added, this will keep generating v1 Location headers. Refactor to read the current request's `ApiVersion` rather than literal-stringing it.

## C# 14 extension members

`extension(IServiceCollection services)` blocks are used for the `Add…` helpers across the layers. This is preview/early syntax — IDE support and analyzer behavior may differ across tooling versions. If you bump the SDK and these stop compiling, that's the cause.
