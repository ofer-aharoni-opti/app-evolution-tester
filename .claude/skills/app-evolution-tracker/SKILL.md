---
name: app-evolution-tracker
description: Automatically tracks application evolution and architectural insights on PR merge to dev branch. Maintains living documentation under .app-knowledge/ that focuses on current application state, how it evolved, and key insights learned. Triggered by GitHub Actions on dev branch merges. Use when you want to automatically capture significant changes, architectural evolution, and team insights without manual maintenance. Also trigger manually with "update app knowledge", "track this change", "document this evolution", "capture app insights".
---

# App Evolution Tracker

Automatically maintains living documentation of application evolution, architecture, and insights. Designed to run via GitHub Actions on every PR merge to dev branch, creating and updating files under `.app-knowledge/evolution/` that represent the current state and evolution path of the application.

## Core Philosophy

**Current state over historical artifacts.** Focus on what the application looks like NOW and how it evolved to this state, not detailed historical records. Old/changed code, deprecated flows, and outdated architecture are treated as less important than current reality and evolution insights.

**Living documentation.** Files are continuously updated to reflect current reality rather than accumulating historical entries that become noise.

## Trigger Patterns

**Automatic (preferred):**
- GitHub Action on PR merge to `dev` branch
- Webhook on dev branch push

**Manual:**
- "update app knowledge with this PR"
- "track this architectural change"  
- "document this evolution"
- "capture insights from this change"
- "refresh app knowledge"

## File Structure

Creates/maintains these files under `.app-knowledge/evolution/`:

```
.app-knowledge/evolution/
├── architecture-current.md      # Current architecture state
├── major-changes.md            # Significant evolution events  
├── technical-insights.md       # Patterns, lessons, discoveries
├── api-evolution.md           # API changes and current state
├── data-evolution.md          # Database/data model evolution
└── deprecated.md              # What was removed/changed and why
```

## Prerequisites

1. **GitHub CLI authenticated:** `gh auth status`
2. **Repository context:** Must run from git repo root
3. **PR context:** Either PR number provided or auto-detected from merge commit

## Workflow

### Phase 1: Gather Context

**1. Extract PR information**
```bash
# Auto-detect from recent merge if no PR number given
RECENT_MERGE=$(git log --oneline --merges -1 --grep="Merge pull request")
PR_NUMBER=$(echo "$RECENT_MERGE" | grep -o '#[0-9]*' | sed 's/#//')

# Get PR details
gh pr view $PR_NUMBER --json title,body,author,files,commits
gh pr diff $PR_NUMBER
```

**2. Analyze change significance**
Classify the PR impact:
- **Architectural:** New services, major refactoring, design pattern changes
- **API:** New endpoints, breaking changes, contract evolution  
- **Data:** Schema changes, new entities, data flow modifications
- **Infrastructure:** Deployment, configuration, environment changes
- **Feature:** New functionality with architectural implications
- **Maintenance:** Bug fixes, updates (usually low-priority for evolution tracking)

### Phase 2: Update Living Documentation

**3. Create evolution directory if needed**
```bash
mkdir -p .app-knowledge/evolution
```

**4. Update architecture-current.md**
**Focus:** What the architecture looks like TODAY after this change.

```markdown
# Current Architecture - Updated {date}

## System Overview
{Current high-level architecture - updated based on PR changes}

## Core Services  
{List of current services with brief descriptions}

## Data Flow
{How data moves through the system currently}

## Recent Evolution
- **{date}:** {Brief description of architectural change from this PR}

## Key Decisions
{Current architectural decisions that shape the system}
```

**5. Update major-changes.md**
**Focus:** Significant evolution events that shaped the current application.

Only add entries for architecturally significant changes:
- New major features that changed system design
- Performance improvements that required architectural shifts  
- Breaking changes that affected system contracts
- Technology migrations or major refactors

