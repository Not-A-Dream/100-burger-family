using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 주방 스테이션 5종을 RoomScene 에 배치하는 에디터 빌더.
///
/// ═══════════════════════════════════════════════
///  배치 순서 (왼쪽 → 오른쪽, 게임 플로우 순)
/// ═══════════════════════════════════════════════
///
///  팜(left back)  →  냉장고(left wall)  →  발주대(left front)
///      →  불판(center-left)  →  조리대(center)  →  서빙대(right)
///
/// 디자인 컨셉:
///   발주대  — 한옥 전통 목재 카운터 (소나무/참나무 계열)
///   냉장고  — 현대 스테인리스 + 흰색 (한옥 속 현대 가전)
///   불판    — 주철 그릴 + 화염 + 후드 (검정/주황/회색)
///   조리대  — 스테인리스 상판 + 한옥 하부 (밝은 회색/참나무)
///   서빙대  — 한옥 목재 카운터 + 단청 포인트 + 서빙 창구
///
/// 색상 팔레트 (FarmStationBuilder 와 공유):
///   소나무  #BF8B47   참나무  #80521F   대들보  #402809
///   단청    #B82614   한지    #FFE085   철재    #3A2E24
///   + 주방: 스테인리스 #C8C8CC, 냉장흰색 #EEEEEE, 주철 #1A1412
/// </summary>
public static class KitchenStationsBuilder
{
    // ── 한옥 색상 (FarmStationBuilder 동일 팔레트) ────────────────
    static readonly Color C_PINE      = new Color(0.75f, 0.55f, 0.28f); // 소나무 기둥
    static readonly Color C_OAK       = new Color(0.50f, 0.32f, 0.14f); // 참나무 본체
    static readonly Color C_BEAM      = new Color(0.25f, 0.16f, 0.06f); // 대들보 (어두운)
    static readonly Color C_DANCHEONG = new Color(0.72f, 0.15f, 0.08f); // 단청 포인트
    static readonly Color C_HANJI     = new Color(1.00f, 0.88f, 0.52f); // 한지 조명
    static readonly Color C_IRON      = new Color(0.22f, 0.18f, 0.14f); // 철재

    // ── 주방 전용 ─────────────────────────────────────────────────
    static readonly Color C_STEEL     = new Color(0.78f, 0.78f, 0.80f); // 스테인리스 밝음
    static readonly Color C_STEEL_DRK = new Color(0.50f, 0.50f, 0.52f); // 스테인리스 어두움
    static readonly Color C_FRIDGE    = new Color(0.93f, 0.93f, 0.93f); // 냉장고 흰색
    static readonly Color C_FRIDGE_T  = new Color(0.72f, 0.72f, 0.72f); // 냉장고 테두리
    static readonly Color C_COLD      = new Color(0.68f, 0.88f, 1.00f); // 냉기 파란빛
    static readonly Color C_GRILL     = new Color(0.10f, 0.08f, 0.07f); // 주철 그릴
    static readonly Color C_GRILL_BAR = new Color(0.16f, 0.13f, 0.10f); // 그릴 격자 막대
    static readonly Color C_FIRE_R    = new Color(0.85f, 0.12f, 0.04f); // 불꽃 빨강
    static readonly Color C_FIRE_O    = new Color(0.98f, 0.46f, 0.04f); // 불꽃 주황
    static readonly Color C_FIRE_Y    = new Color(1.00f, 0.84f, 0.20f); // 불꽃 노랑
    static readonly Color C_HOOD      = new Color(0.38f, 0.38f, 0.40f); // 후드 회색
    static readonly Color C_SERVE     = new Color(0.88f, 0.78f, 0.58f); // 서빙대 나무

    // ── 배치 좌표 ─────────────────────────────────────────────────
    // 왜 이 좌표?
    //   - z 양수(뒤) → 팜 스테이션 구역 (z=1~2.5)
    //   - z 음수(앞) → 플레이어 동선 앞쪽
    //   - x 음수(좌) → 팜/냉장/발주 (재료 수급 구역)
    //   - x 양수(우) → 그릴/조리/서빙 (조리 완성 구역)
    static readonly Vector3 POS_FRIDGE = new Vector3(-3.6f, 0f, -0.5f); // 왼쪽 벽
    static readonly Vector3 POS_SUPPLY = new Vector3(-2.0f, 0f, -1.0f); // 왼쪽 앞
    static readonly Vector3 POS_GRILL  = new Vector3(-0.6f, 0f,  1.0f); // 중앙 왼쪽
    static readonly Vector3 POS_COOK   = new Vector3( 0.6f, 0f,  0.2f); // 중앙
    static readonly Vector3 POS_SERVE  = new Vector3( 2.5f, 0f, -1.5f); // 오른쪽

    // ═════════════════════════════════════════════════════════════
    //  메뉴 엔트리
    // ═════════════════════════════════════════════════════════════

