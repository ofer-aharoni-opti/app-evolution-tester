# app-evolution-tester — knowledge base

Opinionated **.NET 10 Web API template** built on **Clean Architecture + CQRS (MediatR) + Mapster**. Multi-tenant by header (`x-tenant-id`), URL-versioned (`/api/v{n}/…`), with in-memory persistence and Serilog-based request logging. Three feature slices ship today: **Test** (echo + V1/V2 demo), **Zubi** (CRUD), **Zaba** (CRUD). Container target is `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` with Datadog APM env hooks pre-wired.

## Routing — read these for…

| You want to know… | Read |
|---|---|
| Layer layout, projects, dependency direction, tech stack | [architecture.md](architecture.md) |
| Domain rules and behavioral invariants | [logic.md](logic.md) |
| Request/response sequences for the live endpoints | [flows.md](flows.md) |
| Naming, mapping, error handling, testing patterns | [conventions.md](conventions.md) |
| Hot paths and must-not-break invariants | [important.md](important.md) |
| Known fragile spots, tech debt, gotchas | [weak-points.md](weak-points.md) |
| Significant changes, deprecations, removals | [changelog.md](changelog.md) |
| Open questions the skill could not resolve | [gaps.md](gaps.md) |

State for incremental updates lives at [.state/cursor.json](.state/cursor.json).
