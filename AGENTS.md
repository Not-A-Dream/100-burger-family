
```md
# 100 Burger Family — AI Agent Rules

## Primary Goal

Ship a playable WebGL MVP quickly.

Prioritize:
- gameplay iteration
- stable Unity scenes
- debugging simplicity
- fast implementation

Avoid:
- overengineering
- unnecessary refactors
- architecture discussions
- speculative scalability work

---

# Core Rules

- Gameplay first
- Minimal diff
- Preserve working systems
- Prefer Inspector-safe changes
- Keep responses concise
- Modify only what is necessary

---

# Unity Structural Rules

The AI MAY modify:
- scenes
- hierarchy
- prefabs
- serialized references
- colliders
- transforms
- UI layout

WHEN required for:
- gameplay implementation
- bug fixes
- usability improvements

However:
- avoid unnecessary hierarchy rebuilds
- avoid breaking prefab connections
- avoid renaming serialized fields unnecessarily
- avoid full gameplay rewrites

---

# Response Format

Always use:

```text
[Cause]
[Fix]
[Impact]
[Validation]
```

Keep explanations short.

---

# Development Philosophy

```text
shipping gameplay
✅

unnecessary complexity
❌
```

---

# Additional Context Files

Read these files when relevant:

- PROJECT_CONTEXT.md
- UNITY_NOTES.md
- TODO.md
```

---

# Communication Rules

Use:
- Korean for conversation
- English for code structure and technical terminology

Keep:
- Unity API names in English
- C# identifiers in English
- Technical file names in English

Prefer:
- concise Korean explanations
- concise Korean debugging summaries