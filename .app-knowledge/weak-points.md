# Weak Points

## In-memory persistence — no durability
All three repositories (`TestRepository`, `ZubiRepository`, `ZabaRepository`) use `ConcurrentDictionary`. All data is lost on restart. This is intentional for a template, but any agent working on this should not assume real persistence semantics.

## No validation layer
There is no input validation (FluentValidation, DataAnnotations, etc.) on commands/queries. The only validation is `ZubiModel`/`ZabaModel`'s name guard (throws `ArgumentException`). Invalid inputs propagate to domain model creation and throw an unhandled exception → 500.

## `TestModel` has no factory method
Unlike `ZubiModel` and `ZabaModel`, `TestModel` uses a plain object initializer (`new TestModel { Value = ... }`). If invariants grow, this pattern is inconsistent.

## `GetTest` fallback creates ephemeral model
`GetTest.Handler` falls back to `new TestModel { Value = request.Value }` if nothing is found in the store. The returned `Echo` value is just the input — no actual storage lookup guarantees data freshness.

## No concurrency control for update
`ZubiRepository.UpdateAsync` and `ZabaRepository.UpdateAsync` do a `_store[id] = dto` under `ConcurrentDictionary` guarantees, but there is no optimistic concurrency (no ETag, no version field). Last-write-wins in concurrent updates.

## Single-file controller growth
`TestController` accumulates test endpoints (`/error`, `/zibi`) added via multiple small PRs. These feel like scaffolding artifacts rather than production endpoints — worth cleaning up before the template is used as a real base.

## Duplicate domain model structure
`ZubiModel` and `ZabaModel` are structurally identical (name, description, createdAt, updatedAt pattern). If a third entity with the same shape is added, consider a base class or generic pattern to avoid triplication.

## No integration tests for Zubi/Zaba controllers
`Template.IntegrationTests/Tests/TestEndpointsTests.cs` only covers `Test` endpoints. `ZubiController` and `ZabaController` have no integration test coverage.

## App-knowledge-builder workflow triggers on `dev` branch only
The `app-knowledge-builder.yml` workflow fires on PRs merged to `dev`. If the repo's default branch changes or work is merged directly to `main`, the knowledge base will not update.
