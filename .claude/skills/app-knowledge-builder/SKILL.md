---
name: app-knowledge-builder
description: Builds and maintains a living, structured knowledge base of an application — architecture, business logic, flows, conventions, hot paths, and weak points — under `.app-knowledge/` at the repo root, for AI agents working on the app. Use this skill whenever the user says "build app knowledge", "update app knowledge", "refresh app understanding", "onboard me to this app", "what does this app do", "explain the architecture", "what should I know before changing X", or after merging PRs. Also trigger whenever an AI agent is about to work on an unfamiliar codebase and needs to load context — read `.app-knowledge/INDEX.md` first if it exists. On first run (when `INDEX.md` is absent) ingests the full GitHub PR history; on subsequent runs processes only PRs merged since the last cursor. The knowledge base is a living snapshot of the current app, not a history book.
---
 
# App Knowledge Builder
 
Builds and maintains a living mental model of an application under `.app-knowledge/` at the repo root. The data is consumed by AI agents (and humans) who need to understand the app before changing it.

## Core principles

1. **Living snapshot, not history book.** Every main file describes the *current* app. When something is deprecated or removed, it is **deleted from the main files** and a one-line note is added to `changelog.md`. Never write "we used to do X, now we do Y" prose in the main files.
2. **No guessing.** When something is unclear, the skill first reads more code (related files, callers, tests, README, configs). Only if it genuinely cannot resolve the question does it append a line to `gaps.md`. It does *not* make up plausible-sounding explanations.
3. **GitHub-sourced.** PRs are the source of truth for what changed. Use the `gh` CLI for fetching them. The skill assumes `gh` is installed and authenticated; if not, it stops with a clear instruction to the user.
4. **Bounded files.** Each main file has a soft cap (~5 KB) and hard cap (~8 KB). Approaching the cap triggers consolidation, not appending. Rewriting trumps growing.
5. **Built for agent consumption.** `INDEX.md` is the entry point — terse, structured, with explicit "for X, read Y" pointers.
6. **Language- and layout-agnostic.** The skill makes **no assumptions** about programming language, framework, build system, or directory layout. It discovers all of that by inspection. It works equally well on a Python data pipeline, a .NET monolith, a Go microservice, a Rust CLI, a TypeScript monorepo, an iOS app, or anything else hosted on GitHub.
7. **Reader, not runner.** The skill never executes the application, never installs dependencies, never runs build steps. It only reads files (source, manifests, configs, READMEs, PR diffs) as text. This is what makes principle 6 safe — there's nothing to break across stacks.
6. **Runs in any repo.** No assumption about language, framework, layout, hosting, or PR practices beyond "this is a git repository." See *Portability* below.

---

## Portability — language- and layout-agnostic

The skill makes **no assumption about language, framework, directory layout, or how code is organized**. It works equally well in a Python monolith, a TypeScript monorepo, a Go service, a Rust library, a Java backend, a .NET solution, an iOS app, or anything else. The only hard requirement is "this is a git repository on GitHub."

Concretely, this means the skill:

- **Resolves all paths from `git rev-parse --show-toplevel`**, never from the current working directory. It works whether invoked from the repo root or any subdirectory.
- **Detects the stack from whatever artifacts happen to exist** — `package.json`, `pyproject.toml`, `go.mod`, `Cargo.toml`, `pom.xml`, `build.gradle`, `*.csproj`, `Gemfile`, `composer.json`, `Dockerfile`, top-level `README*`, etc. None of these are required. If none of the usual markers exist, it reads the directory tree and the largest source files to infer what the app is.
- **Does not assume any directory convention** — no expectation of `src/`, `app/`, `lib/`, a particular tests location, or any specific entry point. The actual layout is *discovered*, then described in `architecture.md`.
- **Detects the default branch dynamically** via `gh repo view --json defaultBranchRef -q .defaultBranchRef.name`. Never hardcodes `main` or `master`.

When the file content guides below mention things like "services" or "controllers", they are illustrative shorthand, not requirements. Adapt the vocabulary to whatever the app actually is.

---

## Two modes

The skill detects which mode to run based on the presence of `.app-knowledge/INDEX.md`:

- **Bootstrap mode** — `INDEX.md` does *not* exist → ingest the entire PR history and build everything from scratch.
- **Incremental mode** — `INDEX.md` exists → read `.state/cursor.json`, fetch only PRs merged since the last cursor, update affected files in place.

There is also a lightweight **read mode**: if a consuming agent only wants to *understand* the app (not update the knowledge), it should read `INDEX.md` and follow its pointers — no PR fetching needed.

---

## Output structure

