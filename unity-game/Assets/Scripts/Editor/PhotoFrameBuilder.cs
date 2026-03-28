using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 가족사진 액자를 왼쪽 벽에 배치하는 에디터 빌더.
///
/// ══════════════════════════════════════════════
///  디자인 컨셉: 한옥 전통 목재 액자
/// ══════════════════════════════════════════════
///
///   ╔══════════════════════════════╗  ← 코너 장식 (대들보 — 주두 느낌)
///   ║ ┌──────────────────────────┐ ║  ← 외부 프레임 (참나무)
///   ║ │ ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄ │ ║  ← 단청 내부 몰딩 (빨간 선)
///   ║ │                          │ ║
///   ║ │     [ 사진 캔버스 ]       │ ║  ← PhotoCanvas (텍스처 적용 대상)
///   ║ │     흰색 → 업로드 후 사진  │ ║
///   ║ │ ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄ │ ║
///   ║ └──────────────────────────┘ ║
///   ╚══════════════════════════════╝
///         │           │              ← 벽 마운트 브래킷 (대들보색)
///   ─────────────────────────────     ← LeftWall 면
///
/// 배치 조건:
///   - 부모: RoomScene
///   - 위치: (-3.82, 1.35, 1.5) — 왼쪽 벽 눈높이
///   - 회전: Y=90° — 방 안쪽(+X)을 향함
///   - GameObject 이름: "PhotoFrame" (jslib SendMessage 타깃과 일치해야 함)
/// </summary>
public static class PhotoFrameBuilder
{
    // ── 한옥 색상 팔레트 ──────────────────────────────────────────
    static readonly Color C_OAK       = new Color(0.50f, 0.32f, 0.14f); // 참나무 프레임
    static readonly Color C_BEAM      = new Color(0.25f, 0.16f, 0.06f); // 대들보 코너/마운트
    static readonly Color C_DANCHEONG = new Color(0.72f, 0.15f, 0.08f); // 단청 내부 몰딩
    static readonly Color C_PINE      = new Color(0.75f, 0.55f, 0.28f); // 소나무 포인트
    static readonly Color C_CANVAS    = new Color(0.97f, 0.95f, 0.92f); // 사진 캔버스 (따뜻한 흰색)

    // ── 배치 좌표 ─────────────────────────────────────────────────
    // 왜 Y=90°? LeftWall은 XZ 평면에서 X축 방향으로 뻗어 있음.
    //   액자가 방 안쪽을 향하려면 +X 방향을 정면으로 바라봐야 하므로 Y=90°.
    static readonly Vector3 POS = new Vector3(-3.82f, 1.35f, 1.50f);
    static readonly Vector3 ROT = new Vector3(0f, 90f, 0f);

    // 액자 치수
    const float FW = 0.80f; // 너비
    const float FH = 0.60f; // 높이
    const float FD = 0.05f; // 두께
    const float BW = 0.055f; // 테두리 폭

