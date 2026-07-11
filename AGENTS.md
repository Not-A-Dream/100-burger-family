# 100 Burger Family - AI Agent Rules

## Primary Goal

Ship a playable Unreal Engine 5 MVP quickly.

Prioritize:
- gameplay iteration
- stable playable maps
- Blueprint-first implementation
- template and asset reuse
- fast validation in editor

Avoid:
- overengineering
- unnecessary C++ systems
- speculative scalability work
- rewriting working Blueprints without need

---

## Core Rules

- Gameplay first
- Minimal diff
- Preserve working maps and assets
- Prefer Blueprint-safe changes
- Keep responses concise
- Modify only what is necessary

---

## Unreal Engine 5 Structural Rules

The AI MAY modify:
- maps
- Blueprint actors
- components
- collision
- transforms
- widgets
- input actions
- data assets

WHEN required for:
- gameplay implementation
- bug fixes
- usability improvements

However:
- avoid unnecessary folder rebuilds
- avoid breaking Blueprint references
- avoid renaming exposed variables unnecessarily
- avoid full gameplay rewrites
- do not add C++ unless Blueprint duplication or engine access requires it

---

## Project Direction

Use:
- Unreal Engine 5
- Top Down Template as the starting point
- Blueprint-first gameplay
- minimal C++ only for reusable base classes if needed
- Windows playable MVP first

Legacy Unity content lives under:

```text
legacy-unity/
```

Treat it as reference only.

---

## Response Format

Always use:

```text
[Cause]
[Fix]
[Impact]
[Validation]
```

Keep explanations short.

---

## Development Philosophy

```text
shipping gameplay
yes

unnecessary complexity
no
```

---

## Additional Context Files

Read these files when relevant:

- PROJECT_CONTEXT.md
- TODO.md
- docs/UE5_MIGRATION_PLAN.md

---

## Communication Rules

Use:
- Korean for conversation
- English for code structure and technical terminology

Keep:
- Unreal API names in English
- Blueprint names in English
- C++ identifiers in English
- Technical file names in English

Prefer:
- concise Korean explanations
- concise Korean debugging summaries
