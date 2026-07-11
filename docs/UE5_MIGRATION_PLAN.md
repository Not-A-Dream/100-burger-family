# UE5 Migration Plan

## Goal

Rebuild 100 Burger Family as a playable Unreal Engine 5 MVP.

The Unity version is archived under `legacy-unity/` and should be treated as reference material.

## Migration Strategy

```text
Preserve Unity -> reset root docs -> create UE5 project -> build first playable loop
```

## Phase 1 - Repository Reset

- Move Unity project files into `legacy-unity/`
- Rewrite root documents for UE5
- Add UE5 `.gitignore`
- Prepare `unreal-game/` as the UE5 project folder
- Configure Git LFS before committing UE assets

## Phase 2 - UE5 Project Creation

- Create a new Unreal Engine 5 project
- Use Top Down Template
- Place it under `unreal-game/`
- Name the project `100BurgerFamily`
- Verify the project opens in Unreal Editor

## Phase 3 - First Playable Loop

Implement the smallest loop:

```text
Move -> Interact -> Harvest -> Cook -> Serve -> Score
```

Required assets:

```text
BP_PlayerCharacter
BP_InteractableBase
BP_FarmStation
BP_CookStation
BP_ServeCounter
BP_OrderManager
BP_GameMode_BurgerFamily
WBP_HUD
DA_Ingredient
DA_Recipe
```

## Phase 4 - Playability Pass

- Tune movement
- Tune camera angle
- Tune station collision
- Tune interaction prompt
- Add timer and score feedback
- Verify a 3-minute playable session

## Phase 5 - Visual Pass

- Add kitchen or restaurant assets from Fab
- Keep asset imports minimal
- Prefer stylized, readable props
- Avoid production art polish before the loop works

## Git LFS Requirement

Use Git LFS for Unreal binary assets:

```text
*.uasset filter=lfs diff=lfs merge=lfs -text
*.umap filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp4 filter=lfs diff=lfs merge=lfs -text
```

## Not Now

- multiplayer
- backend
- Pixel Streaming
- advanced GAS architecture
- production optimization
- large-scale C++ framework

## Definition of Done

The migration is successful when:

- `unreal-game/100BurgerFamily.uproject` opens
- the main map runs
- the player can interact with three stations
- score changes after serving
- HUD shows score and timer
