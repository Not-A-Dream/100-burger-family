# Changelog

## [0.5.1] - 2026-06-10

### Fixed
- Farm time sign no longer renders a large blue background over the crop prompt
- Reduced farm time sign size and rebuilt legacy signs when old layout parts are detected
- Simplified farm interaction bubble time badge so it does not cover the main text

### Changed
- Shortened several in-game debug/log messages for quicker feedback
- Improved held item rendering so the watering can appears in-hand more clearly
- Tuned HUD prompt and inventory text for tighter layout
- Kept the quick action `Buttons` panel work separate for the other branch

## [0.5.0] - 2026-03-29

![Architecture](../images/making_001.png)


### Added
- **Kitchen Stations — Full Cooking Pipeline (5 Stations)**
  - `Refrigerator` — visual prop; stainless steel fridge placed against the left wall as a contextual anchor explaining ingredient supply
  - `SupplyStation` (발주대) — traditional Hanok-style wooden order counter; [E] restocks Bread / RawPatty / RawBacon / Sauce (+3 each, 20 s cooldown)
  - `GrillStation` (불판) — cast-iron grill with 3-layer flame visuals (red → orange → yellow) and overhead ventilation hood; [E] grills patty + bacon in 15 s, 8 s collect window before burn
  - `CookStation` (조리대) — stainless-steel prep counter with Hanok lower frame; 5-stage burger assembly (Idle → Preparing → Assembling → AddSauce → Done)
  - `ServeCounter` (서빙 카운터) — Hanok wood counter with serving window and mini burger display; [E] submits completed burger → `GameManager.ServeBurger()` → burger count +1

- **Editor Automation**
  - `KitchenStationsBuilder` — single `Tools/100 Burger Family/Build Kitchen Stations` menu item that places all 5 stations in one pass without touching farm objects; safe to re-run (destroys and rebuilds station GameObjects only)

- **Complete Gameplay Loop**
  - Full pipeline now playable end-to-end: Farm → Supply → Grill → Cook → Serve

### Changed
- `SampleScene.unity`
  - Added Refrigerator, SupplyStation, GrillStation, CookStation, ServeCounter to `RoomScene`
  - Manual position adjustments applied to farm objects (TomatoFarm, LettuceFarm, WateringJar, character-b) preserved from prior session

### Notes
- Font Atlas warning (`MalgunGothic SDF`) on station labels is cosmetic; does not affect gameplay
- Refrigerator carries no script by design — `SupplyStation` handles all ingredient replenishment logic

---

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
# 2026-06-11

- Added an asset-pack UI setup path for `Assets/ExternalAssets/AssetPackUI/pack_001.png`.
- Reduced farm interaction/time bubbles so the timer badge no longer dominates the prompt text.
- Bound `UIScreenController` HUD references to the active in-game panels in `SampleScene`.
