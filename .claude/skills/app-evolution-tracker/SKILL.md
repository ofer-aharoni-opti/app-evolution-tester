---
name: app-evolution-tracker
description: Automatically tracks application evolution and architectural insights on PR merge to dev branch. Maintains living documentation under .app-knowledge/evolution/ that focuses on current application state, how it evolved, and key insights learned. Analyzes code diffs to extract architectural significance, detects new components, API changes, database modifications, and technology stack evolution. Provides intelligent filtering (only documents MEDIUM/HIGH significance changes). Creates current-state-focused documentation rather than historical logs. Use when you want meaningful technical evolution tracking across any repository without manual maintenance.
---

# App Evolution Tracker - Intelligent Analysis

I analyze merged PRs to maintain living documentation about your application's architectural evolution. I focus on current system state and meaningful technical insights, not historical logs or generic PR metadata.

## My Analysis Intelligence

When you provide me with a PR number, I:

### 1. **Deep Diff Analysis**
```python
# I parse code changes to extract:
- New files created (by type and purpose)
- Classes, interfaces, services added  
- API endpoints and routes introduced
- Database schemas, migrations, entities
- Configuration changes and dependencies
- Test coverage additions
- Infrastructure modifications

# I detect patterns like:
- "Added UserController with 5 CRUD endpoints"
- "Introduced Entity Framework migration for Users table"  
- "Added circuit breaker pattern to payment service"
- "Refactored authentication to JWT-based approach"
```

### 2. **Architectural Significance Scoring**
```python
def calculate_significance(changes):
    score = 0
    factors = []
    
    # High impact (5 points each)
    if changes.new_services: 
        score += 5
        factors.append(f"New service: {changes.new_services}")
    
    if changes.database_migrations:
        score += 4  
        factors.append("Database schema changes")
        
    if changes.breaking_api_changes:
        score += 4
        factors.append("Breaking API changes detected")
    
    # Medium impact (2-3 points each)
    if changes.new_endpoints:
        score += 2
        factors.append(f"{len(changes.new_endpoints)} new API endpoints")
        
    if changes.major_refactoring:
        score += 3
        factors.append("Major code restructuring")
    
    # Classification
    if score >= 15: return "HIGH", factors
    elif score >= 8: return "MEDIUM", factors  
    elif score >= 3: return "LOW", factors
    else: return "MINIMAL", factors
```

**Only MEDIUM and HIGH significance changes get documented.** This prevents noise from routine maintenance, bug fixes, and minor updates.

### 3. **Technology Stack Detection**
I automatically identify your current tech stack from file analysis and create documentation like:

```markdown
## Current Technology Stack
- .NET 8 / C# (Backend API)
- Entity Framework Core (Data Layer)  
- SQL Server (Primary Database)
- React 18 / TypeScript (Frontend)

## Recent Architectural Changes
### New Components Added (PR #47)
- ZubiController: Full CRUD operations (/api/zubi/*)
- ZubiService: Business logic with validation rules
- Zubi entity: EF model with audit fields and relationships

## Current System Structure
### API Layer (18 controllers total)
- Authentication: JWT with refresh tokens
- User Management: Profile, preferences, roles
- Product Catalog: Search, filtering, recommendations  
- Order Processing: Cart, checkout, payment
- **New: Zubi Management:** CRUD operations with business rules
```

### 4. **Smart Filtering**
Instead of logging every PR like this:
```markdown
❌ PR #9 - Add Zubi CRUD - Status: ✅ completed
```

I create meaningful analysis like this:
```markdown
✅ ## 2026-04-30 - Zubi Management System [MEDIUM]
**Significance Score:** 12/20
**Impact:** Complete domain with CRUD operations and business rules
**Code Analysis:**
- Files changed: 8 (6 new, 2 modified)
- New components: ZubiController, ZubiService, Zubi entity
- API changes: 5 new endpoints (/api/zubi/*)
- DB changes: 1 migration (Zubi table with audit fields)
**Current State:** System now supports 4 business domains
```

## Documentation I Create/Update

### `architecture-current.md`
**Focus:** What the system looks like NOW after this change
- Current technology stack and versions
- Active services and their responsibilities  
- API surface and integration patterns
- Data model and relationships

### `major-changes.md`  
**Focus:** Architecturally significant evolution events only
- HIGH/MEDIUM impact changes with detailed analysis
- Code metrics and business rationale
- Current state after the change

### `technical-insights.md` (when patterns emerge)
**Focus:** Learnings that inform future development
- Recurring architectural decisions
- Technology adoption patterns
- Team coding conventions

## My Quality Filters

**I DOCUMENT when changes:**
- ✅ Add new architectural components (services, controllers, entities)
- ✅ Modify API contracts or introduce new endpoints
- ✅ Change database schemas or add migrations
- ✅ Introduce new technology or frameworks
- ✅ Represent significant refactoring

**I SKIP when changes:**
- ❌ Fix typos, update comments, or adjust formatting
- ❌ Bump dependency versions without behavior changes
- ❌ Add simple bug fixes without architectural impact
- ❌ Make routine maintenance changes

## How to Use Me

**GitHub Actions Integration:**
```yaml
- name: Update Evolution Knowledge
  run: python scripts/run-evolution-skill.py ${{ github.event.number }}
  env:
    ANTHROPIC_API_KEY: ${{ secrets.ANTHROPIC_API_KEY }}
```

**Manual Analysis:**
```bash
claude --skill app-evolution-tracker "analyze PR 123"
claude --skill app-evolution-tracker "update evolution knowledge from recent merge"
```

**Direct Invocation:**
Just tell me: "analyze PR [number]" or "update app knowledge from PR [number]" and I'll:
1. Fetch the PR diff and metadata
2. Analyze architectural significance 
3. Create/update evolution documentation
4. Focus on current state and meaningful insights

## Repository Compatibility

I work across technology stacks:
- **Backend:** .NET, Node.js, Python, Java, Go, PHP
- **Frontend:** React, Vue, Angular, vanilla JS/TS  
- **Databases:** SQL Server, PostgreSQL, MySQL, MongoDB
- **Infrastructure:** Docker, Kubernetes, cloud configurations

My analysis adapts to your technology choices while maintaining consistent architectural insight quality across all your repositories.

I'm designed to be your team's architectural memory - capturing not just what changed, but why it matters for your system's evolution.
