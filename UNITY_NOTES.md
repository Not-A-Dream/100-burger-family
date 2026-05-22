
```md
# Unity Notes and Pitfalls

## Inspector Serialization Priority

```text
Scene serialized values > code default values
```

Example:

```csharp
public float size = 7f;
```

This does NOT update existing scene instances.

To force updates:
- assign values in Awake()
- update serialized values directly
- use Editor scripts

---

# Play Mode Changes Are Temporary

Runtime modifications disappear after Stop.

Permanent modifications require:
- Edit mode changes
- scene saving

---

# Scene Structure Matters More Than Code

Most Unity bugs come from:

- hierarchy structure
- prefab overrides
- serialized references
- runtime creation order
- missing Inspector links

Before modifying systems, determine:

```text
Who creates this object?
When is it created?
Is the serialized reference connected?
Is this prefab overridden?
```

---

# DontDestroyOnLoad Notes

Objects inside DontDestroyOnLoad:
- do not exist in Edit mode
- may return null
- are often runtime-created

Prefer lazy singleton patterns.

---

# Safe Prefab Modification Rules

Preferred:
- incremental prefab edits
- targeted hierarchy changes
- preserving existing references

Avoid:
- recreating prefabs unnecessarily
- breaking prefab connections
- renaming serialized fields
- rebuilding working hierarchies

---

# Unity AI Workflow Philosophy

```text
Aggressive gameplay iteration
+
Conservative structural modification
```

The AI should:
- modify gameplay quickly
- modify Unity structure carefully

---

# Common AI Failure Pattern

Bad:

```text
Small bug
→ full refactor
→ broken references
→ scene corruption
```

Preferred:

```text
Small bug
→ targeted fix
→ preserve existing systems
→ immediate Play test
```
```

---