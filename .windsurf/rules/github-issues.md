---
trigger: manual
---

#### 1. **Tooling Requirement**

Use the GitHub CLI tool (`gh`) to create issues.

---

#### 2. **Command Template (single-line, PowerShell-ready)**

```bash
gh issue create --title "<ISSUE_TITLE>" --body "<ISSUE_BODY>" --assignee "MarkusAlexanderReiter" --label "<LABELS_COMMA_SEPARATED>"
````

* No `\` line continuations—keep everything on one line so it works in PowerShell.
* Escape literal newlines inside `<ISSUE_BODY>` with `` `n ``.

---

#### 3. **Assistant Execution Rule**

When the assistant prepares an issue:

1. Fill `<ISSUE_TITLE>`, `<ISSUE_BODY>`, and `<LABELS_COMMA_SEPARATED>` as described below.
2. Invoke the `run_command` tool immediately to execute the command, setting
   `SafeToAutoRun: true` (creating a GitHub issue is non-destructive).

Example tool-call skeleton the assistant should use:

```json
{
  "name": "run_command",
  "arguments": {
    "CommandLine": "gh issue create --title \"...\" --body \"...\" --assignee \"MarkusAlexanderReiter\" --label \"...\"",
    "Cwd": "c:\\Users\\Markus\\Unity\\Cozy Cat Cafe",
    "SafeToAutoRun": true
  }
}
```

---

#### 4. **Available Labels**

Choose one or several (comma-separated) that best fit the issue:

* **art** – Create or update visual assets for the project
* **bug** – Something isn’t working as intended
* **documentation** – Improvements or additions to documentation or comments
* **enhancement** – New feature or improvement to existing functionality
* **question** – Needs clarification or further investigation
* **refactor** – Internal code cleanup or restructuring without changing behavior
* **research** – Requires exploration, planning, or feasibility analysis
* **task** – A concrete unit of work to implement or complete

---

#### 5. **Instructions for Populating Fields**

* `<ISSUE_TITLE>` – Concise, clear summary of the problem or task.
* `<ISSUE_BODY>` – Detailed explanation:

  * Context or problem description
  * Steps to reproduce (if applicable)
  * Expected outcome or goal
  * Suggestions or proposed solutions
  * Use `` `n `` for newline separation.
* `<LABELS_COMMA_SEPARATED>` – One or more labels from the list above.