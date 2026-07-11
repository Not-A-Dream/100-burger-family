using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 주방 분위기 소품 빌더 — 한옥 부엌(廚) 분위기 완성
///
/// KitchenStationsBuilder 로 스테이션 배치 후 이걸 실행하면
/// 천장 구조 + 등불 + 매달린 채소 + 장독대 가 추가됩니다.
///
/// ═══════════════════════════════════════════════════════
///  추가되는 요소
/// ═══════════════════════════════════════════════════════
///
///  1. 천장 대들보 + 서까래
///     한옥은 항상 노출 목재 천장. 방 폭 전체에 걸쳐진 4개 대들보와
///     그 사이를 잇는 가는 서까래 6개. y = 2.5m.
///
///  2. 한지 등불 3개
///     대들보 아래에 매달린 전통 사각 등불.
///     소나무 틀 + 한지 발광면(C_HANJI) + 단청 술.
///
///  3. 매달린 마늘 + 고추 묶음
///     전통 부엌의 필수 소품. 대들보에서 줄로 매달린 형태.
///     마늘: 크림색 구체 클러스터
///     고추: 단청빨강 원통 클러스터
///
///  4. 뒷벽 장독대 (항아리 줄)
///     뒷벽(z=4.5) 앞에 대형·소형 항아리 8개.
///     전통 한옥의 장독대(醬獨臺) — 간장·된장·고추장 항아리.
///
/// ═══════════════════════════════════════════════════════
///  색상 팔레트 (FarmStationBuilder / KitchenStationsBuilder 동일)
/// ═══════════════════════════════════════════════════════
///   소나무 #BF8B47   참나무 #80521F   대들보 #402809
///   단청   #B82614   한지   #FFE085   철재   #3A2E24
/// </summary>
public static class KitchenAmbienceBuilder
{
    // ── 한옥 색상 팔레트 ──────────────────────────────────────────
    static readonly Color C_PINE      = new Color(0.75f, 0.55f, 0.28f);
    static readonly Color C_OAK       = new Color(0.50f, 0.32f, 0.14f);
    static readonly Color C_BEAM      = new Color(0.25f, 0.16f, 0.06f);
    static readonly Color C_DANCHEONG = new Color(0.72f, 0.15f, 0.08f);
    static readonly Color C_HANJI     = new Color(1.00f, 0.88f, 0.52f);
    static readonly Color C_IRON      = new Color(0.22f, 0.18f, 0.14f);

    // ── 소품 전용 색상 ───────────────────────────────────────────
    static readonly Color C_GARLIC    = new Color(0.94f, 0.90f, 0.78f); // 마늘 크림색
    static readonly Color C_GARLIC_SK = new Color(0.85f, 0.80f, 0.62f); // 마늘 껍질
    static readonly Color C_CHILI     = new Color(0.82f, 0.13f, 0.06f); // 고추 빨강
    static readonly Color C_CHILI_ST  = new Color(0.22f, 0.45f, 0.10f); // 고추 꼭지 초록
    static readonly Color C_ROPE      = new Color(0.55f, 0.42f, 0.22f); // 새끼줄
    static readonly Color C_CLAY      = new Color(0.55f, 0.30f, 0.16f); // 항아리 점토
    static readonly Color C_CLAY_DRK  = new Color(0.36f, 0.18f, 0.08f); // 항아리 짙은부분
    static readonly Color C_JAR_DARK  = new Color(0.28f, 0.14f, 0.06f); // 간장 항아리 (짙은)
    static readonly Color C_JAR_RED   = new Color(0.65f, 0.18f, 0.08f); // 고추장 항아리

    // ── 방 치수 (RoomRebuild 와 동일) ─────────────────────────────
    // 방 10×10, 벽 높이 3.5m
    // 대들보는 벽 바로 아래 y=2.5 에 배치
    const float ROOM_W    = 10f;
    const float ROOM_D    = 10f;
    const float CEIL_Y    = 2.50f;   // 대들보 중심 Y
    const float BEAM_W    = 0.14f;   // 대들보 단면 폭
    const float RAFTER_W  = 0.08f;   // 서까래 단면 폭

