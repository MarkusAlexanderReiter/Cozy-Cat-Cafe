---
trigger: manual
---

# ──────────────  generate_system_doc.rule  ──────────────
name: generate_system_doc
description: |
  Create a Markdown “Technical Overview” for the system described by the user, then write the file to Documentation/<System>.md. (Sub-Folders possible such as Documentation/Scripts)

# ── parameters passed in your call ─────────────────────
#   $system_name      – human-readable system title (“Inventory System”)
#   $filename         – optional; overrides default file name
#                       (default → "$system_name.md", spaces → _)
# -------------------------------------------------------

---

    ## Table of Contents
    1. [Purpose & High-Level Picture](#purpose)
    2. [Key Components](#components)
    3. [Runtime Data & Control Flow](#flow)
    4. [Extending the System](#extend)
    5. [Glossary](#glossary)
    6. [TODO / Future Work](#todo)

    ---

    ## 1  Purpose & High-Level Picture

    {{ Summarise the goal and main design traits of the system in 3–6 bullet points.
       Use the last LLM response as your only source. }}

    ---

    ## 2  Key Components

    {{ List major classes / scripts / modules you detect.
       For each, output a short table of public fields & a bullet list of responsibilities.
       Anchor each sub-section exactly like “### 2.n <ClassName>”. }}

    ---

    ## 3  Runtime Data & Control Flow

    {{ Describe the primary execution path at runtime as numbered steps.
       If helpful, embed an ASCII sequence diagram inside a fenced block. }}

    ---

    ## 4  Extending the System

    {{ Give practical guidance for adding new features, swapping components,
       or tuning performance – drawn from the code where possible. }}

    ---

    ## 5  Glossary

    {{ Table of 3-10 domain terms & their meaning, if any were present. }}

    ---

    ## 6  TODO / Future Work

    {{ Create a bullet list of “next steps” inferred from TODOs or obvious gaps. }}

    MD

# No further context-setting, triggers, or chatter needed.
# ─────────────────────────────────────────────────────────
