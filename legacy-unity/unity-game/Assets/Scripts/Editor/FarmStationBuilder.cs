using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 한옥 테마 3층 실내 재배기 + 항아리 배치
///
/// ═══════════════════════════════
///  디자인 컨셉: 현대 스마트팜 + 한옥 전통 목재
/// ═══════════════════════════════
///
///   ┌──────────────────────┐  ← 처마 (상단 캐노피, 짙은 대들보색)
///   │  ══════════════════  │  ← 한지 조명 (따뜻한 호박색)
///   │  ┌────┬────┬────┐   │  ← 3층: 창살 격자 + 참나무 트레이
///   │  │❚❚❚❚│❚❚❚❚│❚❚❚❚│   │
///   │  └────┴────┴────┘   │
///   │  ══════════════════  │  ← 한지 조명
///   │  ┌────┬────┬────┐   │  ← 2층
///   │  └────┴────┴────┘   │
///   │  ══════════════════  │  ← 한지 조명
///   │  ┌────┬────┬────┐   │  ← 1층
///   │  └────┴────┴────┘   │
///   ⊙   소나무 기둥 4개   ⊙  ← 바퀴 (철재)
///
///  곁에: 항아리 + 대나무 국자
///
/// 색상 팔레트:
///   소나무   #BF8B47  기둥, 창살
///   참나무   #80521F  트레이 외관
///   대들보   #402809  상단 캐노피, 지지바
///   단청빨강 #B82614  포인트 (트레이 테두리)
///   한지조명 #FFE085  LED 발광면
///   철재     #3A2E24  바퀴
///   점토     #8C5229  항아리
/// </summary>
public static class FarmStationBuilder
{
    // ── 치수 ─────────────────────────────────────────────────────
    const float CART_W = 1.20f;
    const float CART_D = 0.65f;
    const float TRAY_H = 0.14f;

    static readonly float[] TIER_Y  = { 0.16f, 0.50f, 0.84f };

    const float POLE_X  = 0.55f;
    const float POLE_Z  = 0.29f;
    const float POLE_CY = 0.59f;
    const float POLE_H  = 1.18f;

    // ── 한옥 색상 팔레트 ──────────────────────────────────────────
    // 왜 이 색들?
    //   소나무(松): 전통 한옥 기둥 재료 — 밝고 따뜻한 황갈색
    //   참나무(樫): 무거운 가구 재료 — 중간 갈색
    //   대들보(大樑): 한옥 구조재 — 짙게 그을린 어두운 갈색
    //   단청(丹靑): 전통 채색 포인트 — 붉은색
    //   한지(韓紙): 창호지를 통해 새는 따뜻한 빛 — 호박빛 노랑
    static readonly Color C_PINE      = new Color(0.75f, 0.55f, 0.28f); // 소나무 기둥
    static readonly Color C_OAK       = new Color(0.50f, 0.32f, 0.14f); // 참나무 트레이
    static readonly Color C_BEAM      = new Color(0.25f, 0.16f, 0.06f); // 대들보 (어두운)
    static readonly Color C_DANCHEONG = new Color(0.72f, 0.15f, 0.08f); // 단청 빨강 포인트
    static readonly Color C_HANJI     = new Color(1.00f, 0.88f, 0.52f); // 한지 조명
    static readonly Color C_IRON      = new Color(0.22f, 0.18f, 0.14f); // 철재 바퀴
    static readonly Color C_SOIL      = new Color(0.20f, 0.13f, 0.05f); // 흙
    static readonly Color C_HERB1     = new Color(0.12f, 0.42f, 0.08f); // 1층 허브 (짙은)
    static readonly Color C_HERB2     = new Color(0.22f, 0.60f, 0.16f); // 2층 잎채소
    static readonly Color C_TOMATO    = new Color(0.85f, 0.20f, 0.12f); // 토마토
    static readonly Color C_LETTUCE   = new Color(0.26f, 0.74f, 0.22f); // 양상추
    static readonly Color C_CLAY      = new Color(0.55f, 0.30f, 0.16f); // 항아리 점토
    static readonly Color C_CLAY_DARK = new Color(0.36f, 0.18f, 0.08f); // 항아리 짙은부분
    static readonly Color C_BAMBOO    = new Color(0.68f, 0.60f, 0.25f); // 대나무

