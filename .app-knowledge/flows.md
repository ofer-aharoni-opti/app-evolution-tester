# Key flows

All flows assume the request hits the pipeline order described in [architecture.md](architecture.md): `RequestLogging → CORS → Swagger → Routing → TenantContext → HttpsRedirect → MapControllers`.

## 1. Tenant resolution (every API request)

Trigger: any HTTP request not under `/swagger`/`/healthcheck`.

1. `RequestLoggingMiddleware` records `Stopwatch.GetTimestamp()` and `try`-wraps `next`.
2. `TenantContextMiddleware.InvokeAsync` reads endpoint metadata. If `[AllowWithNoTenantHeader]` is present, skip tenant validation.
3. Parse `x-tenant-id` as `int > 0`. Fail → write `ProblemDetails 400` and return.
4. Read optional `x-user-name`. If endpoint has `[RequireUserNameHeader]` and value is empty → `ProblemDetails 400`.
5. `tenantContext.Initialize(tenantId, userName)` (scoped instance for this request).
6. Push `TenantId` (and `UserName`) into Serilog `LogContext`, then `await next`.
7. `RequestLoggingMiddleware` logs `HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms` at the level dictated by status code.

End state: handler ran with a populated `ITenantApplicationContext` and tenant-aware logs.

Code: [TenantContextMiddleware.cs](../src/Template.WebApi/Middlewares/TenantContextMiddleware.cs), [RequestLoggingMiddleware.cs](../src/Template.WebApi/Middlewares/RequestLoggingMiddleware.cs).

## 2. Create Test (POST /api/v1/test)

Trigger: `POST /api/v1/test` with `x-tenant-id` and `x-user-name` headers.

1. `TestController.Post` (`[RequireUserNameHeader]`, `[MapToApiVersion("1.0")]`) receives `CreateTestRequest`.
2. Mapster maps `CreateTestRequest → CreateTest.Command`.
3. `CreateTest.Handler` constructs `new TestModel { Value }`, calls `MarkAsProcessed()`, persists via `ITestRepository.AddAsync`, maps the model to `CreateTest.Response` via `IMapper`.
4. Controller maps `CreateTest.Response → CreateTestResponse` (drops `IsProcessed`, formats `ProcessedAt` as string), returns `200 OK`.

V2 variant: `[MapToApiVersion("2.0")]` returns `CreateTestResponseV2(Message)` — `ProcessedAt` is intentionally absent.

Code: [TestController.cs](../src/Template.WebApi/Controllers/TestController.cs), [CreateTest.cs](../src/Template.Application/Features/Test/CreateTest.cs).

## 3. Get Test (GET /api/v1/test?value=…)

1. `TestController.Get` sends `GetTest.Query(value)`.
2. `GetTest.Handler` calls `ITestRepository.GetByValueAsync`. On miss, falls back to `new TestModel { Value }` — never returns 404.
3. Mapster maps to `GetTest.Response(Echo)` and returns `200 OK`.

Sister endpoints share the same logic: `/api/v1/test/zibi` (alias) and `/api/v1/test/error` (returns `400` when `DateTime.Now.Second % 49 == 0`, else normal response — used for chaos/observability testing).

## 4. Zubi/Zaba CRUD (identical shape)

Routes: `/api/v1/zubi`, `/api/v1/zaba`. Both controllers are structurally identical.

- **GET** (list) → `ListZubis.Query` / `ListZabas.Query` → `IRepository.GetAllAsync()` → `200` with snapshot list.
- **GET {id:guid}** → `GetZubi.Query(id)` → `GetByIdAsync` → `200` or `404`.
- **POST** → maps `CreateZubiRequest → CreateZubi.Command` → handler calls `ZubiModel.Create(name, description)` → `AddAsync` → returns `201 Created` with `Location: /api/v1/zubi/{id}` via `CreatedAtAction(nameof(GetById), new { id, version = "1.0" }, response)`.
- **PUT {id:guid}** → fetch existing model via `GetByIdAsync`; if null → `404`; else call `UpdateDetails(name, description)` and `UpdateAsync`; return `200` with response or `404` if `UpdateAsync` returned false.
- **DELETE {id:guid}** → `DeleteAsync` → `204 NoContent` on success, `404` if id was unknown.

Code: [ZubiController.cs](../src/Template.WebApi/Controllers/ZubiController.cs), [ZabaController.cs](../src/Template.WebApi/Controllers/ZabaController.cs), [Features/Zubi](../src/Template.Application/Features/Zubi), [Features/Zaba](../src/Template.Application/Features/Zaba).

## 5. Outbound HTTP with tenant propagation

Trigger: any handler resolves an `HttpClient` registered with `TenantContextHandler` as a `DelegatingHandler`.

1. The handler's `SendAsync` reads the scoped `ITenantApplicationContext.Context`.
2. If the outgoing request does not already contain `x-tenant-id`, it is added from the context. Same for `x-user-name` when non-empty.
3. The request is forwarded to the inner handler.

Caller-set headers are never overwritten.

Code: [TenantContextHandler.cs](../src/Template.Infrastructure/MultiTenancy/TenantContextHandler.cs).

## 6. Unhandled exception → 500

1. Handler throws.
2. `RequestLoggingMiddleware` catches, and if `Response.HasStarted == false` writes a `ProblemDetails` JSON body with `Status = 500`.
3. The log entry is emitted at `Error` with the exception attached.

If the response has already started, only the log line is written — the bytes already on the wire are kept.
