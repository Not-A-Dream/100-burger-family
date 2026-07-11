# Current TODO

## Highest Priority

### Migration

- Create UE5 project in `unreal-game/`
- Use UE5 Top Down Template
- Verify project opens in Unreal Editor
- Configure Git LFS before committing `.uasset` and `.umap`

---

## Gameplay

- Create `BP_InteractableBase`
- Create interaction key flow
- Create `BP_FarmStation`
- Create `BP_CookStation`
- Create `BP_ServeCounter`
- Complete harvest -> cook -> serve gameplay loop

---

## Player

- Tune Top Down movement
- Tune interaction range
- Tune collision capsule and station collision
- Improve camera readability

---

## UI

- Create `WBP_HUD`
- Show score
- Show timer
- Show current carried item
- Show interaction prompt

---

## Data

- Create `DA_Ingredient`
- Create `DA_Recipe`
- Keep first recipe simple

---

## Stability

- Verify Blueprint references
- Verify map loads
- Verify input actions
- Verify cooked/served state transitions

---

## Future Tasks

- Local co-op
- restaurant/kitchen art pass
- order queue
- coupon rewards
- Pixel Streaming or cloud demo
- B2B demo build

---

## Current Development Rule

```text
Do not overengineer.
Build playable gameplay first.
```