    // ─────────────────────────────────────────────────────────────
    [MenuItem("Tools/100 Burger Family/Build Kitchen Ambience")]
    public static void BuildAll()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[KitchenAmbienceBuilder] Stop 후 Edit 모드에서 실행하세요.");
            return;
        }
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene == null)
        {
            Debug.LogError("[KitchenAmbienceBuilder] RoomScene 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // ── 기존 소품 + 잔여 오브젝트 전부 제거 ──────────────────────
        // 이름으로 알려진 그룹 삭제
        foreach (string n in new[] { "KitchenCeiling", "HangingDecor", "JarRow" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        // 씬 전체에서 이름에 "Lantern", "Garlic", "Chili", "HangingDecor" 가 포함된
        // 루트 오브젝트를 추가 청소 — 이전 빌드에서 부모 없이 남은 잔여물 제거
        // (회색 기본 머티리얼로 렌더된 스트레이 등불 포함)
        var allRoots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in allRoots)
        {
            string nm = root.name;
            if (nm.Contains("Lantern") || nm.Contains("Garlic") ||
                nm.Contains("Chili")   || nm.Contains("HangingDecor"))
            {
                Object.DestroyImmediate(root);
            }
        }

        // ── 재건 ──────────────────────────────────────────────────
        BuildHangingDecor(roomScene.transform);
        BuildJarRow(roomScene.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[KitchenAmbienceBuilder] ✅ 등불(뒷벽3 + 왼쪽벽2) + 장독대 배치 완료!");
    }

    // ═════════════════════════════════════════════════════════════
    //  1. 천장 대들보 + 서까래 + 단청 장식
    //
    //  한옥 구조:
    //   - 대들보(大樑): 기둥 사이를 잇는 굵은 가로보. Z 방향으로 배치.
    //   - 서까래(椽): 대들보 위에 X 방향으로 얹는 가는 목재.
    //   - 도리(桃李): 대들보 끝을 받치는 방향과 수직인 보. X 방향.
    //
    //  배치:
    //     도리 2개  (X 방향) : z = -4.8, z = 4.8  (앞뒤 벽)
    //     대들보 5개 (Z 방향) : x = -4, -2, 0, 2, 4
    //     서까래 7개 (X 방향) : z = -3.5 ~ 3.5 간격 1.2
    //
    //  시각 다이어그램 (위에서 본 평면):
    //
    //    z=4.8 ══════════════════════════════════════  도리(앞)
    //          │  │  │  │  │  │  │  │  │  서까래
    //          ┃     ┃     ┃     ┃     ┃  대들보
    //          │  │  │  │  │  │  │  │  │  서까래
    //    z=-4.8 ══════════════════════════════════════  도리(뒤)
    // ═════════════════════════════════════════════════════════════
    static void BuildCeiling(Transform parent)
    {
        var root = new GameObject("KitchenCeiling");
        root.transform.SetParent(parent, false);

        // ── 대들보 5개 (Z 방향, 방 전체 길이) ────────────────────
        // 왜 5개? 기둥 간격 2.5m → 양쪽 2개 벽 기둥 + 중간 3개
        float[] beamX = { -4.0f, -2.0f, 0f, 2.0f, 4.0f };
        foreach (float bx in beamX)
        {
            SubBox(root.transform, "Beam_Z",
                new Vector3(bx, CEIL_Y, 0f),
                new Vector3(BEAM_W, BEAM_W, ROOM_D - 0.40f),
                C_BEAM);

            // 단청 포인트: 대들보 양 끝에 붉은 띠 (봉황·연꽃 무늬 대신 심플한 단청)
            // 왜? 한옥 대들보 끝(빰대머리)에는 반드시 단청을 칠함.
            //      게임 스케일에서는 얇은 빨간 링으로 표현.
            foreach (float dz in new[] { -(ROOM_D * 0.5f - 0.40f), (ROOM_D * 0.5f - 0.40f) })
            {
                SubBox(root.transform, "Beam_DancheongEnd",
                    new Vector3(bx, CEIL_Y, dz),
                    new Vector3(BEAM_W + 0.02f, BEAM_W + 0.02f, 0.10f),
                    C_DANCHEONG);
            }
        }

        // ── 도리 2개 (X 방향, 앞뒤 연결) ────────────────────────
        // 왜 도리? 대들보의 양 끝을 벽 방향으로 묶어주는 구조재.
        //          이게 없으면 대들보들이 개별적으로 떠 있는 것처럼 보임.
        foreach (float dz in new[] { -(ROOM_D * 0.5f - 0.25f), (ROOM_D * 0.5f - 0.25f) })
        {
            SubBox(root.transform, "Doori",
                new Vector3(0f, CEIL_Y, dz),
                new Vector3(ROOM_W - 0.40f, BEAM_W * 0.85f, BEAM_W * 0.85f),
                C_OAK);
        }

        // ── 서까래 7개 (X 방향, 대들보 사이) ────────────────────
        // 왜 가는 서까래? 대들보보다 얇아서 시각적 위계가 자연스러움.
        float[] rafterZ = { -3.5f, -2.2f, -0.9f, 0.4f, 1.7f, 3.0f, 4.3f };
        foreach (float rz in rafterZ)
        {
            SubBox(root.transform, "Rafter",
                new Vector3(0f, CEIL_Y + BEAM_W * 0.55f, rz),
                new Vector3(ROOM_W - 0.40f, RAFTER_W, RAFTER_W),
                C_PINE);
        }

        EditorUtility.SetDirty(root);
        Debug.Log("[KitchenAmbienceBuilder] 천장 대들보 + 서까래 완료");
    }

    // ═════════════════════════════════════════════════════════════
    //  2. 한지 등불 — 뒷벽 3개 + 왼쪽 벽 2개
    //
    //  배치 다이어그램 (위에서 본 평면):
    //
    //              뒷벽 z=4.0
    //    🏮(-3,4)   🏮(0,4)   🏮(3,4)
    //    ┌──────────────────────────────┐
    //    │                              │
    // 🏮 │   FarmSt  CookSt  ServeSt   │  왼쪽 벽 x=-4
    // 🏮 │                              │
    //    └──────────────────────────────┘
    //  (-4,-3)  (-4,0)
    //
    //  액자(z=1.5) 위 등불은 제거 → 대신 PhotoFrameBuilder의 스포트라이트로 대체
    // ═════════════════════════════════════════════════════════════
    static void BuildHangingDecor(Transform parent)
    {
        var root = new GameObject("HangingDecor");
        root.transform.SetParent(parent, false);

        // ── 한지 등불 3개 ─────────────────────────────────────────
        // 왜 z=4.0? (뒷벽 근처)
        //   등불이 z=-0.5(앞쪽)에 있으면 등각뷰에서 스테이션 위를 가림.
        //   뒷벽(z≈4.5) 바로 앞(z=4.0)에 배치하면 스테이션 뒤쪽으로 물러나
        //   플레이 화면을 가리지 않음.
        // 왜 y=2.0? (높이 올림)
        //   뒷벽에서는 카메라 시야각 덕분에 조금 높여도 스테이션과 겹치지 않음.
        float lanternY = 2.0f;

        // ── 뒷벽 등불 3개 (x축 배열) ──────────────────────────────
        float[] lanternX = { -3.0f, 0.0f, 3.0f };
        float   lanternZ = 4.0f;
        foreach (float lx in lanternX)
            BuildLantern(root.transform, new Vector3(lx, lanternY, lanternZ));

        // ── 왼쪽 벽 등불 2개 (뒷벽과 대칭, z축 배열) ──────────────
        // z=3.0 은 액자(z=1.5) 바로 위에 겹쳐 보이므로 제외.
        // 액자 조명은 PhotoFrameBuilder 의 스포트라이트로 대체.
        float   leftWallX = -4.0f;
        float[] leftWallZ = { -3.0f, 0.0f };
        foreach (float lz in leftWallZ)
            BuildLantern(root.transform, new Vector3(leftWallX, lanternY, lz));

        EditorUtility.SetDirty(root);
        Debug.Log("[KitchenAmbienceBuilder] 한지 등불 6개 배치 완료 (뒷벽 3 + 왼쪽벽 3)");
    }

    // ── 한지 사각 등불 ────────────────────────────────────────────
    //
    //   ○  ← 철재 고리 (대들보에 걸림)
    //   │  ← 새끼줄
    //   ┬  ← 상단 뚜껑 (대들보색 나무)
    //  ┌┼┐ ← 소나무 틀 4기둥
    //  │H│ ← 한지 4면 (H = C_HANJI)
    //  └─┘ ← 하단 받침
    //   │  ← 단청 술 (장식)
    static void BuildLantern(Transform parent, Vector3 topPos)
    {
        // 등불 실제 중심 위치: 대들보에서 0.5m 아래
        float ropeLen = 0.40f;
        float lH      = 0.34f;   // 등불 높이
        float lW      = 0.20f;   // 등불 폭·깊이

        float rCy   = topPos.y - ropeLen * 0.5f;          // 줄 중심
        float lidY  = topPos.y - ropeLen - 0.015f;        // 뚜껑
        float boxCy = lidY - lH * 0.5f - 0.015f;          // 등불 중심
        float botY  = boxCy - lH * 0.5f - 0.008f;         // 하단 받침
        float tailY = botY - 0.12f;                        // 술 중심

        var x = topPos.x; var z = topPos.z;

        // 고리
        SubCyl(parent, "Hook",
            new Vector3(x, topPos.y + 0.03f, z),
            new Vector3(0.025f, 0.018f, 0.025f), C_IRON);

        // 줄
        SubBox(parent, "Rope",
            new Vector3(x, rCy, z),
            new Vector3(0.018f, ropeLen, 0.018f), C_ROPE);

        // 상단 뚜껑
        SubBox(parent, "Lid",
            new Vector3(x, lidY, z),
            new Vector3(lW + 0.04f, 0.025f, lW + 0.04f), C_BEAM);

        // 틀 4기둥 (소나무)
        float hx = lW * 0.5f - 0.012f;
        SubBox(parent, "Post_FL", new Vector3(x - hx, boxCy, z - hx), new Vector3(0.020f, lH, 0.020f), C_PINE);
        SubBox(parent, "Post_FR", new Vector3(x + hx, boxCy, z - hx), new Vector3(0.020f, lH, 0.020f), C_PINE);
        SubBox(parent, "Post_BL", new Vector3(x - hx, boxCy, z + hx), new Vector3(0.020f, lH, 0.020f), C_PINE);
        SubBox(parent, "Post_BR", new Vector3(x + hx, boxCy, z + hx), new Vector3(0.020f, lH, 0.020f), C_PINE);

        // 한지 4면 (살짝 투명한 느낌은 색만으로 — 단일 머티리얼)
        // 앞뒤면
        SubBox(parent, "Panel_F",
            new Vector3(x, boxCy, z - lW * 0.5f),
            new Vector3(lW - 0.025f, lH - 0.025f, 0.012f), C_HANJI);
        SubBox(parent, "Panel_B",
            new Vector3(x, boxCy, z + lW * 0.5f),
            new Vector3(lW - 0.025f, lH - 0.025f, 0.012f), C_HANJI);
        // 좌우면
        SubBox(parent, "Panel_L",
            new Vector3(x - lW * 0.5f, boxCy, z),
            new Vector3(0.012f, lH - 0.025f, lW - 0.025f), C_HANJI);
        SubBox(parent, "Panel_R",
            new Vector3(x + lW * 0.5f, boxCy, z),
            new Vector3(0.012f, lH - 0.025f, lW - 0.025f), C_HANJI);

        // 창살 (가로 2줄 — 단순한 격자)
        foreach (float panelZ in new[] { z - lW * 0.5f - 0.001f, z + lW * 0.5f + 0.001f })
        {
            SubBox(parent, "WinBar",
                new Vector3(x, boxCy + lH * 0.12f, panelZ),
                new Vector3(lW - 0.025f, 0.012f, 0.014f), C_PINE);
            SubBox(parent, "WinBar",
                new Vector3(x, boxCy - lH * 0.12f, panelZ),
                new Vector3(lW - 0.025f, 0.012f, 0.014f), C_PINE);
        }

        // 하단 받침
        SubBox(parent, "Base",
            new Vector3(x, botY, z),
            new Vector3(lW + 0.04f, 0.025f, lW + 0.04f), C_BEAM);

        // 단청 술 (4개 가닥 — 붉은 장식)
        // 왜 술? 한옥 등불·노리개에는 단청색 술이 기본 장식임.
        SubCyl(parent, "Tail1", new Vector3(x - 0.04f, tailY, z - 0.04f), new Vector3(0.010f, 0.12f, 0.010f), C_DANCHEONG);
        SubCyl(parent, "Tail2", new Vector3(x + 0.04f, tailY, z - 0.04f), new Vector3(0.010f, 0.12f, 0.010f), C_DANCHEONG);
        SubCyl(parent, "Tail3", new Vector3(x - 0.04f, tailY, z + 0.04f), new Vector3(0.010f, 0.12f, 0.010f), C_DANCHEONG);
        SubCyl(parent, "Tail4", new Vector3(x + 0.04f, tailY, z + 0.04f), new Vector3(0.010f, 0.12f, 0.010f), C_DANCHEONG);
    }

    // ── 마늘 묶음 ────────────────────────────────────────────────
    //
    //   (대들보)
    //     │  ← 새끼줄 0.3m
    //    ╔╗  ← 묶은 매듭 (짙은 갈색 원통)
    //   ◯◯◯  ← 마늘 통 3개 (구체, 크림색)
    //   ◯◯   ← 마늘 통 2개 (아래 열)
    // (각 마늘에 작은 꼭지)
    static void BuildGarlicBundle(Transform parent, Vector3 topPos)
    {
        float ropeLen = 0.28f;
        float bundleY = topPos.y - ropeLen;
        float x = topPos.x, z = topPos.z;

        // 줄
        SubBox(parent, "GarlicRope",
            new Vector3(x, topPos.y - ropeLen * 0.5f, z),
            new Vector3(0.015f, ropeLen, 0.015f), C_ROPE);

        // 매듭
        SubCyl(parent, "GarlicKnot",
            new Vector3(x, bundleY, z),
            new Vector3(0.045f, 0.030f, 0.045f), C_BEAM);

        // 마늘 5개 (불규칙 배치)
        var bulbs = new (float dx, float dz, float dy, float s)[]
        {
            (-0.055f, -0.03f, -0.065f, 0.065f),
            ( 0.000f, -0.04f, -0.075f, 0.072f),
            ( 0.055f, -0.02f, -0.060f, 0.060f),
            (-0.030f,  0.03f, -0.130f, 0.062f),
            ( 0.035f,  0.04f, -0.125f, 0.058f),
        };
        foreach (var b in bulbs)
        {
            SubSph(parent, "Garlic",
                new Vector3(x + b.dx, bundleY + b.dy, z + b.dz),
                new Vector3(b.s, b.s * 0.85f, b.s), C_GARLIC);
            // 꼭지 (작은 초록 원통)
            SubCyl(parent, "GarlicStem",
                new Vector3(x + b.dx, bundleY + b.dy + b.s * 0.5f, z + b.dz),
                new Vector3(0.012f, 0.025f, 0.012f), C_CHILI_ST);
        }

        // 마늘 껍질 껍데기 느낌 (살짝 어두운 면 — 앞쪽 3개에)
        SubSph(parent, "GarlicShell",
            new Vector3(x, bundleY - 0.07f, z - 0.025f),
            new Vector3(0.040f, 0.032f, 0.040f), C_GARLIC_SK);
    }

    // ── 고추 묶음 ────────────────────────────────────────────────
    //
    //   (대들보)
    //     │  ← 새끼줄 0.28m
    //    ╔╗  ← 묶은 매듭 (짙은 갈색)
    //   ║║║  ← 고추 5개 (원통, 단청빨강)
    //      각 고추 상단에 초록 꼭지
    static void BuildChiliBundle(Transform parent, Vector3 topPos)
    {
        float ropeLen = 0.24f;
        float bundleY = topPos.y - ropeLen;
        float x = topPos.x, z = topPos.z;

        // 줄
        SubBox(parent, "ChiliRope",
            new Vector3(x, topPos.y - ropeLen * 0.5f, z),
            new Vector3(0.015f, ropeLen, 0.015f), C_ROPE);

        // 매듭
        SubCyl(parent, "ChiliKnot",
            new Vector3(x, bundleY, z),
            new Vector3(0.040f, 0.025f, 0.040f), C_BEAM);

        // 고추 5개 (불규칙 위치 + 살짝 기울어진 느낌을 scale로 표현)
        var chilies = new (float dx, float dz, float dy, float sw, float sh)[]
        {
            (-0.06f, -0.02f, -0.08f, 0.025f, 0.12f),
            (-0.02f, -0.03f, -0.10f, 0.022f, 0.13f),
            ( 0.01f,  0.00f, -0.07f, 0.028f, 0.11f),
            ( 0.05f, -0.01f, -0.09f, 0.023f, 0.12f),
            (-0.03f,  0.03f, -0.12f, 0.020f, 0.10f),
        };
        foreach (var c in chilies)
        {
            // 고추 몸통
            SubCyl(parent, "Chili",
                new Vector3(x + c.dx, bundleY + c.dy, z + c.dz),
                new Vector3(c.sw, c.sh * 0.5f, c.sw), C_CHILI);
            // 꼭지
            SubCyl(parent, "ChiliStem",
                new Vector3(x + c.dx, bundleY + c.dy + c.sh * 0.5f + 0.01f, z + c.dz),
                new Vector3(0.010f, 0.022f, 0.010f), C_CHILI_ST);
            // 끝부분 (뾰족한 느낌 — 작은 원뿔 대신 작은 구체)
            SubSph(parent, "ChiliTip",
                new Vector3(x + c.dx, bundleY + c.dy - c.sh * 0.5f, z + c.dz),
                new Vector3(c.sw * 0.5f, c.sw * 0.5f, c.sw * 0.5f), C_CHILI * 0.75f);
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  3. 뒷벽 장독대 — 항아리 8개 + 받침돌
    //
    //  장독대(醬獨臺):
    //   전통 한옥에서 뒷마당이나 뒷벽 앞에 항아리를 줄지어 놓는 구역.
    //   간장(간 항아리), 된장(된 항아리), 고추장(붉은 항아리) 등.
    //   게임에서는 단순히 분위기 소품 — 상호작용 없음.
    //
    //  항아리 종류:
    //   - 대형 (h=0.65) 3개: 간장(짙은), 된장(점토), 고추장(붉은)
    //   - 중형 (h=0.50) 3개: 점토색
    //   - 소형 (h=0.32) 2개: 양념통
    //
    //  배치:
    //    뒷벽 앞 (z = 4.2), x = -3.5 ~ 3.5
    //    받침돌: 각 항아리 아래 납작한 회색 박스
    // ═════════════════════════════════════════════════════════════
    static void BuildJarRow(Transform parent)
    {
        var root = new GameObject("JarRow");
        root.transform.SetParent(parent, false);

        var C_STONE_BASE = new Color(0.52f, 0.48f, 0.42f); // 받침돌 회색

        // 항아리 배치 정의 (x, 항아리색, 높이 계수)
        // 왜 이 x 좌표? 뒷벽(z≈5) 앞에서 자연스럽게 퍼진 간격.
        var jars = new (float x, Color bodyCol, Color rimCol, float scale)[]
        {
            (-3.2f, C_JAR_DARK, C_CLAY_DRK,  1.00f), // 간장 대형
            (-1.8f, C_CLAY,     C_CLAY_DRK,   1.10f), // 된장 대형 (약간 더 큼)
            (-0.4f, C_JAR_RED,  C_CLAY_DRK,   0.95f), // 고추장 대형
            ( 0.8f, C_CLAY,     C_CLAY_DRK,   0.75f), // 중형
            ( 1.8f, C_CLAY*0.9f,C_CLAY_DRK,  0.80f), // 중형
            ( 2.8f, C_JAR_DARK, C_CLAY_DRK,   0.72f), // 중형 (간장)
            ( 3.6f, C_CLAY,     C_CLAY_DRK,   0.52f), // 소형
            (-2.6f, C_CLAY,     C_CLAY_DRK,   0.55f), // 소형
        };

        float jarZ = 4.10f; // 뒷벽에서 약간 앞

        foreach (var j in jars)
        {
            float s = j.scale;
            float bW = 0.36f * s;  // 배 폭
            float bH = 0.50f * s;  // 배 높이
            float nH = 0.10f * s;  // 목 높이
            float rH = 0.05f * s;  // 입술 높이
            float baseH = 0.06f * s;

            float baseY  = baseH * 0.5f;
            float bodyY  = baseH + bH * 0.5f;
            float neckY  = baseH + bH + nH * 0.5f;
            float rimY   = baseH + bH + nH + rH * 0.5f;
            float lidY   = rimY + rH * 0.5f + 0.01f * s;

            // 받침돌
            SubBox(root.transform, "JarBase",
                new Vector3(j.x, baseY, jarZ),
                new Vector3(bW * 0.80f, baseH, bW * 0.80f), C_STONE_BASE);

            // 항아리 배 (볼록한 구체형)
            SubSph(root.transform, "JarBody",
                new Vector3(j.x, bodyY, jarZ),
                new Vector3(bW, bH, bW * 0.88f), j.bodyCol);

            // 항아리 어깨 (어두운 빛 — 어깨선 강조)
            SubCyl(root.transform, "JarShoulder",
                new Vector3(j.x, baseH + bH * 0.72f, jarZ),
                new Vector3(bW * 0.90f, 0.015f * s, bW * 0.90f), j.rimCol);

            // 목
            SubCyl(root.transform, "JarNeck",
                new Vector3(j.x, neckY, jarZ),
                new Vector3(bW * 0.35f, nH * 0.5f, bW * 0.35f), j.rimCol);

            // 입술 (테두리)
            SubCyl(root.transform, "JarRim",
                new Vector3(j.x, rimY, jarZ),
                new Vector3(bW * 0.42f, rH * 0.5f, bW * 0.42f), j.rimCol);

            // 뚜껑 (납작 반구)
            SubSph(root.transform, "JarLid",
                new Vector3(j.x, lidY, jarZ),
                new Vector3(bW * 0.38f, bW * 0.14f, bW * 0.38f), j.rimCol * 1.15f);

            // 뚜껑 손잡이
            SubCyl(root.transform, "JarLidKnob",
                new Vector3(j.x, lidY + bW * 0.14f, jarZ),
                new Vector3(0.018f * s, 0.022f * s, 0.018f * s), j.rimCol);
        }

        // ── 항아리 구역 경계: 납작한 돌 틀 ──────────────────────────
        // 왜? 장독대는 돌 혹은 나무 경계로 구분된 독립 공간.
        //      얇은 경계선으로 바닥에 시각적 구역 표시.
        SubBox(root.transform, "JarBorder_Front",
            new Vector3(0f, 0.03f, jarZ - 0.60f),
            new Vector3(8.2f, 0.04f, 0.08f), C_STONE_BASE);
        SubBox(root.transform, "JarBorder_L",
            new Vector3(-4.0f, 0.03f, jarZ - 0.30f),
            new Vector3(0.08f, 0.04f, 0.64f), C_STONE_BASE);
        SubBox(root.transform, "JarBorder_R",
            new Vector3( 4.0f, 0.03f, jarZ - 0.30f),
            new Vector3(0.08f, 0.04f, 0.64f), C_STONE_BASE);

        EditorUtility.SetDirty(root);
        Debug.Log("[KitchenAmbienceBuilder] 장독대 8개 완료");
    }

    // ═════════════════════════════════════════════════════════════
    //  헬퍼
    //
    //  KitchenStationsBuilder 와 동일한 SubBox/SubCyl/SubSph 패턴.
    //  콜라이더를 즉시 제거 → 데코 오브젝트는 물리 충돌 없음.
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
