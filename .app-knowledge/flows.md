# Key Flows

## 1. Inbound request lifecycle
1. `RequestLoggingMiddleware` starts stopwatch, wraps entire pipeline
2. `TenantContextMiddleware` (after routing) reads `x-tenant-id` and `x-user-name` from headers
3. If valid, calls `tenantContext.Initialize(tenantId, userName)` and pushes `TenantId`/`UserName` to Serilog log context
4. Controller receives request → dispatches via `IMediator.Send(command/query)`
5. MediatR locates handler → handler calls repository port → returns response
6. Controller maps Application response to WebApi contract (if shapes differ) → returns HTTP result
7. `RequestLoggingMiddleware` logs `HTTP {Method} {Path} {StatusCode} in {Elapsed}ms`; logs Error on 500, Warning on 400–499, Info otherwise

## 2. Create Zubi (representative CRUD create)
- `POST /api/v1/zubi` `{Name, Description}`
- `ZubiController.Create` → maps request to `CreateZubi.Command` via Mapster
- `CreateZubi.Handler`: calls `ZubiModel.Create(name, description)` → domain validates name → generates `Guid`, stamps `CreatedAt`
- `repository.AddAsync(model)` → `ZubiRepository` converts to `ZubiDto`, stores in `ConcurrentDictionary`
- Handler maps model to `CreateZubi.Response` via Mapster → returns 201 with `Location: /api/v1/zubi/{id}`

## 3. Update Zubi
- `PUT /api/v1/zubi/{id}` `{Name, Description}`
- Handler: `repository.GetByIdAsync(id)` → if null → return null → controller returns 404
- `model.UpdateDetails(name, description)` → domain validates, sets `UpdatedAt`
- `repository.UpdateAsync(model)` → only persists if key already exists (no upsert)
- Returns `UpdateZubi.Response` with non-nullable `UpdatedAt` (Mapster forces `.Value` on `UpdatedAt?`)

## 4. Tenant validation (guard)
- Any endpoint without `[AllowWithNoTenantHeader]` on non-skipped paths:
  - Missing header → 400 `"A valid 'x-tenant-id' header is required."`
  - Non-integer or `<= 0` → 400 same message
  - Endpoints with `[RequireUserNameHeader]` → additionally require `x-user-name`

## 5. Outbound HTTP propagation
- Services registered with `HttpClient` + `TenantContextHandler` as DelegatingHandler
- On each outbound call: adds `x-tenant-id` and `x-user-name` if not already present in request headers

## 6. API versioning (adding a new version)
1. Add `[ApiVersion("X.0")]` to controller
2. Add `[MapToApiVersion("X.0")]` to new action
3. Create `CreateTestResponseVX` contract in `Contracts/Test/` if shape diverges
4. Add Mapster config in `TestContractMappingConfig`
5. Swagger auto-discovers and generates `/swagger/vX/swagger.json`

## 7. Creating a new CRUD entity (Zubi/Zaba pattern)
1. Add domain model to `Template.Domain/Models/`
2. Add port interface to `Template.Application/Abstractions/`
3. Add feature handlers to `Template.Application/Features/{Entity}/`
4. Add Mapster mapping config co-located with features
5. Add DTO and repository adapter to `Template.Infrastructure/`
6. Register with `AddSingleton<IRepo, Repo>()` in `Template.Infrastructure/ServiceCollectionExtensions.cs`
7. Add request contracts (if shape differs) and controller to `Template.WebApi/`
