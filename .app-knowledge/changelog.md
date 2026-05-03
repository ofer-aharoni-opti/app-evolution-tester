# Changelog

One-line entries, newest first. Substantive PRs only — `TEST N` placeholder PRs (#1–#8, #10–#14, #16, #18–#23) are omitted as they did not change the public surface.

- [2026-04-30 PR#15] Added Zaba CRUD slice (Domain `ZabaModel`, Application `Features/Zaba/*`, Infrastructure `ZabaRepository`/`ZabaDto`, WebApi `ZabaController` + contracts) — same shape as Zubi.
- [2026-04-30 PR#9] Added Zubi CRUD slice (`ZubiModel`, `Features/Zubi/*`, `ZubiRepository`/`ZubiDto`, `ZubiController` + contracts).
- [2026-05-03 PR#17] Added `.github/workflows/app-knowledge-builder.yml` to auto-update `.app-knowledge/` on every PR merged to `dev`.
