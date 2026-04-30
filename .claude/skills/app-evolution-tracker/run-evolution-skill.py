#!/usr/bin/env python3
"""
Example skill execution script for app-evolution-tracker.
This script calls Claude API with the skill loaded to analyze PR changes.

Usage from GitHub Actions workflow:
python scripts/run-evolution-skill.py ${{ github.event.number }}

Usage manual:
python scripts/run-evolution-skill.py 123
"""

import os
import sys
import subprocess
import anthropic
import json
from pathlib import Path

def get_pr_info(pr_number):
    """Fetch PR information using GitHub CLI"""
    try:
        # Get PR details
        pr_result = subprocess.run([
            'gh', 'pr', 'view', str(pr_number), 
            '--json', 'title,body,author,mergedAt,files'
        ], capture_output=True, text=True, check=True)
        
        pr_data = json.loads(pr_result.stdout)
        
        # Get PR diff
        diff_result = subprocess.run([
            'gh', 'pr', 'diff', str(pr_number)
        ], capture_output=True, text=True, check=True)
        
        return {
            'number': pr_number,
            'title': pr_data.get('title', ''),
            'body': pr_data.get('body', ''),
            'author': pr_data.get('author', {}).get('login', ''),
            'diff': diff_result.stdout,
            'files': [f['path'] for f in pr_data.get('files', [])]
        }
    except subprocess.CalledProcessError as e:
        print(f"❌ Error fetching PR data: {e}")
        return None

def load_skill():
    """Load the app-evolution-tracker skill content"""
    
    # Look for skill in common locations
    skill_locations = [
        'app-evolution-tracker/SKILL.md',  # If skill is in repo
        '.claude-skills/app-evolution-tracker/SKILL.md',  # Standard location
        '~/.claude/skills/app-evolution-tracker/SKILL.md',  # Global install
        Path(__file__).parent.parent / 'skills' / 'app-evolution-tracker' / 'SKILL.md'  # Relative
    ]
    
    for location in skill_locations:
        skill_path = Path(location).expanduser()
        if skill_path.exists():
            print(f"📝 Loading skill from: {skill_path}")
            return skill_path.read_text(encoding='utf-8')
    
    print("⚠️ Skill file not found. Using minimal built-in skill.")
    return """
You are an app evolution tracker. Analyze the provided PR to create meaningful 
documentation about architectural changes. Focus on:

1. Current system state after this change
2. Architectural significance of the changes  
3. Technology stack and component updates
4. Only log significant changes (not routine maintenance)

Create/update files under .app-knowledge/evolution/:
- architecture-current.md (current system state)
- major-changes.md (significant evolution events only)
"""

def run_evolution_analysis(pr_number):
    """Run the evolution analysis using Claude API"""
    
    # Check for API key
    api_key = os.environ.get('ANTHROPIC_API_KEY')
    if not api_key:
        print("❌ ANTHROPIC_API_KEY not found in environment")
        print("Add your Claude API key to GitHub Secrets or environment")
        return False
    
    # Get PR information
    print(f"📥 Fetching PR #{pr_number} information...")
    pr_info = get_pr_info(pr_number)
    if not pr_info:
        return False
    
    print(f"📋 PR: {pr_info['title']} by @{pr_info['author']}")
    print(f"📄 Files changed: {len(pr_info['files'])}")
    
    # Load skill
    print("🧠 Loading app-evolution-tracker skill...")
    skill_content = load_skill()
    
    # Prepare message for Claude
    message = f"""
Analyze this merged PR for architectural evolution insights:

**PR #{pr_info['number']}:** {pr_info['title']}
**Author:** @{pr_info['author']}
**Files Changed:** {len(pr_info['files'])}

**File List:**
{chr(10).join(f"- {f}" for f in pr_info['files'][:20])}
{"... and more" if len(pr_info['files']) > 20 else ""}

**PR Description:**
{pr_info['body'][:500] if pr_info['body'] else 'No description provided'}

**Code Diff:**
```diff
{pr_info['diff'][:8000]}  # Limit diff size for API
{"... [diff truncated for length]" if len(pr_info['diff']) > 8000 else ""}
```

Please analyze this PR and update the evolution documentation under .app-knowledge/evolution/ 
Create the directory and files if they don't exist. Focus on architectural significance 
and current system state.
"""
    
    # Call Claude API
    print("🚀 Running evolution analysis...")
    try:
        client = anthropic.Anthropic(api_key=api_key)
        
        response = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=4000,
            system=skill_content,
            messages=[{"role": "user", "content": message}]
        )
        
        if response.content:
            result = response.content[0].text
            print("✅ Evolution analysis completed")
            print(f"📄 Response length: {len(result)} characters")
            
            # Show preview of response
            lines = result.split('\n')
            preview_lines = lines[:10]
            print("\n📋 Analysis Preview:")
            for line in preview_lines:
                print(f"  {line}")
            if len(lines) > 10:
                print(f"  ... ({len(lines) - 10} more lines)")
            
            return True
        else:
            print("⚠️ No response content received")
            return False
            
    except Exception as e:
        print(f"❌ Error calling Claude API: {e}")
        return False

def main():
    if len(sys.argv) != 2:
        print("Usage: python run-evolution-skill.py <pr_number>")
        sys.exit(1)
    
    try:
        pr_number = int(sys.argv[1])
    except ValueError:
        print("❌ PR number must be an integer")
        sys.exit(1)
    
    print(f"🎯 Starting app evolution analysis for PR #{pr_number}")
    
    success = run_evolution_analysis(pr_number)
    
    if success:
        print("🎉 Evolution analysis completed successfully!")
        print("📁 Check .app-knowledge/evolution/ for updated documentation")
    else:
        print("💥 Evolution analysis failed")
        sys.exit(1)

if __name__ == "__main__":
    main()