    // ─────────────────────────────────────────────────────────────
    [MenuItem("Tools/100 Burger Family/Build Photo Frame")]
    public static void Build()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[PhotoFrameBuilder] Stop 후 Edit 모드에서 실행하세요.");
            return;
        }
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene == null)
        {
            Debug.LogError("[PhotoFrameBuilder] RoomScene 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 기존 액자 제거 후 재건
        var old = GameObject.Find("PhotoFrame");
        if (old != null) Object.DestroyImmediate(old);

        CreateFrame(roomScene.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[PhotoFrameBuilder] ✅ 가족사진 액자 배치 완료!");
    }

    // ══════════════════════════════════════════════════════════════
    //  액자 생성
    // ══════════════════════════════════════════════════════════════
    static void CreateFrame(Transform parent)
    {
        // ── 루트 ─────────────────────────────────────────────────
        // 이름 "PhotoFrame" 은 jslib SendMessage 타깃과 반드시 일치해야 함
        var go = new GameObject("PhotoFrame");
        go.transform.SetParent(parent, false);
        go.transform.localPosition    = POS;
        go.transform.localEulerAngles = ROT;

        // 상호작용 콜라이더 (프레임 전체)
        var col = go.AddComponent<BoxCollider>();
        col.size   = new Vector3(FW, FH, FD + 0.08f);
        col.center = Vector3.zero;

        // ── 외부 프레임 4변 (참나무) ─────────────────────────────
        // 상단 바
        SubBox(go.transform, "Frame_Top",
            new Vector3(0f,  FH * 0.5f - BW * 0.5f, 0f),
            new Vector3(FW, BW, FD), C_OAK);
        // 하단 바
        SubBox(go.transform, "Frame_Bot",
            new Vector3(0f, -FH * 0.5f + BW * 0.5f, 0f),
            new Vector3(FW, BW, FD), C_OAK);
        // 좌측 바
        SubBox(go.transform, "Frame_L",
            new Vector3(-FW * 0.5f + BW * 0.5f, 0f, 0f),
            new Vector3(BW, FH - BW * 2f, FD), C_OAK);
        // 우측 바
        SubBox(go.transform, "Frame_R",
            new Vector3( FW * 0.5f - BW * 0.5f, 0f, 0f),
            new Vector3(BW, FH - BW * 2f, FD), C_OAK);

        // ── 코너 장식 (대들보색 — 한옥 주두(柱頭) 느낌) ──────────
        // 왜? 한옥에서 기둥과 보가 만나는 부분에 장식 블록이 들어감
        float cx = FW * 0.5f - BW * 0.5f;
        float cy = FH * 0.5f - BW * 0.5f;
        float cs = BW + 0.008f;
        SubBox(go.transform, "Corner_TL", new Vector3(-cx,  cy, FD * 0.3f), new Vector3(cs, cs, FD * 0.5f), C_BEAM);
        SubBox(go.transform, "Corner_TR", new Vector3( cx,  cy, FD * 0.3f), new Vector3(cs, cs, FD * 0.5f), C_BEAM);
        SubBox(go.transform, "Corner_BL", new Vector3(-cx, -cy, FD * 0.3f), new Vector3(cs, cs, FD * 0.5f), C_BEAM);
        SubBox(go.transform, "Corner_BR", new Vector3( cx, -cy, FD * 0.3f), new Vector3(cs, cs, FD * 0.5f), C_BEAM);

        // ── 단청 내부 몰딩 (얇은 빨간 선) ───────────────────────
        // 왜? 한옥 창호 내부 테두리의 단청 채색을 재현
        float mo  = BW + 0.012f;  // 몰딩 오프셋 (테두리 안쪽)
        float mth = 0.010f;       // 몰딩 두께
        float mD  = FD * 0.18f;   // 몰딩 깊이

        float innerW = FW - BW * 2f - mo * 2f;
        float innerH = FH - BW * 2f - mo * 2f;

        SubBox(go.transform, "Mold_Top",
            new Vector3(0f,  FH * 0.5f - mo - mth * 0.5f, FD * 0.4f),
            new Vector3(innerW + mth * 2f, mth, mD), C_DANCHEONG);
        SubBox(go.transform, "Mold_Bot",
            new Vector3(0f, -FH * 0.5f + mo + mth * 0.5f, FD * 0.4f),
            new Vector3(innerW + mth * 2f, mth, mD), C_DANCHEONG);
        SubBox(go.transform, "Mold_L",
            new Vector3(-FW * 0.5f + mo + mth * 0.5f, 0f, FD * 0.4f),
            new Vector3(mth, innerH, mD), C_DANCHEONG);
        SubBox(go.transform, "Mold_R",
            new Vector3( FW * 0.5f - mo - mth * 0.5f, 0f, FD * 0.4f),
            new Vector3(mth, innerH, mD), C_DANCHEONG);

        // ── 소나무 내부 프레임 (몰딩 안쪽 얇은 선) ──────────────
        float mo2 = mo + mth + 0.006f;
        float mth2 = 0.007f;
        SubBox(go.transform, "Inner_Top",
            new Vector3(0f,  FH * 0.5f - mo2 - mth2 * 0.5f, FD * 0.35f),
            new Vector3(FW - BW * 2f - mo2 * 2f, mth2, mD), C_PINE);
        SubBox(go.transform, "Inner_Bot",
            new Vector3(0f, -FH * 0.5f + mo2 + mth2 * 0.5f, FD * 0.35f),
            new Vector3(FW - BW * 2f - mo2 * 2f, mth2, mD), C_PINE);

        // ── 사진 캔버스 ──────────────────────────────────────────
        // 이 오브젝트의 Renderer.material.mainTexture 에 업로드 이미지가 적용됨
        float canW = FW - BW * 2f - 0.025f;
        float canH = FH - BW * 2f - 0.025f;

        var canvas = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canvas.name = "PhotoCanvas";
        canvas.transform.SetParent(go.transform, false);
        canvas.transform.localPosition = new Vector3(0f, 0f, FD * 0.5f + 0.003f);
        canvas.transform.localScale    = new Vector3(canW, canH, 0.006f);
        Object.DestroyImmediate(canvas.GetComponent<BoxCollider>());

        // 캔버스 재질: Unlit 쉐이더 우선 (사진이 조명 영향 없이 선명하게 보임)
        // 왜 Unlit? 사진은 이미 광원 정보가 포함된 이미지이므로 추가 조명 연산이 오히려 사진을 왜곡시킴
        ApplyCanvasColor(canvas, C_CANVAS);

        // ── 벽 마운트 브래킷 2개 ─────────────────────────────────
        // 액자가 벽에 걸려 있다는 시각적 단서
        SubBox(go.transform, "Mount_T",
            new Vector3(0f,  FH * 0.5f + 0.028f, -FD * 0.7f),
            new Vector3(0.14f, 0.025f, 0.04f), C_BEAM);
        SubBox(go.transform, "Mount_B",
            new Vector3(0f, -FH * 0.5f - 0.028f, -FD * 0.7f),
            new Vector3(0.14f, 0.025f, 0.04f), C_BEAM);
        // 마운트 핀 (작은 네일 느낌)
        SubBox(go.transform, "Pin_T",
            new Vector3(0f, FH * 0.5f + 0.028f, -FD * 0.95f),
            new Vector3(0.035f, 0.035f, 0.025f), C_PINE);
        SubBox(go.transform, "Pin_B",
            new Vector3(0f, -FH * 0.5f - 0.028f, -FD * 0.95f),
            new Vector3(0.035f, 0.035f, 0.025f), C_PINE);

        // ── 스크립트 연결 ─────────────────────────────────────────
        var pf = go.AddComponent<PhotoFrame>();
        pf.photoRenderer = canvas.GetComponent<Renderer>();

        go.AddComponent<InteractionBubble>();

        EditorUtility.SetDirty(go);
        EditorUtility.SetDirty(pf);
    }

    // ── 헬퍼 ─────────────────────────────────────────────────────
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

    /// <summary>
    /// PhotoCanvas 전용 — Unlit 쉐이더 우선 적용.
    /// 사진이 씬 조명에 영향받지 않고 원본 색상 그대로 표시됨.
    /// </summary>
    static void ApplyCanvasColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;

        // Unlit 우선, 없으면 Lit fallback
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
                  ?? Shader.Find("Unlit/Texture")
                  ?? Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard");
        if (shader == null) return;

        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color",     color);
        rend.sharedMaterial = mat;
    }
}