```
.app-knowledge/
├── INDEX.md              # entry point for AI agents — read this first
├── architecture.md       # layers, services, modules, deployment topology
├── logic.md              # core business/domain rules — what the app does
├── flows.md              # key user/system flows (signup, payment, sync, etc.)
├── conventions.md        # naming, error handling, testing, team standards
├── important.md          # hot paths, must-not-break invariants, critical assumptions
├── weak-points.md        # fragile areas, known tech debt, gotchas, risk register
├── changelog.md          # rolling list of significant changes incl. deprecations
├── gaps.md               # unresolvable understanding gaps
└── .state/
    └── cursor.json       # last-processed PR, run metadata, stats
```

### What goes in each file

- **INDEX.md** — One-paragraph app summary, then a routing table: "If you want to know about X, read Y." Keep under 2 KB. Updated whenever the structure of `.app-knowledge/` changes.
- **architecture.md** — Component diagram in prose: services/modules, who calls whom, where the data lives, deployment surface. Tech stack at the top. No history — only current.
- **logic.md** — Domain rules and behavioral invariants. "An order can transition from PENDING to PAID only after payment confirmation." "Users are soft-deleted, never hard-deleted." Things the *code does* that an agent must respect.
- **flows.md** — Sequence-style descriptions of the top flows: trigger → steps → end state. Reference the code paths (file:function). Cover the 5–15 flows that matter most.
- **conventions.md** — How the team writes code: naming, error handling, logging, testing patterns, common idioms, what frameworks/libraries are preferred for what.
- **important.md** — The "if you change this without thinking, things break" list. Hot paths, performance-critical code, invariants enforced in non-obvious places, security-critical assumptions.
- **weak-points.md** — Where the code is fragile, under-tested, or carries debt. What to be careful about. Known anti-patterns that haven't been cleaned up yet.
- **changelog.md** — One-line entries: `[YYYY-MM-DD PR#NNN] Short description`. Deprecations and removals go here. This is the *only* place historical info lives.
- **gaps.md** — Open questions the skill couldn't answer. Format: `[YYYY-MM-DD PR#NNN] Question. Tried: <brief list of what was checked>.` Resolved gaps are removed (with optional one-line note in `changelog.md`).
- **.state/cursor.json** — `{ "last_processed_pr": <number>, "last_run": <ISO8601>, "first_run_completed": <bool>, "stats": {...} }`.

---

## Bootstrap workflow (first run)

Triggered when `.app-knowledge/INDEX.md` does not exist.

1. **Verify environment.**
   - Confirm we're in a git repo: `git rev-parse --show-toplevel`.
   - Confirm `gh` is installed and authenticated: `gh auth status`. If not, stop and tell the user to run `gh auth login`.
   - Identify the repo: `gh repo view --json nameWithOwner -q .nameWithOwner`.

2. **Discover the stack and layout.** This step is what makes the skill stack-agnostic — never assume, always inspect.
   - List top-level files and directories. Don't assume `src/`, `app/`, `lib/`, or any specific layout.
   - Look for any of these manifest/build files at the root (and one level down for monorepos): `package.json`, `pyproject.toml`, `requirements.txt`, `Pipfile`, `setup.py`, `*.csproj`, `*.sln`, `Cargo.toml`, `go.mod`, `pom.xml`, `build.gradle*`, `Gemfile`, `composer.json`, `mix.exs`, `Package.swift`, `pubspec.yaml`, `CMakeLists.txt`, `Makefile`, `Dockerfile`, `docker-compose.yml`. Whatever exists tells you the language, framework, and build system.
   - Read `README.md` (and any `docs/` folder index) if present — these often describe the app at a high level.
   - Read CI config (`.github/workflows/`, `.circleci/`, etc.) to learn how the app is built, tested, and deployed.
   - **Monorepo handling**: if multiple unrelated apps exist as siblings (e.g., `apps/api/package.json` and `apps/web/package.json`, or `services/foo/go.mod` and `services/bar/go.mod`), treat the repository as a single knowledge base at the root for this skill's purposes — and clearly enumerate the per-app boundaries inside `architecture.md`.
   - Record the discovered stack, layout, and boundaries as the first content written to `architecture.md`.

3. **Create the folder and stub files.**
   - `mkdir -p .app-knowledge/.state`
   - Create empty stub files for each of the main files listed above so structure is in place from the start.

4. **Fetch full merged-PR history.** Page through with `gh`:
   ```bash
   gh pr list --state merged --limit 1000 --json number,title,mergedAt,author,labels,body \
     --search "is:merged sort:created-asc"
   ```
   For repos with > 1000 PRs, paginate with `--search "merged:<DATE_RANGE>"` or by ascending PR number ranges. **Checkpoint after every 50 PRs** by writing `cursor.json` with the highest PR number processed so far — if the run is interrupted, the next run resumes from there.

