# 100 Burger Family

100 Burger Family는 Unreal Engine 5 기반 cooking simulation MVP로 다시 제작합니다.

기존 Unity 프로젝트는 삭제하지 않고 `legacy-unity/`에 보존했습니다.

## 방향

```text
Engine: Unreal Engine 5
Workflow: Blueprint-first
Template: Top Down Template
Target: Windows playable MVP first
```

## 핵심 루프

```text
Harvest -> Cook -> Serve -> Score
```

## MVP 범위

- top-down player movement
- interact key
- farm station
- cook station
- serve counter
- score HUD
- timer HUD
- one playable map

## 저장소 구조

```text
100-burger-family/
  README.md
  README_KO.md
  AGENTS.md
  PROJECT_CONTEXT.md
  TODO.md
  docs/
    UE5_MIGRATION_PLAN.md
  unreal-game/
    100BurgerFamily.uproject
  legacy-unity/
    unity-game/
    My project/
    100-burger-family.sln
```

## UE5 Content 계획

```text
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
```

## 초기 Blueprint

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

## 개발 원칙

- gameplay first
- Blueprint-first
- C++는 꼭 필요할 때만 사용
- UE5 template과 Fab asset을 적극 활용
- 첫 playable map을 단순하게 유지
- MVP 전에는 대형 architecture를 만들지 않음

## Legacy Unity

기존 Unity 버전은 아래 폴더에 보존합니다.

```text
legacy-unity/
```

사용 목적:
- gameplay reference
- old loop reference
- UI/flow notes
- asset reference

명시 요청이 없으면 Unity 구현은 계속하지 않습니다.
