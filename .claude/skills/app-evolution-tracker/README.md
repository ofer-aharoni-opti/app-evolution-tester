# App Evolution Tracker - Installation Guide

## 📁 **File Locations**

**Skill:** `.claude/skills/app-evolution-tracker/SKILL.md`  
**Data:** `.app-knowledge/evolution/` (in your repository)

## 🚀 **Installation Steps**

### 1. Install the Skill
```bash
# Create skill directory
mkdir -p .claude/skills/app-evolution-tracker

# Copy the SKILL.md file
cp app-evolution-tracker-SKILL.md .claude/skills/app-evolution-tracker/SKILL.md
```

### 2. Initialize Knowledge Folder in Repository
```bash  
# Create knowledge structure in your repo
mkdir -p .app-knowledge/evolution
```

### 3. Test the Skill
```bash
cd your-repository/
claude --skill app-evolution-tracker "analyze PR 123"
```

## 📋 **Usage Examples**

### When You Merge a PR:
```bash
claude --skill app-evolution-tracker "analyze PR 456 for architectural evolution"
claude --skill app-evolution-tracker "update evolution knowledge from merged PR 789"
```

### Query Evolution Patterns:
```bash
claude --skill app-evolution-tracker "what architectural patterns have emerged"  
claude --skill app-evolution-tracker "how has our technology stack evolved"
claude --skill app-evolution-tracker "what are the next logical architectural steps"
```

### Check System Evolution:
```bash
claude --skill app-evolution-tracker "show me our current architecture state"
claude --skill app-evolution-tracker "what domain growth patterns do you see"
```

## 🧠 **How It Gets Smarter**

**First use:** Creates initial knowledge structure
**PRs 1-10:** Learns your basic patterns
**PRs 11-25:** Recognizes team conventions  
**PRs 26+:** Provides predictive insights

## 📁 **Knowledge Structure Created**

The skill automatically creates and maintains:
```
.app-knowledge/evolution/
├── current-state/              # Current system documentation
├── evolution-patterns/         # Learned team patterns
├── significant-changes/        # Major evolution timeline
├── insights/                  # Generated intelligence  
└── memory/                    # Analysis state
```

## ✅ **Benefits**

- ✅ **Comprehensive evolution tracking** like project-coach
- ✅ **No user interaction required** - just analyze PRs
- ✅ **Gets smarter over time** - accumulates architectural intelligence
- ✅ **Contextual insights** - connects changes to evolution patterns
- ✅ **Team knowledge sharing** - committed knowledge in repository

## 🎯 **Perfect for Teams Who Want**

- Automatic architectural evolution documentation
- Understanding of how their system evolved over time
- Recognition of their architectural patterns and conventions
- Guidance for future architectural decisions
- Preservation of architectural knowledge and decisions

Your evolution documentation will grow from simple PR logs to comprehensive architectural intelligence that guides future development!
