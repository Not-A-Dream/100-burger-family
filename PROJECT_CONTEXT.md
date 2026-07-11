# 100 Burger Family - Project Context

## Project Overview

Genre:
- cooperative cooking simulation

Engine:
- Unreal Engine 5

Current Direction:
- full migration from Unity to UE5
- Blueprint-first development
- Top Down Template based MVP

Core Fantasy:
- parent and child cooperation
- harvest, cook, serve gameplay loop
- family-friendly kitchen chaos

Business Direction:
- family telecom bundle services
- senior-care cooperative gameplay concepts
- future AI-assisted content iteration

---

## Current MVP Goal

Build a playable UE5 local prototype.

Priority:

```text
Playable state > perfect architecture
```

---

## Target Project Structure

```text
unreal-game/
  100BurgerFamily.uproject
  Config/
  Content/
    BurgerFamily/
      Blueprints/
        Core/
        Player/
        Stations/
        UI/
      Data/
      Maps/
      Materials/
      Art/
      Input/
  Source/
    100BurgerFamily/
```

Legacy Unity reference:

```text
legacy-unity/
```

---

## Recommended UE5 Starting Point

Template:
- Top Down Template

Reason:
- closest to current isometric cooking sim direction
- fast player/camera setup
- easy station interaction layout
- good fit for Blueprint iteration

---

## Core Gameplay Loop

```text
Harvest -> Cook -> Serve -> Score
```

Gameplay stations:

| Station | Role |
|---|---|
| BP_FarmStation | Produce ingredients |
| BP_CookStation | Cook burger items |
| BP_ServeCounter | Serve orders and increase score |

---

## Initial Blueprint Targets

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

---

## Current Priorities

1. Create UE5 Top Down project under `unreal-game/`
2. Configure input and camera
3. Implement interaction key
4. Implement FarmStation
5. Implement CookStation
6. Implement ServeCounter
7. Add score and timer HUD
8. Verify a 3-minute playable loop

---

## Not Needed Yet

Do NOT prioritize:

- multiplayer
- backend
- Pixel Streaming
- save systems
- advanced GAS architecture
- large C++ framework
- production asset optimization

Reason:

```text
The project is still in MVP phase.
```
