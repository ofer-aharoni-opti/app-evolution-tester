# App Evolution Tracker - Reusable Skill Package

**You're absolutely right!** The analysis logic should be in the **skill**, not scattered across workflow files. This package provides a truly reusable evolution tracker that works across any repository.

## 🎯 **Proper Separation of Concerns**

```
app-evolution-tracker/
├── SKILL.md                    # 🧠 All the intelligence (REUSABLE)
├── workflows/
│   └── github-actions.yml     # ⚙️ Simple skill invoker (GENERIC)
├── scripts/
│   └── run-evolution-skill.py # 🔧 API integration example
└── examples/
    └── integration-examples.md # 📖 Different ways to use the skill
```

## 🚀 **How It Works**

**1. The Skill (SKILL.md)** contains all the intelligent analysis:
- PR diff parsing and architectural pattern extraction
- Significance scoring (HIGH/MEDIUM/LOW/MINIMAL)
- Current system state documentation
- Technology stack detection
- Evolution insights generation

**2. The Workflow (github-actions.yml)** is just a thin wrapper:
- Triggers on PR merge
- Calls the skill with PR context  
- Commits the results
- **Same workflow works in any repo**

**3. The Script (run-evolution-skill.py)** handles skill execution:
- Fetches PR data via GitHub CLI
- Calls Claude API with skill loaded
- **Completely reusable across repositories**

## 📦 **Installation Per Repository**

### Option A: Global Skill + Local Workflow
```bash
# Install skill globally (once)
claude install app-evolution-tracker

# Add workflow to each repo
cp workflows/github-actions.yml .github/workflows/app-evolution-tracker.yml
```

### Option B: Include Skill In Repository
```bash
# Add skill to repo
mkdir -p .claude-skills/app-evolution-tracker
cp SKILL.md .claude-skills/app-evolution-tracker/

# Add workflow and script
cp workflows/github-actions.yml .github/workflows/
cp scripts/run-evolution-skill.py scripts/
```

### Option C: API Integration
```bash
# Just add the workflow and script
cp workflows/github-actions.yml .github/workflows/
cp scripts/run-evolution-skill.py scripts/

# Add GitHub Secret: ANTHROPIC_API_KEY
# The script will load the skill and call Claude API
```

## 🔧 **Configuration**

**Per Repository:**
1. **Change branch name** in workflow (dev → main, develop, etc.)
2. **Add API key** to GitHub Secrets if using API integration
3. **Customize skill focus** if needed (modify SKILL.md)

**That's it!** The skill automatically detects:
- Technology stack from file extensions
- Architectural patterns from code structure  
- Significance based on change analysis
- Current system state from codebase

## 🎯 **Benefits of This Approach**

### ✅ **Truly Reusable**
- **Same skill** works across .NET, Node.js, Python, Java projects
- **Generic workflow** just invokes skill with PR data
- **No copy-paste** of analysis logic between repos

### ✅ **Maintainable**
- **Bug fix in skill** → affects all repositories using it
- **Enhancement to analysis** → all teams benefit
- **Single source of truth** for evolution logic

### ✅ **Extensible**
- **Add new language detection** → edit skill only
- **Improve significance scoring** → edit skill only
- **Custom repository needs** → modify skill per repo if needed

## 🚀 **Example Output**

**Instead of generic logs:**
```markdown
## 2026-04-30 - PR #9
**Title:** Add Zubi CRUD
**Status:** ✅ completed
```

**You get meaningful analysis:**
```markdown
## 2026-04-30 - User Management System [HIGH]
**PR:** #47 by @developer  
**Significance Score:** 16/20
**Impact:** Complete CRUD system with authentication
**Code Analysis:**
- Files changed: 12 (8 new, 4 modified)
- New components: ZubiController, ZubiService, Zubi model
- API changes: 5 new endpoints (/users/create, /users/list, etc.)
- DB changes: 2 migrations (Users, UserRoles tables)
**Current State:** System now supports user authentication and role management
```

## 🎪 **Integration Examples**

See `examples/integration-examples.md` for:
- GitHub Actions with API key
- Self-hosted runner with Claude CLI
- Webhook integration
- Custom skill runners
- Multi-repository deployment

## 🏆 **This Solves Your Original Problem**

**Your need:** Reusable skill for multiple repositories
**Previous issue:** Logic was in workflow files (not portable)
**This solution:** Smart skill + generic workflow = true reusability

Now you can use the same evolution intelligence across all your repositories with minimal setup per repo!