5. **Process PRs in chronological order.** For each PR:
   - Skim title + body + labels + changed files. If the PR is trivial (dependency bump, typo fix, formatting only), record a one-line entry in `changelog.md` and move on.
   - For substantive PRs, fetch the diff: `gh pr view <num> --json files,body,title,labels` and `gh pr diff <num>`.
   - Decide which knowledge file(s) the PR informs: architecture, logic, flows, conventions, important, weak-points.
   - Update those files **in place**. Apply the anti-historical rule: if the PR deprecates something already in the file, delete the deprecated content and note it in `changelog.md`.
   - If the PR raises a question that can't be resolved from the diff alone, read the surrounding code in its current state. Only if still unclear, append to `gaps.md`.

6. **Final consolidation pass.** After all PRs are processed:
   - Walk each main file. Merge duplicate/overlapping bullets, tighten prose, ensure each file is under its size cap.
   - Write `INDEX.md` last, summarizing what the app is and pointing to each file.
   - Mark `first_run_completed: true` in `cursor.json`.

7. **Report to the user.** Print a summary: PRs processed, files created, gaps logged, total knowledge size.

---

## Incremental workflow (subsequent runs)

Triggered when `.app-knowledge/INDEX.md` exists.

1. **Load state.** Read `.state/cursor.json` → `last_processed_pr`.

2. **Fetch new merged PRs.**
   ```bash
   gh pr list --state merged --limit 200 --json number,title,mergedAt,author,labels,body \
     --search "is:merged sort:created-asc"
   ```
   Filter to PRs with `number > last_processed_pr`. (Sorting by created-asc ensures chronological processing.)

3. **For each new PR**, follow the same per-PR logic as bootstrap step 5 (skim → classify → update affected files in place → log gap if needed).

4. **Sweep `gaps.md`.** For each open gap, check if any of the new PRs resolves it. If so, remove it from `gaps.md` and update the relevant main file (and add a one-line note to `changelog.md` if the resolution is significant).

5. **Size check.** For each main file, if it's within 1 KB of its cap, run a consolidation pass: merge similar bullets, tighten prose, drop low-value detail. Don't expand the cap.

6. **Update `INDEX.md`** if the structure or app summary changed materially.

7. **Update `cursor.json`** with the new highest PR number, current timestamp, and updated stats.

8. **Report to the user**: PRs processed this run, files updated, gaps opened/closed.

---

## The anti-historical discipline

This is the single most important rule. The main files are *not* a history book. They describe the app **as it exists today**.

- ❌ "Originally we used Redis for caching, then we switched to Memcached, and now we use both."
- ✅ "Caching uses Memcached for session data and Redis for rate limiting." (Switching from "Redis only" → "Memcached + Redis" goes in `changelog.md` as a one-line note.)

When a PR removes or replaces something:
1. Find the now-stale content in the relevant main file.
2. Delete it (or rewrite it to reflect the new state).
3. Add a single line to `changelog.md`: `[YYYY-MM-DD PR#NNN] Replaced X with Y in <area>.`

`changelog.md` is the **only** place where past states are visible.

---

## The no-guessing discipline

When the skill encounters something it doesn't fully understand:

1. **First, read more code.** Look at callers, callees, tests, configuration files, READMEs in the relevant directory.
2. **Second, check related PRs.** A PR's body or linked PRs often explain the *why*.
3. **Third, only if still unclear, log a gap.** Append a line to `gaps.md`:
   ```
   [2026-05-03 PR#1234] Unclear: why does the OrderService swallow ValidationError silently in submit()? Tried: read OrderService.submit, OrderService tests, PR description, linked issue #890. No explanation found.
   ```
4. **Never invent a plausible explanation.** A wrong "this is probably for X" is worse than an honest gap, because downstream agents will trust it.

Gaps are first-class citizens. The user can review `gaps.md` and answer them directly; the skill picks up resolutions on the next run by checking whether new PRs touch the gap area.

---

## Size management

Each file has a target. Approaching the soft cap means **consolidate**, not extend. Exceeding the hard cap means **split into a directory**, not grow the file.

### Caps

| File | Soft cap (consolidate) | Hard cap (split) |
|---|---|---|
| INDEX.md | 2 KB | 3 KB *(rare; INDEX rarely splits — tighten instead)* |
| architecture.md | 5 KB | 8 KB → split |
| logic.md | 5 KB | 8 KB → split |
| flows.md | 6 KB | 10 KB → split |
| conventions.md | 4 KB | 6 KB → split |
| important.md | 4 KB | 6 KB → split |
| weak-points.md | 4 KB | 6 KB → split |
| changelog.md | 10 KB | 20 KB → rotate oldest 50% to `.state/changelog-archive.md` |
| gaps.md | 3 KB | 5 KB → if persistently exceeded, the user is being asked to triage |

