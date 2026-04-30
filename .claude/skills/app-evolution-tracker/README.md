# App Evolution Tracker Skill

Automatically maintains living documentation of your application's evolution, architecture, and insights. Designed to run via GitHub Actions on every PR merge to your development branch.

## What It Does

Creates and maintains files under `.app-knowledge/evolution/` that represent:

- **Current architecture state** (not historical artifacts)
- **Significant evolution events** that shaped the current system  
- **Technical insights and patterns** actively used
- **API and data model evolution** with current state focus
- **Deprecated approaches** to prevent reintroduction

## Key Philosophy

**Current state over historical artifacts.** This skill focuses on what your application looks like NOW and how it evolved to this state, treating old/changed code and deprecated flows as less important than current reality and insights.

## Quick Start

1. **Install the skill** in your Claude environment
2. **Copy the GitHub Actions workflow** from `github-actions-template.md` to `.github/workflows/app-evolution-tracker.yml`
3. **Configure the workflow** with your branch name and skill execution method
4. **Merge a PR to dev branch** - the skill will create initial documentation

## File Structure Created

```
.app-knowledge/evolution/
├── architecture-current.md      # What the system looks like NOW
├── major-changes.md            # Significant evolution events  
├── technical-insights.md       # Current patterns & lessons learned
├── api-evolution.md           # API changes & current state
├── data-evolution.md          # Database/data model evolution
└── deprecated.md              # What was removed and why
```

## Example Output

See the `examples/` directory for realistic examples of what the evolution documentation looks like in practice.

## Manual Usage

You can also trigger the skill manually:

```bash
claude --skill app-evolution-tracker "update app knowledge from PR 123"
claude --skill app-evolution-tracker "refresh app knowledge" 
claude --skill app-evolution-tracker "document this architectural change"
```

## Integration Options

### Option A: GitHub Actions (Recommended)
Use the provided workflow template to run automatically on PR merge.

### Option B: Webhook Integration
Set up a webhook that calls your Claude-enabled service when PRs are merged.

### Option C: Manual Runs
Run the skill manually after significant changes or during architecture reviews.

## What Makes This Different

### vs. Traditional Changelog
- **Living documentation** that updates current state
- **Focuses on insights**, not just "what changed"  
- **Architecture-focused**, not feature-focused

### vs. Git History
- **Explains WHY** changes were made
- **Connects changes** to current architecture
- **Captures insights** that aren't in commit messages

### vs. Architecture Decision Records (ADRs)
- **Automatically maintained** from PR activity
- **Current state focused** rather than decision-point focused
- **Evolution narrative** rather than isolated decisions

## Success Metrics

The evolution documentation succeeds when:
- ✅ New team members understand current architecture quickly
- ✅ Architectural decisions are clear and traceable  
- ✅ Patterns and lessons are actively referenced during development
- ✅ Documentation stays current without manual maintenance
- ✅ Teams avoid reintroducing deprecated patterns

## Customization

### Focus Areas
Modify the skill to emphasize what matters most for your team:
- **Data-heavy applications:** Emphasize `data-evolution.md`
- **API-first products:** Focus on `api-evolution.md` 
- **Infrastructure-focused:** Add `infrastructure-evolution.md`

### Change Classification
Adjust the change classification logic to match your architecture:
- Define what constitutes "architectural" vs "maintenance" changes
- Set thresholds for what gets documented vs ignored
- Customize the significance scoring for your domain

### Integration Points
- **Slack notifications:** Post evolution updates to team channels
- **Wiki integration:** Sync evolution docs to your team wiki
- **Dashboard integration:** Include evolution insights in team dashboards

## Contributing

This skill improves through usage. Consider contributing:
- **Classification improvements** for better change detection
- **Template enhancements** for your technology stack
- **Integration examples** for different deployment scenarios

## License

Open source - use and modify as needed for your team.
