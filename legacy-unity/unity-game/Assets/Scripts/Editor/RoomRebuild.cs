using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 씬을 처음부터 재건합니다 — 한옥 스타일.
/// Menu: Tools / 100 Burger Family / 씬 초기화 (처음부터 재건)
/// ※ ■ Stop 상태(Edit 모드)에서 실행하세요.
/// </summary>
public static class RoomRebuild
{
    // ── 치수 ──────────────────────────────────────────────────────
    const float ROOM_W   = 10f;
    const float ROOM_D   = 10f;
    const float WALL_H   = 3.5f;
    const float WALL_T   = 0.25f;
    const float FLOOR_T  = 0.1f;
    const float ST_W     = 1.4f;
    const float ST_H     = 0.9f;
    const float ST_D     = 1.0f;
    const float CAM_SIZE = 7f;

    // ── 한옥 색상 팔레트 ──────────────────────────────────────────
    static readonly Color C_FLOOR      = new Color(0.72f, 0.52f, 0.28f); // 따뜻한 마루 나무
    static readonly Color C_WALL       = new Color(0.93f, 0.90f, 0.82f); // 한지 아이보리
    static readonly Color C_WOOD_DARK  = new Color(0.28f, 0.18f, 0.08f); // 기둥·들보 진한 나무
    static readonly Color C_WOOD_MED   = new Color(0.48f, 0.32f, 0.15f); // 중간 나무색
    static readonly Color C_SHOJI      = new Color(0.97f, 0.95f, 0.88f); // 창호지 밝은 흰색
    const float CAM_DIST = 30f;
    static readonly Vector3 CAM_TARGET = new Vector3(0f, 2f, 0f); // y 올림 → 방이 뷰포트 아래로

    // ── Play 모드 중에는 메뉴 항목 비활성 ─────────────────────────
    [MenuItem("Tools/100 Burger Family/씬 초기화 (처음부터 재건)", true)]
    static bool ValidateRebuild() => !Application.isPlaying;

