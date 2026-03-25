# Changelog

## [0.4.0] - 2026-03-25

### Added
- **Smart Indoor Farming System Expansion**
  - `FarmStation` — improved crop harvesting logic and extended state handling
  - `IngredientType` — ingredient type definitions (e.g. tomato, lettuce)
  - `InventoryManager` — player inventory management system
  - `FarmInteractionBubble` — stage-based visual guidance for crop progression (seed / watering / harvest)

- **Farming Workflow Enhancements**
  - Extended gameplay structure for farming → harvesting → cooking → serving
  - Introduced crop-specific interaction branching

- **Editor Automation Tools**
  - `FarmStationBuilder` — editor tool for automated farm object generation and placement
  - Automated generation of multi-layer farm station structures
  - Scene placement alignment and repeatable object creation support

- **Visual and Scene Improvements**
  - Redesigned FarmStation visual structure into a 3-tier indoor farming rack
  - Updated scene layout and object placement
  - Applied Hanok-inspired thematic styling (wooden framing, lattice details, ambient lighting)
  - Improved visual clarity for interactive gameplay objects

### Changed
- `SampleScene.unity`
  - Reworked scene layout and gameplay object placement
  - Repositioned farm stations along the left-side wall layout

- `GameManager`
  - Improved gameplay flow handling

- `CookStation`
  - Refactored structure for future cooking flow expansion

- `InGameHUD`
  - Improved UI update flow

- `PlayerHand`
  - Extended support for ingredient handling flow

- `ServeCounter`
  - Improved serving logic structure

### Fixed
- Fixed isometric placement issue where objects appeared too high on screen
- Adjusted FarmStation coordinate placement logic based on isometric projection

---

## [0.3.0] - 2026-03-22

### Added
- **Implemented an isometric gameplay scene** (inspired by Overcooked / Cats & Soup-style readability)
  - `IsometricCameraController` — enforces runtime orthographic camera setup (Euler 30°/-45°, SE → NW)
  - `PlayerController` — WASD movement (New Input System) with isometric directional correction
  - `PlayerHand` — item holding / dropping state handling
  - `FarmStation` — crop harvesting station with cooldown and UI label: "🌿 Smart Indoor Farm"
  - `CookStation` — cooking station (grill)
  - `ServeCounter` — burger serving counter with served burger count tracking
  - `Interactable` / `InteractionBubble` — E-key interaction system with proximity speech bubble (World Space Canvas)
  - `InGameHUD` — in-game burger counter HUD integration
  - `RoomManager` — room join / leave logic for multiplayer flow

- **Editor Tools**
  - `SceneCleanup` → `Tools/Full Reset Isometric`
  - `KoreanFontSetup` → `Tools/Setup Korean Font`
  - `IsometricSetup`, `RoomSceneSetup`

- **Korean Font Support**
  - `Assets/Fonts/MalgunGothic SDF.asset`
  - Replaced default TMP font settings

### Changed
- `UIScreenController`
- `GameManager`
- `RoomState`

---

## [0.2.0] - 2026-03-XX

### Added
- Completed UI panel flow: MainMenu → Lobby → InGame → Result
- Added image assets

---

## [0.1.0] - 2026-03-XX

### Added
- Initial Unity project setup (WebGL build target)