These caps are deliberately small. The point is **not** that small apps need only 5 KB of architecture documentation — it's that any single file should stay focused and readable. Big apps don't bloat individual files; they grow *more* files via the splitting mechanism below.

### Consolidation tactics (at the soft cap)

In order of preference:
1. Merge bullets that say the same thing in different words.
2. Replace prose paragraphs with bullet lists.
3. Drop examples once a pattern is well-established (one canonical example is enough).
4. Tighten descriptions; remove low-value adjectives and hedging.

### Splitting on overflow (at the hard cap)

When a file would exceed its hard cap even after consolidation, **convert it to a same-named directory** with the original file becoming a routing index inside it.

Before:
```
.app-knowledge/
└── flows.md     ← exceeds 10 KB after consolidation
```

After:
```
.app-knowledge/
└── flows/
    ├── flows.md           ← now a routing index: "for X, read Y"
    ├── auth.md            ← signup, login, session, password reset
    ├── payment.md         ← checkout, refund, subscription
    ├── sync.md            ← background data sync flows
    └── notifications.md   ← email/push/in-app delivery
```

Splitting rules:
- Each sub-file targets the same soft/hard cap as its parent. If a sub-file later exceeds its own hard cap, it splits the same way (rarely needed in practice).
- The routing index (`flows/flows.md`) stays under 2 KB and contains only: a one-line description of the area, then a routing list pointing to each sub-file with one-sentence summaries.
- `INDEX.md` is updated to reference the directory rather than the single file.
- Sub-file names use the natural sub-areas of the topic — domains for `logic/`, services or layers for `architecture/`, flow categories for `flows/`, etc. The skill picks names by inspecting the actual content being split, not by following a template.

This mechanism means the structure scales naturally:

- **Tiny app** (CLI, small service): everything fits in single files. Flat structure.
- **Medium app**: maybe `flows/` and `architecture/` split into directories. Mostly flat.
- **Large app**: most main files have split into directories with 3–8 sub-files each.
- **XL app / monorepo**: multiple levels of splitting; sub-files themselves may have split. The routing indexes guide agents to exactly the leaf they need.

At every scale, individual files stay small enough to be loaded and read efficiently by an AI agent.

---

## Read mode (consumption by other agents)

When an AI agent needs context about the app and `.app-knowledge/INDEX.md` exists, it should:

1. Read `INDEX.md` to get the app summary and routing table.
2. Read the specific files relevant to the task (e.g., for a refactor of payment code → read `flows.md` or `flows/payment.md`, `important.md`, `weak-points.md`). If a topic has split into a directory (e.g., `flows/` instead of `flows.md`), the routing index inside the directory points to the right sub-file.
3. Check `gaps.md` for known unknowns in the area being touched.
4. **Not** modify `.app-knowledge/` — only the build/update modes do that.

---

## Failure modes and edge cases

- **Not a git repo** → stop with: "App-knowledge-builder requires a git repository at the working directory."
- **`gh` not installed or not authenticated** → stop with the exact command to fix it.
- **Unfamiliar language or framework** → ingest by reading source files as text. The skill never runs build steps, never installs dependencies, never executes code. If a file's purpose is unclear from its contents, follow the no-guessing discipline — log to `gaps.md` rather than invent.
- **Monorepo with multiple distinct apps at the root** → v1 maintains a single `.app-knowledge/` at the repo root. `architecture.md` must clearly delineate the per-app boundaries (their paths, languages, and how they communicate). Per-app knowledge bases are out of scope for v1.
- **Repo with thousands of PRs** → bootstrap can take a long time. Always checkpoint `cursor.json` after every 50 PRs so partial runs are recoverable. If the user interrupts, the next run resumes.
- **Squash vs merge commits** → process by PR number, not by commit, so it doesn't matter how the merge strategy is configured.
- **External / fork PRs** → process them like any merged PR.
- **Bot PRs (Dependabot, Renovate, etc.)** → typically a single line in `changelog.md`. Don't update main files unless the upgrade is major (e.g., framework version bump).
- **Files used to exist but were renamed** → the current state matters. The rename itself is a `changelog.md` line.
- **PR with no description** → only the diff and changed files are available; if that's not enough, log a gap.

---

## Reporting format

After each run, print to the user:

```
✅ app-knowledge-builder run complete

Mode: <bootstrap|incremental>
PRs processed: <N> (range: #<first>–#<last>)
Files updated: architecture.md, flows.md, conventions.md
Gaps opened: 2 | Gaps closed: 1 | Open gaps: 7
Total knowledge size: <X> KB across <Y> files
Cursor: PR #<N> @ <ISO timestamp>

→ See .app-knowledge/INDEX.md for the entry point.
→ See .app-knowledge/gaps.md for open questions.
```