    [MenuItem("Tools/100 Burger Family/씬 초기화 (처음부터 재건)")]
    public static void Rebuild()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("실행 불가",
                "■ Stop을 눌러 게임을 멈춘 뒤 실행하세요.", "확인");
            return;
        }

        if (!EditorUtility.DisplayDialog("씬 초기화 확인",
            "모든 오브젝트(카메라/GameManager/HUD Canvas 제외)를 삭제하고\n처음부터 재건합니다.\n계속하시겠습니까?",
            "예, 재건합니다", "취소"))
            return;

        Debug.Log("═══ [RoomRebuild v4] 시작 ═══");

        // 1. Player 보호 (씬 루트로 임시 이동)
        var player = ProtectPlayer();

        // 2. 완전 삭제 (컴포넌트 화이트리스트 방식)
        NukeEverything();

        // 3. 새 RoomScene 생성
        var roomRoot = new GameObject("RoomScene");
        roomRoot.transform.localPosition = Vector3.zero;
        roomRoot.transform.localRotation = Quaternion.identity;
        roomRoot.transform.localScale    = Vector3.one;

        // 4. 방 기하학
        BuildRoom(roomRoot.transform);

        // 5. 스테이션 — 수동 배치 예정이므로 생략
        // BuildStations(roomRoot.transform);

        // 6. Player 복원
        RestorePlayer(player, roomRoot.transform);

        // 7. 카메라
        SetupCamera();

        // 8. 씬 저장
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("═══ [RoomRebuild v4] 완료! ▶ Play로 확인하세요 ═══");
        EditorUtility.DisplayDialog("재건 완료",
            "씬 재건 완료!\n▶ Play로 결과를 확인하세요.", "확인");
    }

    // ══════════════════════════════════════════════════════════════
    //  완전 삭제 — 컴포넌트 화이트리스트 방식
    //
    //  보호 대상 (삭제하지 않음):
    //    - Camera 컴포넌트가 있는 오브젝트 (메인 카메라)
    //    - EventSystem 컴포넌트가 있는 오브젝트
    //    - GameManager 컴포넌트가 있는 오브젝트
    //    - Canvas 컴포넌트가 있는 오브젝트 (HUD)
    //    - Directional Light
    //    - PlayerController 컴포넌트가 있는 오브젝트 (임시 보호된 Player)
    //
    //  나머지는 이름에 상관없이 모두 재귀 삭제
    // ══════════════════════════════════════════════════════════════
    static void NukeEverything()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = new List<GameObject>(scene.GetRootGameObjects());

        int killed = 0;
        int kept   = 0;

        foreach (var root in roots)
        {
            if (root == null) continue;

            if (IsProtected(root))
            {
                Debug.Log($"[RoomRebuild] 보호 유지: '{root.name}'");
                kept++;
                continue;
            }

            Debug.Log($"[RoomRebuild] 삭제 대상: '{root.name}' (자식 {root.transform.childCount}개)");
            DestroyChildrenRecursive(root.transform);
            Object.DestroyImmediate(root);
            killed++;
        }

        Debug.Log($"[RoomRebuild] 삭제 완료: {killed}개 삭제 / {kept}개 보호");
    }

    /// 보호할 오브젝트인지 판별 (컴포넌트 기반)
    static bool IsProtected(GameObject go)
    {
        // 메인 카메라
        if (go.GetComponent<Camera>() != null)                  return true;
        // UI EventSystem
        if (go.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) return true;
        // GameManager 싱글톤
        if (go.GetComponent<GameManager>() != null)             return true;
        // RoomManager 싱글톤
        if (go.GetComponent<RoomManager>() != null)             return true;
        // HUD Canvas
        if (go.GetComponent<Canvas>() != null)                  return true;
        // Player (ProtectPlayer()가 루트로 옮긴 상태)
        if (go.GetComponent<PlayerController>() != null)        return true;
        // Directional Light
        var light = go.GetComponent<Light>();
        if (light != null && light.type == LightType.Directional) return true;

        return false;
    }

    // 재귀적으로 모든 자식을 아래부터 위로 삭제
    static void DestroyChildrenRecursive(Transform t)
    {
        var children = new List<Transform>();
        foreach (Transform child in t)
            children.Add(child);

        foreach (var child in children)
        {
            if (child == null) continue;
            DestroyChildrenRecursive(child);   // 손자 먼저
            Object.DestroyImmediate(child.gameObject);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Player 보호 / 복원
    // ══════════════════════════════════════════════════════════════
    static GameObject ProtectPlayer()
    {
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null) return null;

        var go = pc.gameObject;
        go.transform.SetParent(null, true);  // 씬 루트로 이동
        Debug.Log($"[RoomRebuild] Player '{go.name}' 임시 보호");
        return go;
    }

    static void RestorePlayer(GameObject player, Transform roomParent)
    {
        if (player == null)
            player = CreateNewPlayer();

        player.transform.SetParent(roomParent, false);
        player.transform.localScale    = new Vector3(0.4f, 0.4f, 0.4f); // 이전(2x)의 1/5
        player.transform.localPosition = new Vector3(0f, 0.4f, -2.5f);  // y=0.4: 바닥에 발이 닿도록
        player.transform.localRotation = Quaternion.identity;

        // CapsuleCollider — 로컬 스케일(0.4) 기준: 월드 h≈0.8m
        var col = player.GetComponent<CapsuleCollider>();
        if (col == null) col = player.AddComponent<CapsuleCollider>();
        col.height = 2.0f;   // local height 2 × scale 0.4 = 월드 0.8m
        col.radius = 0.5f;   // local radius 0.5 × scale 0.4 = 월드 0.2m
        col.center = Vector3.zero;

        EditorUtility.SetDirty(player);
        Debug.Log($"[RoomRebuild] Player '{player.name}' 배치 → (0, 0.875, -2.5)");
    }

    static GameObject CreateNewPlayer()
    {
        // 빈 루트 오브젝트 (비주얼은 PlayerVisual이 런타임에 생성)
        var go = new GameObject("Player");

        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerHand>();
        go.AddComponent<PlayerVisual>();   // 런타임에 캐릭터 비주얼 빌드

        // 물리
        var rb = go.AddComponent<Rigidbody>();
        rb.constraints   = RigidbodyConstraints.FreezeRotation
                         | RigidbodyConstraints.FreezePositionY;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity    = false;
        rb.mass          = 1f;

        // 콜라이더 (PlayerVisual.Start()에서 재조정됨)
        var col = go.AddComponent<CapsuleCollider>();
        col.height = 0.9f;
        col.radius = 0.18f;
        col.center = new Vector3(0f, 0.45f, 0f);

        Debug.Log("[RoomRebuild] 새 Player 생성 (PlayerVisual 부착)");
        return go;
    }

    // ══════════════════════════════════════════════════════════════
    //  한옥 방 기하학
    // ══════════════════════════════════════════════════════════════
    static void BuildRoom(Transform parent)
    {
        float hw = ROOM_W / 2f;
        float hd = ROOM_D / 2f;
        float pillarW = 0.30f;

        // ── 바닥 (따뜻한 나무 마루) ─────────────────────────────
        CreateBox(parent, "Floor",
            new Vector3(0f, -FLOOR_T / 2f, 0f),
            new Vector3(ROOM_W, FLOOR_T, ROOM_D),
            C_FLOOR);

        // 바닥 앞면 테두리 (어두운 나무 몰딩)
        CreateBox(parent, "Floor_Border",
            new Vector3(0f, FLOOR_T * 0.5f, -hd + 0.08f),
            new Vector3(ROOM_W, 0.06f, 0.14f),
            C_WOOD_DARK);

        // ── 뒷벽 (한지 — 창호지 톤) ──────────────────────────────
        CreateBox(parent, "BackWall",
            new Vector3(0f, WALL_H / 2f, hd - WALL_T / 2f),
            new Vector3(ROOM_W, WALL_H, WALL_T),
            C_WALL);

        // ── 왼쪽 벽 (한지) ────────────────────────────────────────
        CreateBox(parent, "LeftWall",
            new Vector3(-hw + WALL_T / 2f, WALL_H / 2f, 0f),
            new Vector3(WALL_T, WALL_H, ROOM_D),
            C_WALL);

        // ── 나무 기둥 (모서리 3개 — 한옥 기둥) ───────────────────
        // 왼쪽-뒤 코너
        CreateBox(parent, "Pillar_BackLeft",
            new Vector3(-hw + pillarW / 2f, WALL_H / 2f, hd - pillarW / 2f),
            new Vector3(pillarW, WALL_H, pillarW),
            C_WOOD_DARK);
        // 왼쪽-앞 코너
        CreateBox(parent, "Pillar_FrontLeft",
            new Vector3(-hw + pillarW / 2f, WALL_H / 2f, -hd + pillarW / 2f),
            new Vector3(pillarW, WALL_H, pillarW),
            C_WOOD_DARK);
        // 오른쪽-뒤 코너 (뒷벽 끝)
        CreateBox(parent, "Pillar_BackRight",
            new Vector3(hw - pillarW / 2f, WALL_H / 2f, hd - pillarW / 2f),
            new Vector3(pillarW, WALL_H, pillarW),
            C_WOOD_DARK);

        // ── 상단 들보 (나무 대들보) ───────────────────────────────
        float beamH = 0.22f;
        // 뒷벽 위 들보
        CreateBox(parent, "Beam_Back",
            new Vector3(0f, WALL_H - beamH / 2f, hd - WALL_T / 2f),
            new Vector3(ROOM_W + 0.05f, beamH, WALL_T + 0.04f),
            C_WOOD_DARK);
        // 왼쪽 벽 위 들보
        CreateBox(parent, "Beam_Left",
            new Vector3(-hw + WALL_T / 2f, WALL_H - beamH / 2f, 0f),
            new Vector3(WALL_T + 0.04f, beamH, ROOM_D + 0.05f),
            C_WOOD_DARK);

        // ── 창호 (뒷벽 격자 창문 3개) ────────────────────────────
        BuildShojiWindows(parent, hd);

        Debug.Log($"[RoomRebuild] 한옥 방 생성 완료 ({ROOM_W}×{ROOM_D}m)");
    }

    /// <summary>뒷벽에 창호지 스타일 격자 창문 3개 생성</summary>
    static void BuildShojiWindows(Transform parent, float hd)
    {
        float wallZ    = hd - WALL_T * 0.35f; // 벽 안쪽 면
        float panelW   = 2.0f;
        float panelH   = 1.8f;
        float panelBotY = 0.75f;
        float panelCY  = panelBotY + panelH / 2f;

        float[] xs = { -3.0f, 0f, 3.0f };

        foreach (float px in xs)
        {
            string s = px < 0f ? "L" : (px > 0f ? "R" : "C");

            // 창호지 배경
            CreateBox(parent, $"Shoji_{s}",
                new Vector3(px, panelCY, wallZ),
                new Vector3(panelW, panelH, 0.03f),
                C_SHOJI);

            // 가로 격자살 (3개)
            for (int i = 1; i <= 3; i++)
            {
                float gy = panelBotY + i * panelH / 4f;
                CreateBox(parent, $"ShojiH_{s}_{i}",
                    new Vector3(px, gy, wallZ + 0.015f),
                    new Vector3(panelW - 0.04f, 0.035f, 0.04f),
                    C_WOOD_DARK);
            }

            // 세로 격자살 (1개 — 중앙)
            CreateBox(parent, $"ShojiV_{s}",
                new Vector3(px, panelCY, wallZ + 0.015f),
                new Vector3(0.035f, panelH - 0.04f, 0.04f),
                C_WOOD_DARK);

            // 창틀 4면
            CreateBox(parent, $"ShojiF_T_{s}",
                new Vector3(px, panelBotY + panelH + 0.02f, wallZ + 0.015f),
                new Vector3(panelW + 0.06f, 0.05f, 0.04f),
                C_WOOD_DARK);
            CreateBox(parent, $"ShojiF_B_{s}",
                new Vector3(px, panelBotY - 0.02f, wallZ + 0.015f),
                new Vector3(panelW + 0.06f, 0.05f, 0.04f),
                C_WOOD_DARK);
            CreateBox(parent, $"ShojiF_L_{s}",
                new Vector3(px - panelW / 2f - 0.02f, panelCY, wallZ + 0.015f),
                new Vector3(0.05f, panelH + 0.06f, 0.04f),
                C_WOOD_DARK);
            CreateBox(parent, $"ShojiF_R_{s}",
                new Vector3(px + panelW / 2f + 0.02f, panelCY, wallZ + 0.015f),
                new Vector3(0.05f, panelH + 0.06f, 0.04f),
                C_WOOD_DARK);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  게임 스테이션
    // ══════════════════════════════════════════════════════════════
    static void BuildStations(Transform parent)
    {
        {
            var go = CreateStation(parent, "FarmStation",
                new Vector3(-2.5f, ST_H / 2f, 3.5f),
                new Color(0.30f, 0.68f, 0.25f), "농장");
            go.AddComponent<FarmStation>();
            go.AddComponent<InteractionBubble>();
        }
        {
            var go = CreateStation(parent, "CookStation",
                new Vector3(0f, ST_H / 2f, 1.0f),
                new Color(0.93f, 0.52f, 0.18f), "조리대");
            go.AddComponent<CookStation>();
            go.AddComponent<InteractionBubble>();
        }
        {
            var go = CreateStation(parent, "ServeCounter",
                new Vector3(2.5f, ST_H / 2f, -1.5f),
                new Color(0.25f, 0.50f, 0.88f), "서빙");
            go.AddComponent<ServeCounter>();
            go.AddComponent<InteractionBubble>();
        }

        Debug.Log("[RoomRebuild] 스테이션 3개 생성 (Farm / Cook / Serve)");
    }

    // ══════════════════════════════════════════════════════════════
    //  카메라
    // ══════════════════════════════════════════════════════════════
    static void SetupCamera()
    {
        var isoCtrl = Object.FindFirstObjectByType<IsometricCameraController>();
        if (isoCtrl != null)
        {
            isoCtrl.orthographicSize = CAM_SIZE;
            isoCtrl.lookTarget       = CAM_TARGET;
            EditorUtility.SetDirty(isoCtrl.gameObject);
        }

        var cam = Camera.main;
        if (cam == null) { Debug.LogWarning("[RoomRebuild] Main Camera 없음"); return; }

        cam.orthographic     = true;
        cam.orthographicSize = CAM_SIZE;
        cam.nearClipPlane    = 0.1f;
        cam.farClipPlane     = 200f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.85f, 0.92f, 0.78f);

        cam.transform.rotation = Quaternion.Euler(30f, -45f, 0f);
        cam.transform.position = CAM_TARGET - cam.transform.forward * CAM_DIST;

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log($"[RoomRebuild] 카메라 설정 완료 (size={CAM_SIZE})");
    }

    // ══════════════════════════════════════════════════════════════
    //  헬퍼
    // ══════════════════════════════════════════════════════════════
    static GameObject CreateBox(Transform parent, string name,
                                Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        ApplyURPColor(go, color);
        return go;
    }

    static GameObject CreateStation(Transform parent, string name,
                                    Vector3 pos, Color color, string label)
    {
        var go = CreateBox(parent, name, pos, new Vector3(ST_W, ST_H, ST_D), color);

        // 라벨 (World Space, 개발 중 식별용)
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        labelGO.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        labelGO.transform.localScale    = new Vector3(0.012f, 0.012f, 0.012f);

        var canvas = labelGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 5;
        var rt = labelGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 60f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(labelGO.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 28;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.fontStyle = TMPro.FontStyles.Bold;

        return go;
    }

    static void ApplyURPColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var shader = Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard");
        if (shader == null) return;
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color",     color);
        rend.sharedMaterial = mat;
    }
}
