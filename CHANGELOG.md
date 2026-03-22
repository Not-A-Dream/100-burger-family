# Changelog

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
  - `SceneCleanup` → `Tools/Full Reset Isometric` — 씬 1회성 전체 초기화 (카메라·스케일·스테이션 배치·캐릭터)
  - `KoreanFontSetup` → `Tools/Setup Korean Font` — MalgunGothic SDF 동적 폰트 생성 + 씬 전체 TMP 적용
  - `IsometricSetup`, `RoomSceneSetup` — 보조 에디터 도구

- **한국어 폰트**
  - `Assets/Fonts/MalgunGothic SDF.asset` — Dynamic TMP 폰트 에셋 생성
  - TMP Settings 기본 폰트 교체 (LiberationSans → MalgunGothic)

### Changed
- `UIScreenController` — InGame 패널 전환 시 게임 씬 연동
- `GameManager` — 게임 상태 관리 개선
- `RoomState` — 방 상태 필드 추가

### Known Issues (다음 작업 예정)
- 오브젝트 크기 문제 → 아래 [TODO] 참고

---

## [0.2.0] - 2026-03-XX

### Added
- UI 패널 흐름 완성: MainMenu → Lobby → InGame → Result
- 이미지 에셋 삽입

---

## [0.1.0] - 2026-03-XX

### Added
- Unity 프로젝트 초기 세팅 (WebGL 빌드 타겟)

---

# TODO (다음 세션 작업 목록)

## 🔴 우선순위 1 — 오브젝트 크기·배치 문제

**원인 분석**
- `character-b`는 `RoomScene/Characters/character-b` 계층 구조
- `RoomScene` 스케일이 커질수록 모든 자식 오브젝트 (가구·캐릭터)도 같이 커짐
- 단순히 RoomScene 스케일만 키우면 캐릭터도 같이 커지는 악순환
- 가구의 `localPosition`이 RoomScene 로컬 좌표 기준이라 RoomScene 스케일 변경 시 방 안/밖 기준이 달라짐

**해결 방향**
- [ ] `RoomScene` 스케일을 고정값으로 결정하고 더 이상 건드리지 않기
- [ ] 대신 카메라 `orthographicSize`만 조절해서 보이는 범위를 조정
- [ ] 각 오브젝트(가구·캐릭터)의 `localScale`을 RoomScene 스케일 기준으로 역산해서 고정
- [ ] 가구 위치는 Unity Inspector에서 직접 확인 후 좌표값을 코드에 반영

## 🔴 우선순위 2 — FarmBox 시각적 표현

**현황**
- FarmBox 모델이 단순 박스 형태 → "실내 스마트 재배기" 느낌 없음
- 뒷벽에 붙어서 잘 안 보임

**해결 방향**
- [ ] FarmBox 위치를 플레이어 이동 동선 위쪽 (왼쪽 벽 쪽)에 고정 배치
- [ ] 필요하면 색상 머티리얼 교체 또는 별도 3D 모델 교체 검토

## 🟡 우선순위 3 — 게임 플로우 미완성

**현황**
- 수확(FarmStation) → 조리(CookStation) → 서빙(ServeCounter) 순서가 코드 상 존재하지만 실제 재료 전달 로직 미완성
- 버거 카운트는 올라가지만 실제 조리 과정(재료 조합) 없이 바로 서빙 가능

**해결 방향**
- [ ] `PlayerHand`에 재료 종류(Ingredient 타입) 구분 추가
- [ ] `CookStation`에서 재료를 받아 조리 완료 후 버거 패티 생성
- [ ] `ServeCounter`에서 버거 번 + 패티 조합 검증 후 서빙 카운트

## 🟡 우선순위 4 — 멀티플레이어 2인 입장 연결

**현황**
- `RoomManager` 코드 완성됐지만 실제 게임 씬과 연결 미완성
- 부모/자녀 각각 다른 캐릭터 조작 필요

**해결 방향**
- [ ] 2번째 플레이어 캐릭터 오브젝트 추가 (character-a 또는 별도 모델)
- [ ] Photon 또는 Unity Netcode 연동 검토

## 🟢 우선순위 5 — 기타

- [ ] `RebuildUI.cs` 경고 수정: `enableWordWrapping` → `textWrappingMode`
- [ ] Child1Body/Head/Hat이 에디터 씬에 잔존 → `Full Reset Isometric` 실행 후에도 씬 파일에 남아 있는 문제 확인
- [ ] 상호작용 말풍선(InteractionBubble) 크기가 캐릭터 대비 너무 크거나 작을 수 있음 → 크기 조정 필요
