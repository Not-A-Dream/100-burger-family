
```md
# 100 Burger Family — Project Context

## Project Overview

Genre:
- cooperative cooking simulation

Platform:
- Unity 6.3 LTS
- WebGL-first
- mobile-friendly target

Core Fantasy:
- parent ↔ child cooperation
- harvest → cook → serve gameplay loop

Business Direction:
- family telecom bundle services
- senior-care cooperative gameplay concepts

---

# Current MVP Goal

Build a playable local co-op MVP.

Priority:

```text
Playable state > perfect architecture
```

---

# Current Scene Structure

```text
SampleScene
├── Main Camera
├── Directional Light
├── EventSystem
├── Canvas
├── RoomScene
└── DontDestroyOnLoad
```

---

# Camera Setup

```text
Type: Orthographic
Rotation: (30, -45, 0)
Size: 7
Look Target: (0, 2, 0)
```

---

# UI Structure

```text
Canvas
├── TopPanel
├── Buttons
├── UIRoot
│   ├── MainMenuPanel
│   ├── LobbyPanel
│   ├── InGamePanel
│   └── ResultPanel
├── DebugToggleBtn
└── DebugPanel
```

---

# RoomScene Structure

```text
RoomScene
├── Floor
├── BackWall
├── LeftWall
├── FarmStation
├── CookStation
├── ServeCounter
└── Player
```

---

# Gameplay Stations

| Station | Role |
|---|---|
| FarmStation | Harvest ingredients |
| CookStation | Cook burgers |
| ServeCounter | Serve burgers and increase score |

---

# Important Coordinates

| Object | Position |
|---|---|
| FarmStation | (-2.5, 0.45, 3.5) |
| CookStation | (0, 0.45, 1.0) |
| ServeCounter | (2.5, 0.45, -1.5) |

---

# Important Scripts

```text
Assets/Scripts/
├── Core/
├── Game/
├── UI/
└── Editor/
```

Core gameplay scripts:

```text
GameManager.cs
RoomManager.cs
PlayerController.cs
PlayerHand.cs
FarmStation.cs
CookStation.cs
ServeCounter.cs
Interactable.cs
InteractionBubble.cs
IsometricCameraController.cs
```

---

# Current Priorities

1. Finish FarmStation
2. Finish CookStation
3. Finish ServeCounter
4. Tune movement and interaction
5. Complete gameplay loop

---

# Not Needed Yet

Do NOT prioritize:

- ECS
- Addressables
- advanced DI systems
- multiplayer abstraction
- backend optimization
- large-scale refactors

Reason:

```text
The project is still in MVP phase.
```
```

---