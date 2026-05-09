# 100 Burger Family — Codex 작업 지침

## 1. 설명 방식 (가장 중요)

코드를 수정할 때는 **반드시 변경 이유와 작동 원리를 함께 설명**한다.

### 원칙: "무엇을"이 아니라 "왜/어떻게"를 설명한다

❌ 나쁜 예:
> `orthographicSize`를 6에서 7로 바꿨습니다.

✅ 좋은 예:
> Unity는 Inspector에 저장된 값(직렬화)이 코드 기본값보다 항상 우선합니다.
> 따라서 코드에서 `= 7f`로 바꿔도 씬 파일에 `6`이 저장되어 있으면 무시됩니다.
> 이를 우회하려면 `const`로 선언하거나 `Awake()`에서 강제 대입합니다.

### 시각적 비교 다이어그램을 적극 사용한다

```
변경 전                    변경 후
┌────────────┐            ┌────────────┐
│ TopPanel   │            │ TopPanel   │
│ (방 가림)  │    →       │ 방 전체 보임│
│ 뒷벽 안보임│            │            │
└────────────┘            └────────────┘
```

### 변경 사항은 표로 요약한다

| 항목 | 전 | 후 | 이유 |
|---|---|---|---|
| orthographicSize | 6 | 7 | 방 전체가 TopPanel 아래로 들어오게 |
| lookTarget.y | 0 | 2 | y 올리면 방이 뷰포트에서 아래로 이동 |

---

## 2. Unity 특유의 주의사항 (자주 헷갈리는 것들)

### Inspector 직렬화 우선순위
```
씬 파일(.unity)에 저장된 값  >  코드의 public 필드 기본값
```
- `public float size = 7f` 변경은 **기존 컴포넌트에 영향 없음**
- 즉시 적용하려면: `const` 사용 또는 `Awake()`에서 강제 대입

### Play 모드 중 변경은 저장되지 않음
- MCP Unity 툴로 런타임에 수정한 값은 **Stop 하면 사라짐**
- 영구 적용: Editor 스크립트에서 Edit 모드에서 수정 + `EditorSceneManager.SaveOpenScenes()`

### Undo.DestroyObjectImmediate는 자식을 루트로 탈출시킴
- `Object.DestroyImmediate()` + `DestroyChildrenRecursive()` 조합으로 대체

### DontDestroyOnLoad 오브젝트
- Edit 모드(Stop 상태)에서는 존재하지 않음
- Play 시작 시 씬에서 생성되어야 함
- Lazy 싱글톤(`I` getter에서 자동 생성) 패턴 권장

---

## 3. 프로젝트 컨텍스트

### 게임 개요
- **장르**: 협동 요리 시뮬레이션 (로컬 MVP)
- **플랫폼**: Unity 6.3 LTS, WebGL (모바일 최적화 목표)
- **컨셉**: 부모(물주기) ↔ 자녀(요리) 협동으로 100개 버거 달성
- **B2B 전략**: SKT/KT/LGU+ 가족 요금제 부가서비스, 시니어 케어 플랫폼

### 씬 구조
```
SampleScene
├── Main Camera (IsometricCameraController)
│     카메라: Euler(30,-45,0), orthographic, size=7, lookTarget=(0,2,0)
├── Directional Light
├── EventSystem
├── Canvas (UIScreenController, UIController)
│   ├── TopPanel          ← 인게임 HUD (버거카운트 등), Canvas 직접 자식
│   ├── Buttons           ← 인게임 버튼들
│   ├── UIRoot
│   │   ├── MainMenuPanel (MainMenuController)
│   │   ├── LobbyPanel    (LobbyController)
│   │   ├── InGamePanel   (InGameHUD)
│   │   └── ResultPanel
│   ├── DebugToggleBtn
│   └── DebugPanel
├── RoomScene             ← 방 + 스테이션 + Player (RoomRebuild로 재건)
│   ├── Floor / BackWall / LeftWall
│   ├── FarmStation / CookStation / ServeCounter  (작업 중: 수동 제작 예정)
│   └── Player (scale=0.4, Capsule, 빨간색)
└── DontDestroyOnLoad     ← Play 중에만 존재
    ├── GameManager
    └── [RoomManager]     ← Lazy 자동 생성
```

### 현재 게임 스테이션 위치 (RoomScene 좌표)
| 스테이션 | 위치 | 색 | 게임 역할 |
|---|---|---|---|
| FarmStation | (-2.5, 0.45, 3.5) | 초록 | [E] 재료 수확, 6초 쿨다운 |
| CookStation | (0, 0.45, 1.0) | 주황 | [E] 조리 시작 |
| ServeCounter | (2.5, 0.45, -1.5) | 파랑 | [E] 서빙 → 버거 카운트 +1 |

### 핵심 스크립트 위치
```
Assets/Scripts/
├── Core/     GameManager.cs, RoomManager.cs
├── Game/     PlayerController.cs, PlayerHand.cs, FarmStation.cs,
│             CookStation.cs, ServeCounter.cs, Interactable.cs,
│             InteractionBubble.cs, InGameHUD.cs,
│             IsometricCameraController.cs
├── UI/       MainMenuController.cs, LobbyController.cs,
│             InGameController.cs, UIScreenController.cs
└── Editor/   RoomRebuild.cs, HUDFix.cs
```

### Editor 메뉴 (Tools → 100 Burger Family)
- **씬 초기화 (처음부터 재건)** — RoomScene 전체 삭제 후 재건
- **HUD 수정** — TopPanel 레이아웃 조정 (Edit 모드에서 실행)

---

## 4. 남은 작업 (빌드 로드맵)

### 현재 단계: 게임 오브젝트 수동 제작
1. **스마트 실내 재배기** (FarmStation, 좌측) — 진행 중
2. **조리대** (CookStation, 중앙)
3. **서빙 카운터** (ServeCounter, 우측)
4. 플레이어 이동/상호작용 최종 튜닝

### 이후 단계
5. 멀티플레이어 (Firebase Realtime DB or Photon)
6. 쿠폰/리워드 시스템 (버거 100개 달성)
7. WebGL 빌드 최적화
8. B2B 데모 영상 제작

---

## 5. 한국어로 대화한다
이 프로젝트의 모든 대화는 한국어로 진행한다.
코드 주석, 로그 메시지도 한국어 유지.
