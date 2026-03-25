# Changelog

## [0.4.0] - 2026-03-25

### Added
- **스마트 실내 재배기 시스템 확장**
  - `FarmStation` — 작물 수확 로직 개선 및 상태 확장
  - `IngredientType` — 재료 타입 정의 (토마토, 양상추 등)
  - `InventoryManager` — 플레이어 인벤토리 관리 시스템 추가
  - `FarmInteractionBubble` — 작물 성장 단계별 시각 가이드 (씨앗 / 물주기 / 수확)

- **농장 워크플로우 개선**
  - 재배 → 수확 → 조리 → 서빙 흐름 기반 구조 확장
  - 작물별 상호작용 분기 기반 시스템 도입

- **에디터 자동 배치 시스템**
  - `FarmStationBuilder` — 농장 오브젝트 자동 생성 및 배치 툴
  - 3층 구조 재배기 자동 생성 (카트형 구조)
  - 씬 내 위치 자동 정렬 및 반복 생성 지원

- **비주얼 및 씬 개선**
  - FarmStation 모델 구조 개선 (3단 구조 재배기)
  - 씬 내 오브젝트 재배치 및 레이아웃 조정
  - 한옥 테마 스타일 적용 (목재 구조, 창살, 조명 등)
  - 상호작용 오브젝트 시각적 명확성 개선

### Changed
- `SampleScene.unity`
  - 전체 레이아웃 재구성 (FarmStation, 동선, 오브젝트 배치)
  - 농장 위치 좌측 벽 기준으로 재정렬

- `GameManager`
  - 게임 흐름 제어 로직 일부 개선

- `CookStation`
  - 조리 로직 확장 준비 구조로 리팩토링

- `InGameHUD`
  - UI 업데이트 흐름 개선

- `PlayerHand`
  - 재료 처리 흐름 확장 대응

- `ServeCounter`
  - 서빙 처리 구조 개선

### Fixed
- 아이소메트릭 좌표 기준으로 오브젝트가 화면 상단에 몰리는 문제 수정
- FarmStation 위치 계산 로직 보정 (x + z 기준 보정)

---

## [0.3.0] - 2026-03-22

### Added
- **아이소메트릭 게임 씬 구현** (Overcooked / 고양이와 비밀레시피 스타일)
  - `IsometricCameraController` — 런타임 직교 카메라(Euler 30°/-45°, SE → NW) 강제 적용
  - `PlayerController` — WASD 이동 (New Input System), 아이소메트릭 방향 보정
  - `PlayerHand` — 재료 들기/내려놓기 상태 관리
  - `FarmStation` — 농장 수확 스테이션, 쿨다운 포함, UI "🌿 실내 스마트 재배기"
  - `CookStation` — 조리 스테이션 (그릴)
  - `ServeCounter` — 버거 서빙 카운터, 버거 카운트 증가
  - `Interactable` / `InteractionBubble` — E키 상호작용 시스템, 근접 말풍선 (World Space Canvas)
  - `InGameHUD` — 버거 카운트 HUD 연동
  - `RoomManager` — 멀티플레이어 방 참여/나가기 로직

- **에디터 툴**
  - `SceneCleanup` → `Tools/Full Reset Isometric`
  - `KoreanFontSetup` → `Tools/Setup Korean Font`
  - `IsometricSetup`, `RoomSceneSetup`

- **한국어 폰트**
  - `Assets/Fonts/MalgunGothic SDF.asset`
  - TMP Settings 기본 폰트 교체

### Changed
- `UIScreenController`
- `GameManager`
- `RoomState`

---

## [0.2.0] - 2026-03-XX

### Added
- UI 패널 흐름 완성: MainMenu → Lobby → InGame → Result
- 이미지 에셋 삽입

---

## [0.1.0] - 2026-03-XX

### Added
- Unity 프로젝트 초기 세팅 (WebGL 빌드 타겟)