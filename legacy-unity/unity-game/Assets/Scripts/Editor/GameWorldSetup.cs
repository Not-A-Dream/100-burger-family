using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// 씬의 기존 3D 오브젝트에 게임 컴포넌트를 추가하고
/// 탑다운 카메라 + HUD를 구성합니다.
/// Menu: Tools/Setup Game World
/// </summary>
public static class GameWorldSetup
{
    static readonly Color COL_HUD_BG   = new Color(0f, 0f, 0f, 0.55f);
    static readonly Color COL_ORANGE   = new Color(0.95f, 0.58f, 0.12f);
    static readonly Color COL_WHITE    = Color.white;
    static readonly Color COL_RED      = new Color(0.88f, 0.28f, 0.22f);
    static readonly Color COL_GREEN    = new Color(0.22f, 0.70f, 0.32f);
    static TMP_FontAsset _font;

    [MenuItem("Tools/Setup Game World")]
    public static void Setup()
    {
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");

        SetupCamera();
        SetupPlayer();
        SetupStations();
        SetupHUD();
        EnsureGameManager();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[GameWorldSetup] 게임 월드 설정 완료!");
    }

    // ─────────────────────────────────────────────────────────
    // 1. 카메라: 탑다운 직교 투영
    // ─────────────────────────────────────────────────────────
    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[GameWorldSetup] Main Camera not found");
            return;
        }

        // RPG 스타일 탑다운 카메라
        cam.orthographic     = false;
        cam.fieldOfView      = 55f;
        cam.nearClipPlane    = 0.1f;
        cam.farClipPlane     = 100f;
        cam.clearFlags       = CameraClearFlags.Skybox;

        // 씬 중앙 위에서 비스듬히 내려다보기 (고양이와 비밀레시피 스타일)
        cam.transform.position = new Vector3(0f, 12f, -8f);
        cam.transform.rotation = Quaternion.Euler(52f, 0f, 0f);

        EditorUtility.SetDirty(cam);
        Debug.Log("[GameWorldSetup] 카메라 설정 완료");
    }

    // ─────────────────────────────────────────────────────────
    // 2. 플레이어 캐릭터
    // ─────────────────────────────────────────────────────────
    static void SetupPlayer()
    {
        // 씬의 실제 캐릭터 오브젝트 탐색 (MomBody 우선)
        GameObject player = FindInHierarchy("MomBody")
                         ?? FindInHierarchy("ChildBody")
                         ?? FindInHierarchy("character-a");

        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "PlayerCharacter";
            player.transform.position = new Vector3(-1f, 0.5f, 0f);
            var rend = player.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = new Color(0.95f, 0.75f, 0.4f);
                rend.material = mat;
            }
            Debug.Log("[GameWorldSetup] 캐릭터 없어서 캡슐 생성");
        }

        // 플레이어 컴포넌트
        if (player.GetComponent<Rigidbody>() == null)
        {
            var rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.mass = 1f;
        }

        if (player.GetComponent<CapsuleCollider>() == null)
            player.AddComponent<CapsuleCollider>();

        var hand = player.GetComponent<PlayerHand>() ?? player.AddComponent<PlayerHand>();
        var ctrl = player.GetComponent<PlayerController>() ?? player.AddComponent<PlayerController>();
        ctrl.moveSpeed = 4f;

        EditorUtility.SetDirty(player);
        Debug.Log($"[GameWorldSetup] 플레이어 설정: {player.name}");
    }

    // ─────────────────────────────────────────────────────────
    // 3. 스테이션: 농장 / 조리대 / 서빙 카운터
    // ─────────────────────────────────────────────────────────
    static void SetupStations()
    {
        // ── 농장 스테이션 (FarmBox1 또는 Tomato1) ────────
        GameObject farm = FindInHierarchy("FarmBox1")
                       ?? FindInHierarchy("Tomato1")
                       ?? FindInHierarchy("FarmSoil1")
                       ?? FindInHierarchy("tomato");

        if (farm == null)
        {
            farm = CreateStationMarker("FarmStation_Marker", new Vector3(-3f, 0f, 3f),
                                       new Color(0.25f, 0.70f, 0.25f));
            Debug.Log("[GameWorldSetup] 농장 마커 생성");
        }
        EnsureStation<FarmStation>(farm);
        EnsureBoxCollider(farm, new Vector3(1.5f, 1f, 1.5f));

        // ── 조리대 스테이션 (Grill 우선) ─────────────────
        GameObject stove = FindInHierarchy("Grill")
                        ?? FindInHierarchy("KitchenIsland")
                        ?? FindInHierarchy("Stove");

        if (stove == null)
        {
            stove = CreateStationMarker("CookStation_Marker", new Vector3(1f, 0f, 2f),
                                        new Color(0.85f, 0.45f, 0.10f));
            Debug.Log("[GameWorldSetup] 조리대 마커 생성");
        }
        EnsureStation<CookStation>(stove);
        EnsureBoxCollider(stove, new Vector3(1.5f, 1f, 1.5f));

        // ── 서빙 카운터 (KitchenCounter 우선) ────────────
        GameObject counter = FindInHierarchy("KitchenCounter")
                          ?? FindInHierarchy("CounterTop")
                          ?? FindInHierarchy("Sink");

        if (counter == null)
        {
            counter = CreateStationMarker("ServeCounter_Marker", new Vector3(2f, 0f, -2f),
                                          new Color(0.25f, 0.52f, 0.88f));
            Debug.Log("[GameWorldSetup] 서빙 카운터 마커 생성");
        }
        EnsureStation<ServeCounter>(counter);
        EnsureBoxCollider(counter, new Vector3(1.5f, 1f, 1.5f));

        Debug.Log("[GameWorldSetup] 스테이션 설정 완료");
    }

    static T EnsureStation<T>(GameObject go) where T : Component
        => go.GetComponent<T>() ?? go.AddComponent<T>();

    static void EnsureBoxCollider(GameObject go, Vector3 size)
    {
        // 기존 Collider 있으면 그대로 사용
        if (go.GetComponentInChildren<Collider>() != null) return;
        var col = go.AddComponent<BoxCollider>();
        col.size = size;
        col.center = new Vector3(0f, size.y * 0.5f, 0f);
    }

    // ─────────────────────────────────────────────────────────
    // 4. HUD (Canvas 위 투명 오버레이)
    // ─────────────────────────────────────────────────────────
    static void SetupHUD()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("[GameWorldSetup] Canvas not found"); return; }

        var uiRoot = canvas.transform.Find("UIRoot")?.gameObject;
        if (uiRoot == null) { Debug.LogError("[GameWorldSetup] UIRoot not found. Run 'Rebuild UI From Scratch' first."); return; }

        // InGamePanel 찾기 또는 새로 만들기
        var inGamePanel = uiRoot.transform.Find("InGamePanel")?.gameObject;
        if (inGamePanel == null)
        {
            inGamePanel = NewStretchPanel(uiRoot, "InGamePanel", new Color(0, 0, 0, 0));
            Debug.Log("[GameWorldSetup] InGamePanel 새로 생성");
        }
        else
        {
            // 배경 투명하게
            var img = inGamePanel.GetComponent<Image>();
            if (img != null) img.color = new Color(0, 0, 0, 0);
        }
        inGamePanel.SetActive(false);

        // 기존 자식 모두 삭제 후 HUD 재구성
        DestroyChildren(inGamePanel);
        BuildHUDContent(inGamePanel);
    }

    static void BuildHUDContent(GameObject panel)
    {
        // ── 상단 바: 버거 카운트 ──────────────────────────
        var topBar = NewUIGO(panel, "TopBar");
        var topRT  = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 0.88f);
        topRT.anchorMax = new Vector2(1f, 1.00f);
        topRT.offsetMin = Vector2.zero;
        topRT.offsetMax = Vector2.zero;
        var topImg = topBar.AddComponent<Image>();
        topImg.color = COL_HUD_BG; topImg.sprite = null; topImg.type = Image.Type.Simple;

        var countGO = NewUIGO(topBar, "BurgerCount");
        StretchFull(countGO);
        MakeTMP(countGO, "버거 0 / 100", 26, COL_ORANGE, FontStyles.Bold);

        // ── 하단 중앙: 상호작용 프롬프트 ─────────────────
        var promptBox = NewUIGO(panel, "PromptBox");
        var pbRT = promptBox.GetComponent<RectTransform>();
        pbRT.anchorMin = new Vector2(0.25f, 0.05f);
        pbRT.anchorMax = new Vector2(0.75f, 0.14f);
        pbRT.offsetMin = Vector2.zero;
        pbRT.offsetMax = Vector2.zero;
        var pbImg = promptBox.AddComponent<Image>();
        pbImg.color = COL_HUD_BG; pbImg.sprite = null; pbImg.type = Image.Type.Simple;

        var promptGO = NewUIGO(promptBox, "PromptText");
        StretchFull(promptGO);
        MakeTMP(promptGO, "", 22, COL_WHITE, FontStyles.Bold);

        // ── 좌측 하단: 손에 든 아이템 ────────────────────
        var itemBox = NewUIGO(panel, "ItemBox");
        var ibRT = itemBox.GetComponent<RectTransform>();
        ibRT.anchorMin = new Vector2(0.02f, 0.05f);
        ibRT.anchorMax = new Vector2(0.24f, 0.14f);
        ibRT.offsetMin = Vector2.zero;
        ibRT.offsetMax = Vector2.zero;
        var ibImg = itemBox.AddComponent<Image>();
        ibImg.color = COL_HUD_BG; ibImg.sprite = null; ibImg.type = Image.Type.Simple;

        var itemGO = NewUIGO(itemBox, "HeldItemText");
        StretchFull(itemGO);
        MakeTMP(itemGO, "", 20, new Color(0.9f, 1f, 0.7f), FontStyles.Bold);

        // ── 좌측 중단: 조리 진행 바 ──────────────────────
        var cookBarBg = NewUIGO(panel, "CookProgressBg");
        var cbBgRT = cookBarBg.GetComponent<RectTransform>();
        cbBgRT.anchorMin = new Vector2(0.02f, 0.16f);
        cbBgRT.anchorMax = new Vector2(0.32f, 0.22f);
        cbBgRT.offsetMin = Vector2.zero;
        cbBgRT.offsetMax = Vector2.zero;
        var cbBgImg = cookBarBg.AddComponent<Image>();
        cbBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        cbBgImg.sprite = null; cbBgImg.type = Image.Type.Simple;
        cookBarBg.SetActive(false);

        var cookFillGO = NewUIGO(cookBarBg, "Fill");
        StretchFull(cookFillGO);
        var fillImg = cookFillGO.AddComponent<Image>();
        fillImg.color = COL_ORANGE;
        fillImg.sprite = null;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;

        var cookLabelGO = NewUIGO(cookBarBg, "Label");
        StretchFull(cookLabelGO);
        MakeTMP(cookLabelGO, "조리 중...", 18, COL_WHITE, FontStyles.Bold);

        // ── 우측 상단: 나가기 버튼 ───────────────────────
        var backBtn = NewButton(panel, "Btn_BackToMenu", "나가기", 18, COL_RED);
        var bbRT = backBtn.GetComponent<RectTransform>();
        bbRT.anchorMin = new Vector2(0.82f, 0.90f);
        bbRT.anchorMax = new Vector2(0.98f, 0.99f);
        bbRT.offsetMin = Vector2.zero;
        bbRT.offsetMax = Vector2.zero;

        // ── InGameHUD 컴포넌트 연결 ───────────────────────
        var hud = panel.GetComponent<InGameHUD>() ?? panel.AddComponent<InGameHUD>();
        hud.burgerCountText  = countGO.GetComponent<TMP_Text>();
        hud.promptText       = promptGO.GetComponent<TMP_Text>();
        hud.heldItemText     = itemGO.GetComponent<TMP_Text>();
        hud.cookProgressBar  = cookBarBg;
        hud.cookFillImage    = fillImg;
        hud.backButton       = backBtn.GetComponent<Button>();
        EditorUtility.SetDirty(hud);

        // 나가기 버튼 연결
        WireButton(backBtn, hud, "OnClick_BackToMenu");

        Debug.Log("[GameWorldSetup] HUD 구성 완료");
    }

    // ─────────────────────────────────────────────────────────
    // 5. GameManager 확인
    // ─────────────────────────────────────────────────────────
    static void EnsureGameManager()
    {
        if (GameObject.Find("GameManager") == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            Debug.Log("[GameWorldSetup] GameManager 생성");
        }
    }

    // ─────────────────────────────────────────────────────────
    // 헬퍼
    // ─────────────────────────────────────────────────────────

    static GameObject FindInHierarchy(string name)
    {
        var found = GameObject.Find(name);
        if (found != null) return found;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var t = root.transform.Find(name);
            if (t != null) return t.gameObject;
            // deep search
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child.gameObject;
        }
        return null;
    }

    static GameObject CreateStationMarker(string name, Vector3 pos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            rend.sharedMaterial = mat;
        }
        return go;
    }

    static void DestroyChildren(GameObject parent)
    {
        var list = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < parent.transform.childCount; i++)
            list.Add(parent.transform.GetChild(i).gameObject);
        foreach (var c in list) Object.DestroyImmediate(c);
    }

    // ── UI 헬퍼 ──────────────────────────────────────────────

    static GameObject NewStretchPanel(GameObject parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.layer = parent.layer;
        var rt = go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = bg; img.sprite = null; img.type = Image.Type.Simple;
        return go;
    }

    static GameObject NewUIGO(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.layer = parent.layer;
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static TMP_Text MakeTMP(GameObject go, string text, int size, Color color, FontStyles style)
    {
        var t = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = style; t.alignment = TextAlignmentOptions.Center;
        if (_font != null) t.font = _font;
        return t;
    }

    static GameObject NewButton(GameObject parent, string name, string label, int fontSize, Color color)
    {
        var go = NewUIGO(parent, name);
        var img = go.AddComponent<Image>();
        img.color = color; img.sprite = null; img.type = Image.Type.Simple;
        go.AddComponent<Button>();
        var textGO = NewUIGO(go, "Label");
        StretchFull(textGO);
        MakeTMP(textGO, label, fontSize, COL_WHITE, FontStyles.Bold);
        return go;
    }

    static void WireButton(GameObject go, Object target, string method)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>(); if (btn == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.ClearArray();
        so.ApplyModifiedProperties();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            System.Delegate.CreateDelegate(typeof(UnityAction), target,
                target.GetType().GetMethod(method)) as UnityAction);
        EditorUtility.SetDirty(go);
    }
}
