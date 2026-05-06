# App Knowledge — dotnet-template

Opinionated **.NET 10 Web API template** (repo: `ofer-aharoni-opti/app-evolution-tester`) implementing Clean Architecture + CQRS with MediatR. Currently exposes three REST resources — **Test** (echo/process), **Zubi** (full CRUD), and **Zaba** (full CRUD) — backed by in-memory stores. Multi-tenancy is enforced via `x-tenant-id` header on all endpoints except health/swagger. The repo doubles as a test harness for the `app-knowledge-builder` CI skill.

## Routing table

| If you want to know about… | Read |
|---|---|
| Layer structure, projects, deployment, tech stack | `architecture.md` |
| Domain rules, entity invariants, CRUD semantics, tenant enforcement | `logic.md` |
| Request lifecycle, create/update flows, adding a new entity | `flows.md` |
| Naming, CQRS patterns, mapping, error handling, testing patterns | `conventions.md` |
| Hot paths, must-not-break invariants, critical coupling | `important.md` |
| Fragile areas, missing tests, known tech debt | `weak-points.md` |
| Significant changes over time, deprecations | `changelog.md` |
| Open questions and unresolved gaps | `gaps.md` |

## Quick facts
- Default branch: `main` | Active dev branch: `dev`
- API base: `/api/v{version}/[controller]` (default v1)
- No real database — all data in-memory, lost on restart
- `app-knowledge-builder.yml` updates this folder automatically on every PR merged to `dev`