```markdown  
# Major Evolution Events

## {date} - {Change Title}
**PR:** #{number} by @{author}
**Impact:** {Brief description of what changed architecturally}  
**Why:** {Reason for the change}
**Current State:** {How this affects current architecture}

---
{Previous entries, pruned if no longer relevant to current state}
```

**6. Update technical-insights.md**
**Focus:** Patterns, lessons, and discoveries that inform future development.

```markdown
# Technical Insights

## Current Patterns We Use
{Patterns that are actively used in current codebase}

## Lessons Learned  
{Insights from evolution - what worked, what didn't}

## Performance Discoveries
{Performance insights that guide current development}

## Recent Insights
- **{date}:** {Insight from this PR that affects future development}
```

**7. Update API/Data evolution files if applicable**
Similar approach - focus on current state with evolution context.

**8. Update deprecated.md**
**Focus:** What was removed and why, to prevent re-introduction of discarded approaches.

```markdown
# Deprecated & Removed

## Recently Deprecated
- **{date}:** Removed {feature/pattern} because {reason}

## Historical Deprecations  
{Keep only items that teams might be tempted to re-introduce}
```

### Phase 3: Intelligent Updates

**9. Content refresh strategy**
- **Keep:** Current state, recent insights (last 6 months), active patterns
- **Summarize:** Older detailed entries that are still relevant  
- **Remove:** Outdated information that no longer applies to current system
- **Archive:** Historical details that might be needed later but aren't day-to-day relevant

**10. Cross-reference with code**
Verify that documented architecture still matches codebase:
- Check that mentioned services/files still exist
- Validate that described patterns are still in use
- Update outdated references

**11. Commit changes**
```bash
git add .app-knowledge/evolution/
git commit -m "docs: update app evolution knowledge from PR #${PR_NUMBER}

Automated update from app-evolution-tracker skill.
Changes: ${CHANGE_SUMMARY}"

git push origin dev
```

## Change Classification Logic

**High Priority (always update evolution docs):**
- New services, major refactors, architectural pattern changes
- API contract changes, new major endpoints
- Database schema changes, new entities
- Performance changes that required architectural shifts
- Security changes that affected system design

**Medium Priority (update if significant):**
- New features with moderate architectural impact
- Technology updates that change development patterns
- Configuration changes that affect system behavior

**Low Priority (usually skip):**
- Bug fixes without architectural implications
- Dependency updates without behavior changes
- Documentation or testing changes only
- Minor UI/UX changes

## Content Quality Guidelines

**Good evolution entries:**
- Focus on current state after the change
- Explain WHY the change was made
- Connect to overall application evolution story
- Help future developers understand current architecture

**Avoid:**
- Detailed historical logs that become noise
- Technical implementation details that change frequently  
- Temporary workarounds or experiments
- Information that doesn't help understand current system

## GitHub Action Integration

```yaml
name: Update App Evolution Knowledge
on:
  pull_request:
    types: [closed]
    branches: [dev]

jobs:
  update-evolution:
    if: github.event.pull_request.merged == true
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
      with:
        fetch-depth: 0
    - name: Update Evolution Knowledge  
      run: |
        claude --skill app-evolution-tracker \
          "update app knowledge from PR ${{ github.event.number }}"
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## Success Metrics

**The evolution documentation succeeds when:**
- New team members can understand current architecture quickly
- Architectural decisions are clear and traceable
- Patterns and lessons are actively referenced during development
- Documentation stays current without manual maintenance

**It fails when:**
- Files become stale or contradictory
- Information is too detailed/historical to be useful
- Updates create noise rather than insights
- Evolution story becomes unclear

## Manual Refresh Mode

For occasional cleanup or bootstrap:

```bash
claude --skill app-evolution-tracker "full refresh of app knowledge"
```

This analyzes recent dev branch activity and rebuilds evolution documentation focused on current state.

---

This skill maintains **living memory** of application evolution - not a detailed changelog, but a current-state-focused narrative of how the application got to where it is and what insights emerged along the way.
