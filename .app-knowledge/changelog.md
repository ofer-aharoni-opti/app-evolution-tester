# Changelog

[2026-04-30 PR#1] Initial template: TestController with GET/POST v1/v2, TestModel, CreateTest/GetTest handlers, Clean Architecture scaffold.
[2026-04-30 PR#1] Added `GET /api/v1/test/error` endpoint (simulated random errors for testing).
[2026-04-30 PR#2] Added old app-evolution-tracker skill and workflow (later replaced).
[2026-04-30 PR#3–PR#8] Iterative changes to TestController (incremental test PRs; exact changes in error-endpoint wiring).
[2026-04-30 PR#9] Added Zubi CRUD: `ZubiModel`, `IZubiRepository`, `ZubiRepository`, full CQRS feature set (CreateZubi, GetZubi, ListZubis, UpdateZubi, DeleteZubi), `ZubiController`, Mapster configs.
[2026-04-30 PR#10] Updated app-evolution-tracker skill; added `.app-knowledge/evolution/` tracking files (old format).
[2026-04-30 PR#11–PR#12] Minor TestController changes (test PRs).
[2026-04-30 PR#13] Updated app-evolution-tracker skill and `.app-knowledge/evolution/architecture-current.md`.
[2026-04-30 PR#14] Minor TestController change (test PR).
[2026-04-30 PR#15] Added Zaba CRUD: `ZabaModel`, `IZabaRepository`, `ZabaRepository`, full CQRS feature set (CreateZaba, GetZaba, ListZabas, UpdateZaba, DeleteZaba), `ZabaController`, Mapster configs.
[2026-05-03 PR#16] Replaced old `app-evolution-tracker` skill/workflow with new `app-knowledge-builder` skill. Deleted `.app-knowledge/evolution/` directory. Updated GitHub Actions workflow.
[2026-05-03 PR#17] Fixed GitHub workflow (added `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24`, `id-token: write` permission).
[2026-05-03 PR#18–PR#23] Iterative TestController changes and workflow tweaks (test/config PRs).
[2026-05-03 PR#24] First local bootstrap run of `app-knowledge-builder`; created `.app-knowledge/` with all knowledge files and cursor.
[2026-05-06 PR#25–PR#28] TestController changes and workflow refinements; knowledge base updated incrementally.
[2026-05-06 PR#29] Changed model in `app-knowledge-builder.yml` workflow (model selection update).
[2026-05-06 PR#30–PR#31] Fixed push command in app-knowledge-builder workflow (sets remote URL with GITHUB_TOKEN before push).
[2026-05-06 PR#32] Adjusted simulated error trigger in `TestController.GetError`: modulo divisor changed from 46 to 45.