    [MenuItem("Tools/100 Burger Family/Build Kitchen Stations")]
    public static void BuildAll()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[KitchenStationsBuilder] Stop 후 Edit 모드에서 실행하세요.");
            return;
        }
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene == null)
        {
            Debug.LogError("[KitchenStationsBuilder] RoomScene 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 기존 스테이션 제거 후 재건
        foreach (string n in new[] { "Refrigerator", "SupplyStation", "GrillStation", "CookStation", "ServeCounter" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        BuildRefrigerator(roomScene.transform);
        BuildSupplyStation(roomScene.transform);
        BuildGrillStation(roomScene.transform);
        BuildCookStation(roomScene.transform);
        BuildServeCounter(roomScene.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[KitchenStationsBuilder] ✅ 냉장고·발주대·불판·조리대·서빙대 배치 완료!");
    }

    // ═════════════════════════════════════════════════════════════
    //  1. 냉장고 (시각 소품 — 스크립트 없음)
    //
    //   현대 스테인리스 냉장고가 한옥 안에 있는 컨셉.
    //   왜 스크립트 없음? → 발주대(SupplyStation)가 재료 지급을 담당.
    //   냉장고는 "왜 재료가 즉시 나오는가"를 설명하는 시각적 이유 역할.
    //
    //   ┌───────┐  ← 상단 냉동실 (전체의 1/3)
    //   │  ○    │  ← 냉동실 손잡이 (스테인리스 바)
    //   ├───────┤  ← 구분선 + 냉기 LED
    //   │       │
    //   │  ○    │  ← 냉장실 손잡이
    //   │       │
    //   └───────┘
    //    ─ ─ ─ ─   ← 발 4개 (짧은 원통)
    // ═════════════════════════════════════════════════════════════
    static void BuildRefrigerator(Transform parent)
    {
        var go = new GameObject("Refrigerator");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = POS_FRIDGE;

        const float FW = 0.76f, FD = 0.60f, FH = 1.72f;
        const float FOOT_Y  = 0.04f;  // 발 높이
        const float BODY_CY = FOOT_Y + FH * 0.5f; // 본체 중심 Y

        // 플레이어가 냉장고를 통과하지 못하게 막는 콜라이더
        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(FW, FH, FD);
        col.center = new Vector3(0f, BODY_CY, 0f);

        // ── 본체 ─────────────────────────────────────────────────
        SubBox(go.transform, "Body",    new Vector3(0f, BODY_CY, 0f), new Vector3(FW, FH, FD), C_FRIDGE);

        // 좌우 테두리 (스테인리스 느낌)
        SubBox(go.transform, "Rim_L",   new Vector3(-(FW*0.5f - 0.015f), BODY_CY, 0f), new Vector3(0.025f, FH - 0.04f, FD - 0.02f), C_FRIDGE_T);
        SubBox(go.transform, "Rim_R",   new Vector3(  FW*0.5f - 0.015f,  BODY_CY, 0f), new Vector3(0.025f, FH - 0.04f, FD - 0.02f), C_FRIDGE_T);
        SubBox(go.transform, "Rim_Top", new Vector3(0f, FOOT_Y + FH + 0.02f, 0f),      new Vector3(FW, 0.04f, FD), C_FRIDGE_T);

        // ── 문 패널 (앞면) ───────────────────────────────────────
        // 냉동실: 상단 1/3
        float freezeH = FH * 0.34f;
        float fridgeH = FH * 0.62f;
        float freezeCY = FOOT_Y + fridgeH + freezeH * 0.5f + 0.025f;
        float fridgeCY = FOOT_Y + fridgeH * 0.5f;

        SubBox(go.transform, "FreezerDoor", new Vector3(0f, freezeCY, -(FD*0.5f - 0.01f)),
            new Vector3(FW - 0.04f, freezeH - 0.04f, 0.03f), C_FRIDGE);
        SubBox(go.transform, "FridgeDoor",  new Vector3(0f, fridgeCY,  -(FD*0.5f - 0.01f)),
            new Vector3(FW - 0.04f, fridgeH - 0.04f, 0.03f), C_FRIDGE);

        // 냉동/냉장 구분선 + 냉기 LED
        float divY = FOOT_Y + fridgeH + 0.015f;
        SubBox(go.transform, "DivBar", new Vector3(0f, divY, -(FD*0.5f)), new Vector3(FW, 0.025f, 0.02f), C_STEEL_DRK);
        SubBox(go.transform, "ColdLED", new Vector3(0f, divY + 0.02f, -(FD*0.5f - 0.04f)),
            new Vector3(FW - 0.20f, 0.015f, 0.02f), C_COLD);

        // ── 손잡이 (좌측에 세로 바 형태) ────────────────────────
        float hx = FW * 0.5f - 0.04f;
        SubBox(go.transform, "Handle_Freeze", new Vector3(hx, freezeCY, -(FD*0.5f + 0.04f)), new Vector3(0.04f, 0.22f, 0.04f), C_STEEL_DRK);
        SubBox(go.transform, "Handle_Fridge", new Vector3(hx, fridgeCY, -(FD*0.5f + 0.04f)), new Vector3(0.04f, 0.22f, 0.04f), C_STEEL_DRK);

        // ── 발 4개 ───────────────────────────────────────────────
        float fx = FW*0.5f - 0.09f, fz = FD*0.5f - 0.09f;
        SubCyl(go.transform, "Foot_FL", new Vector3(-fx, 0.02f, -fz), new Vector3(0.05f, 0.02f, 0.05f), C_STEEL_DRK);
        SubCyl(go.transform, "Foot_FR", new Vector3( fx, 0.02f, -fz), new Vector3(0.05f, 0.02f, 0.05f), C_STEEL_DRK);
        SubCyl(go.transform, "Foot_BL", new Vector3(-fx, 0.02f,  fz), new Vector3(0.05f, 0.02f, 0.05f), C_STEEL_DRK);
        SubCyl(go.transform, "Foot_BR", new Vector3( fx, 0.02f,  fz), new Vector3(0.05f, 0.02f, 0.05f), C_STEEL_DRK);

        // 상단 소형 디스플레이 (온도 표시 느낌)
        SubBox(go.transform, "TempDisplay", new Vector3(-FW*0.5f + 0.15f, FOOT_Y + FH + 0.005f, -(FD*0.5f - 0.12f)),
            new Vector3(0.18f, 0.02f, 0.10f), new Color(0.10f, 0.18f, 0.28f));

        EditorUtility.SetDirty(go);
    }

    // ═════════════════════════════════════════════════════════════
    //  2. 발주대 (SupplyStation)
    //
    //   한옥 전통 목재 카운터 — 재료 주문/보충 스테이션.
    //   [E]를 누르면 빵·생패티·생베이컨·소스를 각 3개씩 인벤토리에 추가.
    //   20초 쿨다운으로 남용 방지.
    //
    //   ┌──────────────────┐  ← 소나무 상판 + 단청 앞몰딩
    //   │ 📄  📦  🔔       │  ← 발주서 종이, 공급 박스, 주문 벨
    //   ├──────────────────┤  ← 서랍 구분선 (대들보색)
    //   │ [──────] [─────] │  ← 서랍 손잡이 (철재)
    //   │ [───────────────]│  ← 하단 서랍 손잡이
    //   └──────────────────┘
    //   ██              ██   ← 다리 4개 (대들보색)
    // ═════════════════════════════════════════════════════════════
    static void BuildSupplyStation(Transform parent)
    {
        var go = new GameObject("SupplyStation");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = POS_SUPPLY;

        const float W = 1.20f, D = 0.70f, TOP_Y = 0.88f;

        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(W, 0.95f, D);
        col.center = new Vector3(0f, 0.475f, 0f);

        // ── 상판 ─────────────────────────────────────────────────
        SubBox(go.transform, "Top",
            new Vector3(0f, TOP_Y, 0f), new Vector3(W, 0.065f, D), C_PINE);
        // 단청 앞면 몰딩
        SubBox(go.transform, "TopFront",
            new Vector3(0f, TOP_Y - 0.01f, -(D*0.5f + 0.01f)),
            new Vector3(W, 0.065f, 0.025f), C_DANCHEONG);

        // ── 본체 ─────────────────────────────────────────────────
        SubBox(go.transform, "Body",
            new Vector3(0f, 0.44f, 0f), new Vector3(W - 0.04f, 0.83f, D - 0.04f), C_OAK);
        SubBox(go.transform, "FrontFace",
            new Vector3(0f, 0.44f, -(D*0.5f - 0.012f)),
            new Vector3(W - 0.08f, 0.79f, 0.04f), C_PINE);

        // ── 서랍 ─────────────────────────────────────────────────
        SubBox(go.transform, "DrawDiv_H",
            new Vector3(0f, 0.62f, -(D*0.5f)),
            new Vector3(W - 0.10f, 0.025f, 0.02f), C_BEAM);
        SubBox(go.transform, "DrawDiv_V",
            new Vector3(0.05f, 0.44f, -(D*0.5f)),
            new Vector3(0.025f, 0.78f, 0.02f), C_BEAM);
        // 손잡이 3개
        SubBox(go.transform, "Handle_TL",
            new Vector3(-0.35f, 0.73f, -(D*0.5f + 0.03f)),
            new Vector3(0.22f, 0.032f, 0.04f), C_IRON);
        SubBox(go.transform, "Handle_TR",
            new Vector3( 0.30f, 0.73f, -(D*0.5f + 0.03f)),
            new Vector3(0.40f, 0.032f, 0.04f), C_IRON);
        SubBox(go.transform, "Handle_B",
            new Vector3(-0.12f, 0.40f, -(D*0.5f + 0.03f)),
            new Vector3(0.55f, 0.032f, 0.04f), C_IRON);

        // ── 다리 4개 ─────────────────────────────────────────────
        float lx = W*0.5f - 0.06f, lz = D*0.5f - 0.06f;
        SubBox(go.transform, "Leg_FL", new Vector3(-lx, 0.04f, -lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);
        SubBox(go.transform, "Leg_FR", new Vector3( lx, 0.04f, -lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);
        SubBox(go.transform, "Leg_BL", new Vector3(-lx, 0.04f,  lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);
        SubBox(go.transform, "Leg_BR", new Vector3( lx, 0.04f,  lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);

        // ── 상판 소품 ────────────────────────────────────────────
        // 발주서 (누런 한지 종이)
        SubBox(go.transform, "Paper",
            new Vector3(-0.30f, TOP_Y + 0.04f, 0.06f),
            new Vector3(0.26f, 0.018f, 0.32f), new Color(0.95f, 0.90f, 0.75f));
        // 공급 박스 (골판지 2개)
        SubBox(go.transform, "Box1",
            new Vector3(0.20f, TOP_Y + 0.09f, 0.05f),
            new Vector3(0.20f, 0.14f, 0.20f), new Color(0.82f, 0.68f, 0.44f));
        SubBox(go.transform, "Box2",
            new Vector3(0.44f, TOP_Y + 0.07f, 0.00f),
            new Vector3(0.12f, 0.11f, 0.12f), new Color(0.85f, 0.72f, 0.50f));
        // 소스 병 (빨간 원통)
        SubCyl(go.transform, "Sauce",
            new Vector3(0.44f, TOP_Y + 0.14f, 0.16f),
            new Vector3(0.05f, 0.07f, 0.05f), C_DANCHEONG);
        // 주문 벨 (반구 + 베이스)
        SubSph(go.transform, "Bell",
            new Vector3(-0.30f, TOP_Y + 0.085f, -0.18f),
            new Vector3(0.08f, 0.072f, 0.08f), C_STEEL);
        SubCyl(go.transform, "BellBase",
            new Vector3(-0.30f, TOP_Y + 0.035f, -0.18f),
            new Vector3(0.062f, 0.018f, 0.062f), C_STEEL_DRK);

        BuildLabel(go.transform, "발주대", TOP_Y + 0.55f);

        var s = go.AddComponent<SupplyStation>();
        s.restockAmount   = 3;
        s.restockCooldown = 20f;
        go.AddComponent<InteractionBubble>();
        EditorUtility.SetDirty(go);
    }

    // ═════════════════════════════════════════════════════════════
    //  3. 불판 (GrillStation)
    //
    //   주철 그릴 + 실제 화염 + 환기 후드.
    //   [E] → 생패티 + 생베이컨 → 15초 굽기 → Done → 8초 내 수거
    //   수거 실패 시 Burned (재료 소실).
    //
    //       ┌──────────────────────┐  ← 후드 (연기 흡입)
    //       │  ≡ ≡ ≡ ≡ ≡ ≡         │  ← 환기 슬릿
    //       └──────────────────────┘
    //   ═══════════════════════════  ← 그릴 격자 (가로+세로 막대)
    //   ▓ ▒ ▓ ▒ ▓  불꽃 3층         ← 빨강/주황/노랑
    //   ───────────────────────────  ← 주철 상판
    //   │  [○] [○] [○]  제어 노브  │  ← 본체 앞면
    //   └───────────────────────────┘
    //   ████                    ████  ← 다리 (철재)
    // ═════════════════════════════════════════════════════════════
    static void BuildGrillStation(Transform parent)
    {
        var go = new GameObject("GrillStation");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = POS_GRILL;

        const float GW = 1.10f, GD = 0.70f;
        const float BODY_H = 0.82f;
        const float GRILL_Y = BODY_H + 0.055f; // 그릴 면 높이

        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(GW, BODY_H + 0.10f, GD);
        col.center = new Vector3(0f, (BODY_H + 0.10f) * 0.5f, 0f);

        // ── 본체 ─────────────────────────────────────────────────
        SubBox(go.transform, "Body",
            new Vector3(0f, BODY_H * 0.5f, 0f), new Vector3(GW, BODY_H, GD), C_GRILL);
        SubBox(go.transform, "FrontFace",
            new Vector3(0f, BODY_H * 0.5f, -(GD*0.5f - 0.01f)),
            new Vector3(GW - 0.04f, BODY_H - 0.04f, 0.04f),
            new Color(0.16f, 0.13f, 0.11f));

        // 제어 패널 (앞면 하단)
        SubBox(go.transform, "CtrlPanel",
            new Vector3(0f, 0.18f, -(GD*0.5f + 0.02f)),
            new Vector3(GW * 0.62f, 0.15f, 0.04f),
            new Color(0.20f, 0.18f, 0.15f));
        // 노브 3개
        float[] nkX = { -0.24f, 0f, 0.24f };
        foreach (float nx in nkX)
            SubCyl(go.transform, "Knob",
                new Vector3(nx, 0.18f, -(GD*0.5f + 0.05f)),
                new Vector3(0.04f, 0.022f, 0.04f), C_STEEL);

        // ── 그릴 상판 ────────────────────────────────────────────
        SubBox(go.transform, "GrillTop",
            new Vector3(0f, GRILL_Y, 0f),
            new Vector3(GW + 0.04f, 0.06f, GD + 0.04f), C_GRILL);

        // ── 그릴 격자 (가로 5줄 + 세로 5줄) ─────────────────────
        // 왜 격자? 실제 그릴처럼 고기가 굽는 면을 시각적으로 표현
        float[] grillZs = { -0.22f, -0.11f, 0f, 0.11f, 0.22f };
        float[] grillXs = { -0.38f, -0.19f,  0f, 0.19f, 0.38f };
        foreach (float gz in grillZs)
            SubBox(go.transform, "GBar_H",
                new Vector3(0f, GRILL_Y + 0.045f, gz),
                new Vector3(GW - 0.04f, 0.028f, 0.022f), C_GRILL_BAR);
        foreach (float gx in grillXs)
            SubBox(go.transform, "GBar_V",
                new Vector3(gx, GRILL_Y + 0.045f, 0f),
                new Vector3(0.022f, 0.028f, GD - 0.04f), C_GRILL_BAR);

        // ── 화염 3층 (빨강→주황→노랑) ───────────────────────────
        // 왜 3층? 실제 불꽃은 중심이 가장 뜨겁고(빨강) 끝이 노랑
        Color[] fireCols = { C_FIRE_R, C_FIRE_O, C_FIRE_Y };
        float[] fireYs   = { GRILL_Y - 0.040f, GRILL_Y - 0.005f, GRILL_Y + 0.025f };
        float[] fireH    = { 0.072f, 0.052f, 0.030f };
        float[] fireW    = { 0.120f, 0.090f, 0.058f };

        for (int layer = 0; layer < 3; layer++)
        {
            for (int fi = 0; fi < 6; fi++)
            {
                // 불규칙한 간격으로 실제 불꽃 느낌 연출
                float fxPos = -0.42f + fi * 0.17f + (fi % 2 == 0 ? 0.04f : -0.04f);
                float fzPos = (fi % 3 - 1) * 0.07f;
                SubBox(go.transform, $"Fire{layer}_{fi}",
                    new Vector3(fxPos, fireYs[layer], fzPos),
                    new Vector3(fireW[layer], fireH[layer], fireW[layer] * 0.75f),
                    fireCols[layer]);
            }
        }

        // ── 다리 4개 ─────────────────────────────────────────────
        float ldx = GW*0.5f - 0.06f, ldz = GD*0.5f - 0.06f;
        SubBox(go.transform, "Leg_FL", new Vector3(-ldx, 0.04f, -ldz), new Vector3(0.09f, 0.08f, 0.09f), C_IRON);
        SubBox(go.transform, "Leg_FR", new Vector3( ldx, 0.04f, -ldz), new Vector3(0.09f, 0.08f, 0.09f), C_IRON);
        SubBox(go.transform, "Leg_BL", new Vector3(-ldx, 0.04f,  ldz), new Vector3(0.09f, 0.08f, 0.09f), C_IRON);
        SubBox(go.transform, "Leg_BR", new Vector3( ldx, 0.04f,  ldz), new Vector3(0.09f, 0.08f, 0.09f), C_IRON);

        // ── 후드 (연기 흡입) ─────────────────────────────────────
        // 후드 본체 (그릴 위 0.3부터 시작)
        float hoodBotY = GRILL_Y + 0.30f;
        float hoodTopY = GRILL_Y + 0.82f;
        SubBox(go.transform, "Hood_Bot",
            new Vector3(0f, hoodBotY, 0f),
            new Vector3(GW + 0.12f, 0.28f, GD + 0.12f), C_HOOD);
        SubBox(go.transform, "Hood_Top",
            new Vector3(0f, hoodTopY, 0f),
            new Vector3(GW - 0.22f, 0.22f, GD - 0.22f), C_HOOD);
        // 덕트 (원통형 연통)
        SubCyl(go.transform, "Duct",
            new Vector3(0f, hoodTopY + 0.32f, 0f),
            new Vector3(0.18f, 0.30f, 0.18f), C_STEEL_DRK);
        // 환기 슬릿 (후드 하단 앞면)
        for (int si = 0; si < 4; si++)
        {
            float sx = -0.30f + si * 0.20f;
            SubBox(go.transform, $"Slit{si}",
                new Vector3(sx, hoodBotY - 0.08f, -(GD*0.5f + 0.07f)),
                new Vector3(0.04f, 0.13f, 0.04f), C_STEEL);
        }
        // 후드 하단 조명 (조리면 밝히는 노란빛)
        SubBox(go.transform, "HoodLight",
            new Vector3(0f, hoodBotY - 0.12f, 0f),
            new Vector3(GW - 0.20f, 0.018f, 0.18f), C_HANJI);

        BuildLabel(go.transform, "불판", hoodTopY + 0.55f);

        var gs = go.AddComponent<GrillStation>();
        gs.grillTime     = 15f;
        gs.collectWindow = 8f;
        go.AddComponent<InteractionBubble>();
        EditorUtility.SetDirty(go);
    }

    // ═════════════════════════════════════════════════════════════
    //  4. 조리대 (CookStation)
    //
    //   스테인리스 상판 + 한옥 하부 구조.
    //   [E] → 재료 확인 → 자동 5초(준비) → 자동 8초(조립) → [E] 소스 → [E] 완성
    //
    //   ┌────────────────────────────┐  ← 스테인리스 상판 + 테두리
    //   │  [도마]  [재료볼] [재료볼] │  ← 상판 소품
    //   ├──────┬─────────────────────┤  ← 단청 수직 구분선
    //   │      │                     │  ← 참나무 본체
    //   │[──]  │  [─────] [──────]   │  ← 서랍 손잡이
    //   └──────┴─────────────────────┘
    //   ████                      ████
    // ═════════════════════════════════════════════════════════════
    static void BuildCookStation(Transform parent)
    {
        var go = new GameObject("CookStation");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = POS_COOK;

        const float CW = 1.30f, CD = 0.70f, TOP_Y = 0.88f;

        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(CW, 0.95f, CD);
        col.center = new Vector3(0f, 0.475f, 0f);

        // ── 스테인리스 상판 ──────────────────────────────────────
        SubBox(go.transform, "Top",
            new Vector3(0f, TOP_Y, 0f), new Vector3(CW, 0.062f, CD), C_STEEL);
        // 테두리 (앞/좌/우 3면)
        SubBox(go.transform, "Edge_F",
            new Vector3(0f, TOP_Y - 0.02f, -(CD*0.5f + 0.012f)),
            new Vector3(CW, 0.062f, 0.022f), C_STEEL_DRK);
        SubBox(go.transform, "Edge_L",
            new Vector3(-(CW*0.5f + 0.012f), TOP_Y - 0.02f, 0f),
            new Vector3(0.022f, 0.062f, CD), C_STEEL_DRK);
        SubBox(go.transform, "Edge_R",
            new Vector3(  CW*0.5f + 0.012f,  TOP_Y - 0.02f, 0f),
            new Vector3(0.022f, 0.062f, CD), C_STEEL_DRK);

        // ── 본체 (참나무) ────────────────────────────────────────
        SubBox(go.transform, "Body",
            new Vector3(0f, 0.43f, 0f), new Vector3(CW - 0.04f, 0.82f, CD - 0.04f), C_OAK);
        SubBox(go.transform, "Front",
            new Vector3(0f, 0.43f, -(CD*0.5f - 0.012f)),
            new Vector3(CW - 0.08f, 0.79f, 0.04f), C_PINE);
        // 단청 수직 구분선 (한옥 창살 패턴 암시)
        SubBox(go.transform, "Div_V",
            new Vector3(0.08f, 0.43f, -(CD*0.5f)),
            new Vector3(0.025f, 0.79f, 0.02f), C_DANCHEONG);

        // ── 서랍 손잡이 ─────────────────────────────────────────
        SubBox(go.transform, "Handle_L",
            new Vector3(-0.45f, 0.52f, -(CD*0.5f + 0.03f)),
            new Vector3(0.22f, 0.032f, 0.04f), C_STEEL_DRK);
        SubBox(go.transform, "Handle_RM",
            new Vector3( 0.32f, 0.68f, -(CD*0.5f + 0.03f)),
            new Vector3(0.20f, 0.032f, 0.04f), C_STEEL_DRK);
        SubBox(go.transform, "Handle_RB",
            new Vector3( 0.32f, 0.42f, -(CD*0.5f + 0.03f)),
            new Vector3(0.20f, 0.032f, 0.04f), C_STEEL_DRK);

        // ── 다리 4개 ─────────────────────────────────────────────
        float lx = CW*0.5f - 0.06f, lz = CD*0.5f - 0.06f;
        SubBox(go.transform, "Leg_FL", new Vector3(-lx, 0.04f, -lz), new Vector3(0.08f, 0.08f, 0.08f), C_BEAM);
        SubBox(go.transform, "Leg_FR", new Vector3( lx, 0.04f, -lz), new Vector3(0.08f, 0.08f, 0.08f), C_BEAM);
        SubBox(go.transform, "Leg_BL", new Vector3(-lx, 0.04f,  lz), new Vector3(0.08f, 0.08f, 0.08f), C_BEAM);
        SubBox(go.transform, "Leg_BR", new Vector3( lx, 0.04f,  lz), new Vector3(0.08f, 0.08f, 0.08f), C_BEAM);

        // ── 상판 소품 ────────────────────────────────────────────
        // 도마 (나무색 사각형)
        SubBox(go.transform, "CutBoard",
            new Vector3(-0.30f, TOP_Y + 0.025f, 0.06f),
            new Vector3(0.46f, 0.022f, 0.33f), new Color(0.78f, 0.62f, 0.38f));
        // 재료 볼 2개 (스테인리스 납작 원통)
        SubCyl(go.transform, "Bowl_L",
            new Vector3(0.28f, TOP_Y + 0.06f, 0.10f),
            new Vector3(0.19f, 0.03f, 0.19f), C_STEEL);
        SubCyl(go.transform, "Bowl_R",
            new Vector3(0.50f, TOP_Y + 0.055f, 0.10f),
            new Vector3(0.13f, 0.025f, 0.13f), C_STEEL);
        // 도마 위 재료 표시 (빵/양상추/토마토)
        SubBox(go.transform, "Ing_Bun",
            new Vector3(-0.42f, TOP_Y + 0.055f, 0.02f),
            new Vector3(0.09f, 0.050f, 0.09f), new Color(0.95f, 0.85f, 0.60f));
        SubBox(go.transform, "Ing_Let",
            new Vector3(-0.25f, TOP_Y + 0.050f, 0.02f),
            new Vector3(0.09f, 0.040f, 0.09f), new Color(0.22f, 0.72f, 0.18f));
        SubBox(go.transform, "Ing_Tom",
            new Vector3(-0.08f, TOP_Y + 0.055f, 0.02f),
            new Vector3(0.07f, 0.068f, 0.07f), new Color(0.88f, 0.18f, 0.10f));

        BuildLabel(go.transform, "조리대", TOP_Y + 0.55f);

        var cs = go.AddComponent<CookStation>();
        cs.prepareTime  = 5f;
        cs.assembleTime = 8f;
        go.AddComponent<InteractionBubble>();
        EditorUtility.SetDirty(go);
    }

    // ═════════════════════════════════════════════════════════════
    //  5. 서빙 카운터 (ServeCounter)
    //
    //   완성 버거를 들고 [E] → 버거 카운트 +1.
    //   한옥 카운터 + 서빙 창구 + 주문 번호 디스플레이.
    //
    //                  ╔══════════╗
    //                  ║  🍔      ║  ← 서빙 창구 (뒷면 상단)
    //   ┌──────────────╚══════════╝──┐  ← 밝은 나무 상판
    //   │  [버거 모형]   [디스플레이] │  ← 상판 소품
    //   ├────────────────────────────┤  ← 단청 상단 몰딩
    //   │           서빙 카운터      │  ← 참나무 본체
    //   └────────────────────────────┘
    //   ████                      ████
    // ═════════════════════════════════════════════════════════════
    static void BuildServeCounter(Transform parent)
    {
        var go = new GameObject("ServeCounter");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = POS_SERVE;

        const float SW = 1.20f, SD = 0.65f, TOP_Y = 0.90f;

        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(SW, 1.0f, SD);
        col.center = new Vector3(0f, 0.50f, 0f);

        // ── 상판 (밝은 나무) ─────────────────────────────────────
        SubBox(go.transform, "Top",
            new Vector3(0f, TOP_Y, 0f), new Vector3(SW, 0.065f, SD), C_SERVE);
        SubBox(go.transform, "TopEdge_F",
            new Vector3(0f, TOP_Y - 0.01f, -(SD*0.5f + 0.01f)),
            new Vector3(SW, 0.065f, 0.025f), C_DANCHEONG);

        // ── 본체 ─────────────────────────────────────────────────
        SubBox(go.transform, "Body",
            new Vector3(0f, 0.44f, 0f), new Vector3(SW - 0.04f, 0.84f, SD - 0.04f), C_OAK);
        SubBox(go.transform, "Front",
            new Vector3(0f, 0.44f, -(SD*0.5f - 0.012f)),
            new Vector3(SW - 0.08f, 0.80f, 0.04f), C_PINE);
        // 단청 상단 몰딩
        SubBox(go.transform, "Molding",
            new Vector3(0f, TOP_Y - 0.10f, -(SD*0.5f)),
            new Vector3(SW - 0.04f, 0.04f, 0.022f), C_DANCHEONG);

        // ── 서빙 창구 (뒷면 상단) ───────────────────────────────
        // 왜 뒷면? 서빙은 반대편(손님 쪽)으로 내미는 구조
        float wW = 0.54f, wH = 0.34f;
        float wCY = TOP_Y + wH * 0.5f + 0.02f;
        // 창구 좌우 기둥 + 상단 보
        SubBox(go.transform, "Win_L",
            new Vector3(-(wW*0.5f + 0.04f), wCY, SD*0.5f + 0.01f),
            new Vector3(0.06f, wH + 0.09f, 0.04f), C_OAK);
        SubBox(go.transform, "Win_R",
            new Vector3(  wW*0.5f + 0.04f,  wCY, SD*0.5f + 0.01f),
            new Vector3(0.06f, wH + 0.09f, 0.04f), C_OAK);
        SubBox(go.transform, "Win_Top",
            new Vector3(0f, wCY + wH*0.5f, SD*0.5f + 0.01f),
            new Vector3(wW + 0.16f, 0.062f, 0.04f), C_BEAM);
        SubBox(go.transform, "Win_Sill",
            new Vector3(0f, TOP_Y + 0.032f, SD*0.5f + 0.05f),
            new Vector3(wW + 0.16f, 0.04f, 0.09f), C_PINE);

        // ── 다리 4개 ─────────────────────────────────────────────
        float lx = SW*0.5f - 0.06f, lz = SD*0.5f - 0.06f;
        SubBox(go.transform, "Leg_FL", new Vector3(-lx, 0.04f, -lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);
        SubBox(go.transform, "Leg_FR", new Vector3( lx, 0.04f, -lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);
        SubBox(go.transform, "Leg_BL", new Vector3(-lx, 0.04f,  lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);
        SubBox(go.transform, "Leg_BR", new Vector3( lx, 0.04f,  lz), new Vector3(0.09f, 0.08f, 0.09f), C_BEAM);

        // ── 상판 소품 ────────────────────────────────────────────
        // 미니 버거 모형 (완성된 버거를 표시)
        SubBox(go.transform, "Burg_BotBun",
            new Vector3(-0.22f, TOP_Y + 0.040f, 0.00f),
            new Vector3(0.17f, 0.055f, 0.17f), new Color(0.90f, 0.72f, 0.38f));
        SubBox(go.transform, "Burg_Patty",
            new Vector3(-0.22f, TOP_Y + 0.090f, 0.00f),
            new Vector3(0.16f, 0.038f, 0.16f), new Color(0.40f, 0.22f, 0.08f));
        SubBox(go.transform, "Burg_Let",
            new Vector3(-0.22f, TOP_Y + 0.124f, 0.00f),
            new Vector3(0.18f, 0.022f, 0.18f), new Color(0.25f, 0.70f, 0.20f));
        SubBox(go.transform, "Burg_TopBun",
            new Vector3(-0.22f, TOP_Y + 0.162f, 0.00f),
            new Vector3(0.17f, 0.090f, 0.17f), new Color(0.95f, 0.80f, 0.46f));

        // 주문 번호 디스플레이 (녹색 LED 패널)
        SubBox(go.transform, "Display",
            new Vector3(0.36f, TOP_Y + 0.16f, -(SD*0.5f + 0.02f)),
            new Vector3(0.18f, 0.20f, 0.03f), new Color(0.10f, 0.14f, 0.20f));
        SubBox(go.transform, "Display_Glow",
            new Vector3(0.36f, TOP_Y + 0.16f, -(SD*0.5f + 0.03f)),
            new Vector3(0.13f, 0.14f, 0.01f), new Color(0.22f, 0.78f, 0.34f));

        BuildLabel(go.transform, "서빙 카운터", TOP_Y + 0.60f);

        go.AddComponent<ServeCounter>();
        go.AddComponent<InteractionBubble>();
        EditorUtility.SetDirty(go);
    }

    // ═════════════════════════════════════════════════════════════
    //  공통: 라벨 (FarmStationBuilder 와 동일 구조)
    // ═════════════════════════════════════════════════════════════
    static void BuildLabel(Transform parent, string text, float y)
    {
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(parent, false);
        labelGO.transform.localPosition = new Vector3(0f, y, 0f);
        labelGO.transform.localScale    = new Vector3(0.012f, 0.012f, 0.012f);

        var canvas = labelGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 5;
        labelGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 60f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(labelGO.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin        = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.sizeDelta        = new Vector2(200f, 60f);
        trt.anchoredPosition = Vector2.zero;

        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 24;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = new Color(1f, 0.95f, 0.75f); // 한지 호박빛 흰색
        tmp.fontStyle = TMPro.FontStyles.Bold;
    }

    // ═════════════════════════════════════════════════════════════
    //  헬퍼 (FarmStationBuilder 와 동일 패턴)
    //
    //  모든 자식 오브젝트는 콜라이더를 즉시 제거.
    //  → 루트의 BoxCollider 하나만 상호작용에 사용.
    //  → 데코 메시 충돌 방지.
    // ═════════════════════════════════════════════════════════════
    static void SubBox(Transform p, string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(p, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        ApplyColor(go, color);
    }

    // Cylinder: local Y 범위 [-1,1] → 시각 높이 = scaleY × 2
    static void SubCyl(Transform p, string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(p, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
        ApplyColor(go, color);
    }

    // Sphere: local 반지름 0.5 → 시각 반지름 = scale × 0.5
    static void SubSph(Transform p, string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(p, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        Object.DestroyImmediate(go.GetComponent<SphereCollider>());
        ApplyColor(go, color);
    }

    static void ApplyColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) return;
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color",     color);
        rend.sharedMaterial = mat;
    }
}
