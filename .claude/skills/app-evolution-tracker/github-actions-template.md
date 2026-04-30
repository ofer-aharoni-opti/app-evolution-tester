# GitHub Actions Workflow Template for App Evolution Tracker

Drop this file into your repository at `.github/workflows/app-evolution-tracker.yml` to automatically update app knowledge on every PR merge to dev branch.

```yaml
name: App Evolution Tracker
on:
  pull_request:
    types: [closed]
    branches: [dev]  # Change to your main development branch

jobs:
  update-evolution:
    if: github.event.pull_request.merged == true
    runs-on: ubuntu-latest
    
    permissions:
      contents: write
      pull-requests: read
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
        token: ${{ secrets.GITHUB_TOKEN }}
        
    - name: Setup Git
      run: |
        git config --local user.email "action@github.com"
        git config --local user.name "App Evolution Tracker"
        
    - name: Install GitHub CLI
      run: |
        curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
        sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
        echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null
        sudo apt update
        sudo apt install gh
        
    - name: Update App Evolution Knowledge
      run: |
        # Your skill execution command here - adapt based on how you run Claude skills
        # Example options:
        
        # Option 1: If using Claude API directly
        # python scripts/run-evolution-tracker.py ${{ github.event.number }}
        
        # Option 2: If using Claude desktop/CLI
        # claude --skill app-evolution-tracker "update app knowledge from PR ${{ github.event.number }}"
        
        # Option 3: Custom script that calls the skill
        echo "Updating evolution knowledge for PR #${{ github.event.number }}"
        echo "PR Title: ${{ github.event.pull_request.title }}"
        echo "PR Author: ${{ github.event.pull_request.user.login }}"
        
        # Placeholder - replace with your actual skill execution
        echo "Replace this section with your skill execution command"
        
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        PR_NUMBER: ${{ github.event.number }}
        PR_TITLE: ${{ github.event.pull_request.title }}
        PR_AUTHOR: ${{ github.event.pull_request.user.login }}
        
    - name: Commit and push changes
      run: |
        if [[ -n $(git status --porcelain .app-knowledge/evolution/) ]]; then
          git add .app-knowledge/evolution/
          git commit -m "docs: update app evolution knowledge from PR #${{ github.event.number }}

          Automated update from app-evolution-tracker skill.
          PR: ${{ github.event.pull_request.title }} by @${{ github.event.pull_request.user.login }}"
          git push
        else
          echo "No evolution knowledge changes to commit"
        fi

    - name: Comment on PR (optional)
      if: success()
      run: |
        gh pr comment ${{ github.event.number }} --body "🧠 **App evolution knowledge updated**

        The application evolution documentation has been automatically updated to reflect changes from this PR.
        
        View updated knowledge: [.app-knowledge/evolution/](../tree/dev/.app-knowledge/evolution)"
      env:
        GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## Configuration Notes

1. **Branch name**: Change `branches: [dev]` to your main development branch
2. **Skill execution**: Replace the placeholder in "Update App Evolution Knowledge" step with your actual method of running Claude skills
3. **Permissions**: The workflow needs `contents: write` to commit changes and `pull-requests: read` to access PR data
4. **Optional PR comment**: Remove the last step if you don't want the bot to comment on merged PRs

## Alternative Execution Methods

### Option A: Direct API calls
If you have Claude API access, create a script that calls the API with the skill:

```python
# scripts/run-evolution-tracker.py
import os
import anthropic

client = anthropic.Anthropic(api_key=os.environ["ANTHROPIC_API_KEY"])

pr_number = os.environ["PR_NUMBER"]
message = f"update app knowledge from PR {pr_number}"

response = client.messages.create(
    model="claude-sonnet-4-20250514",
    max_tokens=4000,
    messages=[{"role": "user", "content": message}],
    # Include the skill content in system prompt
)
```

### Option B: Self-hosted runner
If using Claude desktop on a self-hosted runner:

```yaml
runs-on: self-hosted  # Your runner with Claude access
steps:
- name: Update Evolution Knowledge
  run: claude --skill app-evolution-tracker "update app knowledge from PR ${{ github.event.number }}"
```

### Option C: Webhook to external service
Send webhook to your service that has Claude access:

```yaml
- name: Trigger Evolution Update
  run: |
    curl -X POST "https://your-service.com/webhook/evolution-tracker" \
      -H "Content-Type: application/json" \
      -d '{
        "pr_number": "${{ github.event.number }}",
        "pr_title": "${{ github.event.pull_request.title }}",
        "repository": "${{ github.repository }}"
      }'
```