    // ── 배치 좌표 ─────────────────────────────────────────────────
    static readonly Vector3 POS_TOMATO   = new Vector3(-3.8f, 0f, 1.0f);
    static readonly Vector3 POS_LETTUCE  = new Vector3(-3.8f, 0f, 2.5f);
    static readonly Vector3 POS_WATERING_CAN = new Vector3(-3.34f, 0f, -0.5f);

    // ─────────────────────────────────────────────────────────────
    [MenuItem("Tools/100 Burger Family/Build FarmStations")]
    public static void Build() => BuildInternal();

    [MenuItem("Tools/100 Burger Family/Build FarmStations Force")]
    public static void BuildForce() => BuildInternal();

    static void BuildInternal()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[FarmStationBuilder] Stop 후 Edit 모드에서 실행하세요.");
            return;
        }
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene == null)
        {
            Debug.LogError("[FarmStationBuilder] RoomScene 오브젝트를 찾을 수 없습니다.");
            return;
        }

        foreach (string n in new[] { "TomatoFarm", "LettuceFarm", "WateringJar", "WateringCan" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        CreateFarmStation(roomScene.transform, "TomatoFarm",  POS_TOMATO,
                          C_TOMATO,  IngredientType.Tomato,  "토마토 재배기");
        CreateFarmStation(roomScene.transform, "LettuceFarm", POS_LETTUCE,
                          C_LETTUCE, IngredientType.Lettuce, "양배추 재배기");
        var wateringCan = CreateWateringCan(roomScene.transform, POS_WATERING_CAN);
        wateringCan.AddComponent<WateringCanStation>();
        wateringCan.AddComponent<InteractionBubble>();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FarmStationBuilder] ✅ 한옥 테마 재배기 2개 + 항아리 배치 완료!");
    }

    // ══════════════════════════════════════════════════════════════
    //  재배기 생성
    // ══════════════════════════════════════════════════════════════
    static void CreateFarmStation(Transform parent, string objName,
                                  Vector3 pos, Color cropColor,
                                  IngredientType cropType, string label)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = Vector3.one;

        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(CART_W, 1.15f, CART_D);
        col.center = new Vector3(0f, 0.57f, 0f);

        BuildFrame(go.transform);
        BuildTiers(go.transform, cropColor);
        BuildTopCanopy(go.transform);
        BuildLabel(go.transform, label);

        var farm = go.AddComponent<FarmStation>();
        farm.cropType = cropType;
        go.AddComponent<FarmInteractionBubble>();

        EditorUtility.SetDirty(go);
    }

    // ── 프레임: 기둥 + 바퀴 + 가로 지지바 ────────────────────────
    static void BuildFrame(Transform t)
    {
        // 바퀴 4개 — 철재 (CapsuleCollider 제거 필요)
        // Cylinder: 높이 방향 Y, local Y범위 [-1,1]
        // 시각 높이 h → scaleY = h/2
        float wx = 0.50f, wz = 0.27f;
        SubCyl(t, "Wheel_FL", new Vector3(-wx, 0.04f, -wz), new Vector3(0.10f, 0.04f, 0.10f), C_IRON);
        SubCyl(t, "Wheel_FR", new Vector3( wx, 0.04f, -wz), new Vector3(0.10f, 0.04f, 0.10f), C_IRON);
        SubCyl(t, "Wheel_BL", new Vector3(-wx, 0.04f,  wz), new Vector3(0.10f, 0.04f, 0.10f), C_IRON);
        SubCyl(t, "Wheel_BR", new Vector3( wx, 0.04f,  wz), new Vector3(0.10f, 0.04f, 0.10f), C_IRON);

        // 바퀴 허브 (밝은 철재)
        Color hubCol = new Color(0.38f, 0.32f, 0.28f);
        SubSph(t, "Hub_FL", new Vector3(-wx, 0.04f, -wz), new Vector3(0.05f, 0.05f, 0.05f), hubCol);
        SubSph(t, "Hub_FR", new Vector3( wx, 0.04f, -wz), new Vector3(0.05f, 0.05f, 0.05f), hubCol);
        SubSph(t, "Hub_BL", new Vector3(-wx, 0.04f,  wz), new Vector3(0.05f, 0.05f, 0.05f), hubCol);
        SubSph(t, "Hub_BR", new Vector3( wx, 0.04f,  wz), new Vector3(0.05f, 0.05f, 0.05f), hubCol);

        // 수직 기둥 4개 — 소나무색
        SubBox(t, "Pole_FL", new Vector3(-POLE_X, POLE_CY, -POLE_Z), new Vector3(0.06f, POLE_H, 0.06f), C_PINE);
        SubBox(t, "Pole_FR", new Vector3( POLE_X, POLE_CY, -POLE_Z), new Vector3(0.06f, POLE_H, 0.06f), C_PINE);
        SubBox(t, "Pole_BL", new Vector3(-POLE_X, POLE_CY,  POLE_Z), new Vector3(0.06f, POLE_H, 0.06f), C_PINE);
        SubBox(t, "Pole_BR", new Vector3( POLE_X, POLE_CY,  POLE_Z), new Vector3(0.06f, POLE_H, 0.06f), C_PINE);

        // 기둥 조임쇠 (각 층 높이에서 기둥을 감싸는 어두운 링)
        // 한옥의 주두(柱頭) 느낌 — 기둥과 대들보 연결부
        foreach (float ty in TIER_Y)
        {
            float ringY = ty - TRAY_H * 0.5f - 0.04f;
            float[] bx2 = { -POLE_X, POLE_X, -POLE_X,  POLE_X };
            float[] bz2 = { -POLE_Z, -POLE_Z, POLE_Z,  POLE_Z };
            for (int i = 0; i < 4; i++)
                SubBox(t, "Ring", new Vector3(bx2[i], ringY, bz2[i]),
                    new Vector3(0.08f, 0.04f, 0.08f), C_BEAM);
        }

        // 가로 지지바 — 대들보색 (각 층 아래)
        foreach (float ty in TIER_Y)
        {
            float barY = ty - TRAY_H * 0.5f - 0.02f;
            SubBox(t, "Bar_F", new Vector3(0f, barY, -POLE_Z), new Vector3(CART_W + 0.04f, 0.05f, 0.05f), C_BEAM);
            SubBox(t, "Bar_B", new Vector3(0f, barY,  POLE_Z), new Vector3(CART_W + 0.04f, 0.05f, 0.05f), C_BEAM);
            SubBox(t, "Bar_L", new Vector3(-POLE_X, barY, 0f),  new Vector3(0.05f, 0.05f, CART_D + 0.04f), C_BEAM);
            SubBox(t, "Bar_R", new Vector3( POLE_X, barY, 0f),  new Vector3(0.05f, 0.05f, CART_D + 0.04f), C_BEAM);
        }
    }

    // ── 3층 트레이 + 창살 + 한지조명 + 식물 ──────────────────────
    static void BuildTiers(Transform t, Color cropColor)
    {
        Color[] plantCol = { C_HERB1, C_HERB2, cropColor };
        float[] plantH   = { 0.11f,   0.18f,   0.27f   }; // 1층 낮게, 3층 높게
        float[] plantW   = { 0.18f,   0.21f,   0.08f   }; // 3층은 파/양파처럼 얇게

        for (int i = 0; i < 3; i++)
        {
            float ty  = TIER_Y[i];
            string tn = $"Tier{i + 1}";

            // ─ 트레이 외부 (참나무색) ─────────────────────────────
            SubBox(t, $"{tn}_Tray",
                new Vector3(0f, ty, 0f),
                new Vector3(CART_W, TRAY_H, CART_D),
                C_OAK);

            // ─ 단청 포인트: 트레이 앞면 테두리 (붉은 옻칠 느낌) ──
            // 왜? 한옥에서 단청은 목재 보호 + 장식 기능
            SubBox(t, $"{tn}_Front",
                new Vector3(0f, ty, -(CART_D * 0.5f + 0.01f)),
                new Vector3(CART_W, TRAY_H, 0.03f),
                C_DANCHEONG);

            // ─ 창살(窓欞): 앞면에 세로 슬랫 5개 ──────────────────
            // 왜? 한옥 창호의 세로 살 패턴을 재현
            //     나무 트레이를 꾸미며 통기성도 있어 보임
            float[] slatX = { -0.44f, -0.22f, 0f, 0.22f, 0.44f };
            foreach (float sx in slatX)
                SubBox(t, $"{tn}_Slat",
                    new Vector3(sx, ty, -(CART_D * 0.5f + 0.02f)),
                    new Vector3(0.04f, TRAY_H * 0.72f, 0.04f),
                    C_PINE);

            // ─ 흙 ─────────────────────────────────────────────────
            float soilY = ty + TRAY_H * 0.22f;
            SubBox(t, $"{tn}_Soil",
                new Vector3(0f, soilY, 0f),
                new Vector3(CART_W - 0.14f, TRAY_H * 0.44f, CART_D - 0.12f),
                C_SOIL);

            // ─ 한지 조명 (각 층 위) ───────────────────────────────
            // 왜 위쪽? 식물은 위에서 내리쬐는 빛으로 자람
            // 한지 색조(#FFE085)로 전통 램프 느낌 연출
            float ledY = ty + TRAY_H * 0.5f + 0.07f;
            // LED 하우징 (대들보색 — 목재 프레임)
            SubBox(t, $"{tn}_LedHousing",
                new Vector3(0f, ledY, 0f),
                new Vector3(CART_W - 0.04f, 0.06f, CART_D - 0.04f),
                C_BEAM);
            // LED 발광면 (한지 호박색)
            SubBox(t, $"{tn}_LedStrip",
                new Vector3(0f, ledY - 0.04f, 0f),
                new Vector3(CART_W - 0.16f, 0.02f, 0.14f),
                C_HANJI);

            // ─ 식물 4×2 그리드 ────────────────────────────────────
            float soilTop  = soilY + TRAY_H * 0.22f;
            float pCenterY = soilTop + plantH[i] * 0.5f;

            float[] xs = { -0.44f, -0.15f, 0.15f, 0.44f };
            float[] zs = { -0.13f, 0.13f };
            int idx = 0;

            foreach (float px in xs)
            {
                foreach (float pz in zs)
                {
                    float j = ((idx * 13 + i * 7) % 9 - 4) * 0.012f;
                    SubBox(t, $"{tn}_Plant",
                        new Vector3(px + j, pCenterY, pz + j * 0.4f),
                        new Vector3(plantW[i], plantH[i], plantW[i]),
                        plantCol[i]);
                    idx++;
                }
            }

            // 3층은 윗잎 추가 (풍성하게)
            if (i == 2)
            {
                foreach (float px in xs)
                {
                    float j2 = ((int)(px * 100) % 5 - 2) * 0.01f;
                    SubBox(t, $"{tn}_PlantLeaf",
                        new Vector3(px + j2, pCenterY + plantH[i] * 0.38f, j2 * 0.5f),
                        new Vector3(plantW[i] * 1.5f, plantH[i] * 0.22f, plantW[i] * 1.5f),
                        cropColor * 1.15f);
                }
            }
        }
    }

    // ── 처마 (上端 캐노피) ──────────────────────────────────────────
    // 한옥의 처마(軒): 지붕이 벽보다 넓게 돌출되어 비와 햇빛을 차단
    // 여기서는 상단 프레임이 기둥보다 약간 바깥으로 튀어나옴
    static void BuildTopCanopy(Transform t)
    {
        float topY = POLE_CY + POLE_H * 0.5f;

        // 상단 4방향 대들보
        SubBox(t, "Canopy_F",
            new Vector3(0f, topY, -POLE_Z),
            new Vector3(CART_W + 0.16f, 0.08f, 0.08f),
            C_BEAM);
        SubBox(t, "Canopy_B",
            new Vector3(0f, topY, POLE_Z),
            new Vector3(CART_W + 0.16f, 0.08f, 0.08f),
            C_BEAM);
        SubBox(t, "Canopy_L",
            new Vector3(-POLE_X, topY, 0f),
            new Vector3(0.08f, 0.08f, CART_D + 0.16f),
            C_BEAM);
        SubBox(t, "Canopy_R",
            new Vector3( POLE_X, topY, 0f),
            new Vector3(0.08f, 0.08f, CART_D + 0.16f),
            C_BEAM);

        // 처마 윗판 (기둥보다 넓게 돌출 — 처마 느낌)
        SubBox(t, "Canopy_Cap",
            new Vector3(0f, topY + 0.06f, 0f),
            new Vector3(CART_W + 0.24f, 0.06f, CART_D + 0.24f),
            C_BEAM);

        // 처마 상단 한지 조명 (전체 조명)
        SubBox(t, "Canopy_LED",
            new Vector3(0f, topY - 0.05f, 0f),
            new Vector3(CART_W - 0.10f, 0.03f, 0.16f),
            C_HANJI);
    }

    // ══════════════════════════════════════════════════════════════
    //  항아리 + 대나무 국자
    //
    //  한옥 전통 물그릇: 옹기 항아리 (甕器)
    //  넓은 배, 좁은 목, 입술이 있는 형태
    //  곁에 대나무 국자 (박 바가지 대신 대나무 손잡이)
    //
    //  배치: 재배기 앞쪽 동선에 둔다.
    //  왜 재배기 사이가 아닌 앞쪽인가?
    //    플레이어가 물 도구를 집은 뒤 토마토/양배추 재배기로 이동하는 동선이
    //    분명해야 "물 주기"가 별도 플레이 행동으로 느껴진다.
    // ══════════════════════════════════════════════════════════════
    static GameObject CreateWateringCan(Transform parent, Vector3 pos)
    {
        var go = new GameObject("WateringCan");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = Vector3.one;

        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(0.9f, 0.9f, 1.15f);
        col.center = new Vector3(0.08f, 0.32f, 0.05f);

        // 항아리 몸통 (배가 볼록한 구형)
        SubSph(go.transform, "Jar_Body",
            new Vector3(0f, 0.22f, 0f),
            new Vector3(0.30f, 0.35f, 0.28f),
            C_CLAY);

        // 항아리 배 무늬 (살짝 어두운 띠 — 전통 옹기 문양)
        SubCyl(go.transform, "Jar_Band",
            new Vector3(0f, 0.22f, 0f),
            new Vector3(0.31f, 0.025f, 0.31f), // 얇고 넓은 띠
            C_CLAY_DARK);

        // 항아리 목 (좁은 원통형)
        SubCyl(go.transform, "Jar_Neck",
            new Vector3(0f, 0.375f, 0f),
            new Vector3(0.13f, 0.038f, 0.13f), // h = 0.076
            C_CLAY_DARK);

        // 항아리 입술 (rim — 두꺼운 테두리)
        SubCyl(go.transform, "Jar_Rim",
            new Vector3(0f, 0.425f, 0f),
            new Vector3(0.16f, 0.020f, 0.16f), // h = 0.04
            C_CLAY_DARK);

        // 항아리 바닥 (안정적인 받침)
        SubCyl(go.transform, "Jar_Base",
            new Vector3(0f, 0.04f, 0f),
            new Vector3(0.18f, 0.04f, 0.18f),
            C_CLAY_DARK);

        // 항아리 안쪽 (물 색상 — 짙은 파란색)
        SubCyl(go.transform, "Jar_Water",
            new Vector3(0f, 0.41f, 0f),
            new Vector3(0.11f, 0.015f, 0.11f),
            new Color(0.25f, 0.45f, 0.65f));

        // ── 대나무 국자 ────────────────────────────────────────────
        // 전통 한옥에서 항아리 물을 뜨는 도구
        // 손잡이: 대나무 줄기 (황록색), 컵: 작은 바가지 (대나무색)

        // 손잡이 (대나무 줄기 — 항아리에 기대어 있는 각도)
        SubBox(go.transform, "Dipper_Handle",
            new Vector3(0.25f, 0.38f, 0.05f),
            new Vector3(0.38f, 0.04f, 0.04f),
            C_BAMBOO);

        // 손잡이 마디 (대나무 마디 표현 — 어두운 링 2개)
        SubBox(go.transform, "Dipper_Node1",
            new Vector3(0.10f, 0.38f, 0.05f),
            new Vector3(0.03f, 0.055f, 0.055f),
            new Color(0.48f, 0.40f, 0.14f));
        SubBox(go.transform, "Dipper_Node2",
            new Vector3(0.30f, 0.38f, 0.05f),
            new Vector3(0.03f, 0.055f, 0.055f),
            new Color(0.48f, 0.40f, 0.14f));

        // 컵 (바가지 — 반구 느낌)
        SubSph(go.transform, "Dipper_Cup",
            new Vector3(0.46f, 0.35f, 0.05f),
            new Vector3(0.10f, 0.08f, 0.10f),
            C_BAMBOO);

        // 컵 안쪽 물방울
        SubSph(go.transform, "Dipper_Water",
            new Vector3(0.46f, 0.36f, 0.05f),
            new Vector3(0.07f, 0.04f, 0.07f),
            new Color(0.35f, 0.60f, 0.80f));

        // 국자 지지대 (항아리 입술에 걸쳐진 부분 — 어두운 띠)
        SubBox(go.transform, "Dipper_Rest",
            new Vector3(0.06f, 0.43f, 0.05f),
            new Vector3(0.06f, 0.02f, 0.06f),
            C_CLAY_DARK);

        EditorUtility.SetDirty(go);
        return go;
    }

    // ── 라벨 ─────────────────────────────────────────────────────
    static void BuildLabel(Transform parent, string text)
    {
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(parent, false);
        labelGO.transform.localPosition = new Vector3(0f, 1.32f, 0f);
        labelGO.transform.localScale    = new Vector3(0.012f, 0.012f, 0.012f);

        var canvas = labelGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 5;
        labelGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 60f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(labelGO.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.sizeDelta       = new Vector2(200f, 60f);
        trt.anchoredPosition = Vector2.zero;

        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 24;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = new Color(1f, 0.95f, 0.75f); // 한지 빛 노란빛 흰색
        tmp.fontStyle = TMPro.FontStyles.Bold;
    }

    // ══════════════════════════════════════════════════════════════
    //  헬퍼
    //
    //  세 가지 프리미티브 모두 콜라이더를 즉시 제거:
    //    → 루트의 BoxCollider 하나만 Interactable 감지에 사용
    //    → 데코 오브젝트의 물리 충돌 방지
    // ══════════════════════════════════════════════════════════════
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

    // Cylinder: local Y [-1,1] → 시각 높이 = scaleY × 2
    //           local XZ 반지름 0.5 → 시각 반지름 = scaleX × 0.5
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
