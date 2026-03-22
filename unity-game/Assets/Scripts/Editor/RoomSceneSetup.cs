using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// 방 만들기/참여 UI 및 씬 배경을 자동으로 구성합니다.
/// Menu: Tools → Setup Room Scene
/// </summary>
public static class RoomSceneSetup
{
    // ── 색상 팔레트 (CasualUISetup과 동일) ─────────────────────
    static readonly Color COL_GREEN  = new Color(0.25f, 0.72f, 0.35f);
    static readonly Color COL_BLUE   = new Color(0.25f, 0.55f, 0.90f);
    static readonly Color COL_RED    = new Color(0.90f, 0.30f, 0.25f);
    static readonly Color COL_ORANGE = new Color(0.95f, 0.60f, 0.15f);
    static readonly Color COL_TEAL   = new Color(0.15f, 0.55f, 0.65f, 0.94f);
    static readonly Color COL_DARK   = new Color(0.08f, 0.10f, 0.15f, 1.00f); // 완전 불투명
    static readonly Color COL_WHITE  = Color.white;

    static Sprite       _uiSprite;
    static TMP_FontAsset _korFont;

    [MenuItem("Tools/Setup Room Scene")]
    public static void Setup()
    {
        _uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        _korFont  = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");

        // 0. UIRoot 전체 화면 stretch 보정 (100×100 고정 앵커 버그 방지)
        FixUIRootRect();

        // 1. RoomManager 싱글톤 생성
        SetupRoomManager();

        // 2. 카메라 배경 색상
        SetupCamera();

        // 3. 씬 조명
        SetupLighting();

        // 4. MainMenuPanel 수정
        SetupMainMenuPanel();

        // 5. LobbyPanel 수정
        SetupLobbyPanel();

        // 6. 씬 저장
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[RoomSceneSetup] 씬 구성 완료!");
    }

    // ── 0. UIRoot 수정 ────────────────────────────────────────

    static void FixUIRootRect()
    {
        var uiRoot = FindGO("Canvas/UIRoot");
        if (uiRoot == null) return;
        var rt = uiRoot.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        EditorUtility.SetDirty(uiRoot);
        Debug.Log($"[RoomSceneSetup] UIRoot rect 보정: {rt.rect.width}x{rt.rect.height}");
    }

    // ── 1. RoomManager ────────────────────────────────────────

    static void SetupRoomManager()
    {
        var existing = GameObject.Find("RoomManager");
        if (existing != null)
        {
            if (existing.GetComponent<RoomManager>() == null)
                existing.AddComponent<RoomManager>();
            Debug.Log("[RoomSceneSetup] RoomManager: 기존 오브젝트 재사용");
            return;
        }

        var go = new GameObject("RoomManager");
        go.AddComponent<RoomManager>();
        Debug.Log("[RoomSceneSetup] RoomManager 생성");
    }

    // ── 2. 카메라 배경 ──────────────────────────────────────────

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.55f, 0.75f, 0.90f); // 하늘색
        EditorUtility.SetDirty(cam);
        Debug.Log("[RoomSceneSetup] 카메라 배경색 설정");
    }

    // ── 3. 조명 ────────────────────────────────────────────────

    static void SetupLighting()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.color     = new Color(1.0f, 0.95f, 0.82f); // 따뜻한 햇빛
                l.intensity = 1.2f;
                l.transform.eulerAngles = new Vector3(50f, -30f, 0f);
                EditorUtility.SetDirty(l);
                Debug.Log("[RoomSceneSetup] Directional Light 색상 조정");
            }
        }
        // 앰비언트 라이트
        RenderSettings.ambientLight = new Color(0.45f, 0.50f, 0.60f);
    }

    // ── 4. MainMenuPanel ──────────────────────────────────────

    static void SetupMainMenuPanel()
    {
        var mainPanel = FindGO("Canvas/UIRoot/MainMenuPanel");
        if (mainPanel == null) return;

        var menuContainer = FindGO("Canvas/UIRoot/MainMenuPanel/MenuContainer");

        // ─ 타이틀: MainMenuPanel 직속 자식, 화면 상단에 고정 ─
        var titleGO = FindChildOrCreate(mainPanel, "TitleText");
        var titleRT  = titleGO.GetComponent<RectTransform>() ?? titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 1f);   // top-left
        titleRT.anchorMax        = new Vector2(1f, 1f);   // top-right
        titleRT.pivot            = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -60f); // 60px 아래
        titleRT.sizeDelta        = new Vector2(0f, 80f);  // 전체 폭, 높이 80
        EnsureTMP(titleGO, "100 버거 패밀리", 46, COL_WHITE, FontStyles.Bold);
        titleGO.transform.SetSiblingIndex(0); // MenuContainer 이전

        // ─ MenuContainer 크기·위치 정돈 ─
        var mcRT = menuContainer.GetComponent<RectTransform>();
        mcRT.anchorMin        = new Vector2(0.5f, 0.5f);
        mcRT.anchorMax        = new Vector2(0.5f, 0.5f);
        mcRT.pivot            = new Vector2(0.5f, 0.5f);
        mcRT.anchoredPosition = new Vector2(0f, 0f);      // 화면 정중앙
        mcRT.sizeDelta        = new Vector2(440f, 200f);
        EditorUtility.SetDirty(menuContainer);

        // ─ CreateRoomButton 위치/텍스트 ─
        var createBtn = FindGO("Canvas/UIRoot/MainMenuPanel/MenuContainer/CreateRoomButton");
        if (createBtn != null)
        {
            var rt = createBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(380f, 80f);
            rt.anchoredPosition = new Vector2(0f, 50f);
            EditorUtility.SetDirty(createBtn);
        }
        SetChildTMP("Canvas/UIRoot/MainMenuPanel/MenuContainer/CreateRoomButton", "방 만들기", 30, COL_WHITE);

        // ─ JoinRoomButton 위치/텍스트 ─
        var joinBtn = FindGO("Canvas/UIRoot/MainMenuPanel/MenuContainer/JoinRoomButton");
        if (joinBtn != null)
        {
            var rt = joinBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(380f, 80f);
            rt.anchoredPosition = new Vector2(0f, -50f);
            EditorUtility.SetDirty(joinBtn);
        }
        SetChildTMP("Canvas/UIRoot/MainMenuPanel/MenuContainer/JoinRoomButton", "참여하기", 30, COL_WHITE);

        // ─ RoleSelectPanel ─
        SetupRoleSelectPanel(mainPanel, menuContainer);

        // ─ JoinPanel ─
        SetupJoinPanel(mainPanel, menuContainer);

        // ─ MainMenuController 연결 ─
        var ctrl = mainPanel.GetComponent<MainMenuController>()
                ?? mainPanel.AddComponent<MainMenuController>();

        ctrl.roleSelectPanel = FindChildGO(mainPanel, "RoleSelectPanel");
        ctrl.joinPanel       = FindChildGO(mainPanel, "JoinPanel");
        ctrl.joinCodeInput   = FindDeep(mainPanel, "Input_RoomCode")?.GetComponent<TMP_InputField>();
        ctrl.joinErrorText   = FindDeep(mainPanel, "Text_JoinError")?.GetComponent<TMP_Text>();
        EditorUtility.SetDirty(ctrl);

        // ─ 버튼 OnClick 연결 ─
        WireButtonClick("Canvas/UIRoot/MainMenuPanel/MenuContainer/CreateRoomButton",
            ctrl, "OnClick_CreateRoom");
        WireButtonClick("Canvas/UIRoot/MainMenuPanel/MenuContainer/JoinRoomButton",
            ctrl, "OnClick_ShowJoin");

        // ─ 오버레이가 항상 최상위 sibling이 되도록 강제 ─
        var rolePanel = FindChildGO(mainPanel, "RoleSelectPanel");
        var joinPanel = FindChildGO(mainPanel, "JoinPanel");
        if (rolePanel != null) rolePanel.transform.SetAsLastSibling();
        if (joinPanel  != null) joinPanel.transform.SetAsLastSibling();
    }

    static void SetupRoleSelectPanel(GameObject mainPanel, GameObject menuContainer)
    {
        var panel = FindChildOrCreate(mainPanel, "RoleSelectPanel");
        StyleOverlay(panel, COL_DARK);
        panel.transform.SetAsLastSibling(); // MenuContainer보다 위에 렌더링
        panel.SetActive(false);

        // 타이틀
        var title = FindChildOrCreate(panel, "Title");
        EnsureTMP(title, "역할을 선택하세요", 30, COL_WHITE, FontStyles.Bold);
        SetAnchoredPos(title, 0f, 80f, 500f, 50f);

        // 부모 버튼
        var btnParent = FindChildOrCreate(panel, "Btn_RoleParent");
        StyleButton(btnParent, "부모로 시작", 26, COL_GREEN, 240f, 60f, 0f, 10f);

        // 자녀 버튼
        var btnChild = FindChildOrCreate(panel, "Btn_RoleChild");
        StyleButton(btnChild, "자녀로 시작", 26, COL_BLUE, 240f, 60f, 0f, -60f);

        // 취소 버튼
        var btnCancel = FindChildOrCreate(panel, "Btn_Cancel");
        StyleButton(btnCancel, "취소", 22, COL_RED, 180f, 50f, 0f, -140f);

        // MainMenuController는 나중에 연결 (SetupMainMenuPanel 에서)
    }

    static void SetupJoinPanel(GameObject mainPanel, GameObject menuContainer)
    {
        var panel = FindChildOrCreate(mainPanel, "JoinPanel");
        StyleOverlay(panel, COL_DARK);
        panel.transform.SetAsLastSibling(); // MenuContainer보다 위에 렌더링
        panel.SetActive(false);

        // 타이틀
        var title = FindChildOrCreate(panel, "Title");
        EnsureTMP(title, "방 코드를 입력하세요", 28, COL_WHITE, FontStyles.Bold);
        SetAnchoredPos(title, 0f, 130f, 440f, 52f);

        // InputField
        var inputGO = FindChildOrCreate(panel, "Input_RoomCode");
        var inputField = inputGO.GetComponent<TMP_InputField>()
                      ?? inputGO.AddComponent<TMP_InputField>();
        SetAnchoredPos(inputGO, 0f, 45f, 320f, 58f);

        // InputField 배경
        var bg = inputGO.GetComponent<Image>() ?? inputGO.AddComponent<Image>();
        bg.sprite = _uiSprite;
        bg.color  = new Color(0.9f, 0.95f, 1f);
        bg.type   = Image.Type.Sliced;

        // InputField Text child
        var textChild = FindChildOrCreate(inputGO, "Text");
        var tmp = textChild.GetComponent<TextMeshProUGUI>()
               ?? textChild.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = 26;
        tmp.color     = new Color(0.1f, 0.1f, 0.2f);
        var rt = textChild.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(8, 4);
        rt.offsetMax = new Vector2(-8, -4);
        inputField.textComponent = tmp;

        // placeholder
        var phGO = FindChildOrCreate(inputGO, "Placeholder");
        var ph   = phGO.GetComponent<TextMeshProUGUI>() ?? phGO.AddComponent<TextMeshProUGUI>();
        ph.text      = "1234";
        ph.fontSize  = 26;
        ph.color     = new Color(0.6f, 0.6f, 0.7f);
        ph.fontStyle = FontStyles.Italic;
        var phRt = phGO.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(8, 4);
        phRt.offsetMax = new Vector2(-8, -4);
        inputField.placeholder = ph;

        // 에러 텍스트
        var errGO = FindChildOrCreate(panel, "Text_JoinError");
        var errTMP = EnsureTMP(errGO, "", 18, new Color(1f, 0.5f, 0.4f), FontStyles.Normal);
        SetAnchoredPos(errGO, 0f, -10f, 400f, 38f);

        // 참여 버튼
        var btnConfirm = FindChildOrCreate(panel, "Btn_JoinConfirm");
        StyleButton(btnConfirm, "참여하기", 26, COL_GREEN, 200f, 60f, 70f, -80f);

        // 취소 버튼
        var btnCancel = FindChildOrCreate(panel, "Btn_JoinCancel");
        StyleButton(btnCancel, "취소", 22, COL_RED, 160f, 54f, -80f, -80f);
    }

    // ── 5. LobbyPanel ─────────────────────────────────────────

    static void SetupLobbyPanel()
    {
        var lobbyPanel = FindGO("Canvas/UIRoot/LobbyPanel");
        if (lobbyPanel == null) return;

        var container = FindGO("Canvas/UIRoot/LobbyPanel/LobbyContainer");

        // ─ 방 코드 텍스트 ─
        var codeGO = FindChildOrCreate(container, "Text_RoomCode");
        EnsureTMP(codeGO, "방 코드: ----", 24, COL_WHITE, FontStyles.Bold);
        SetAnchoredPos(codeGO, 0f, 80f, 400f, 40f);

        // ─ 내 역할 텍스트 ─
        var roleGO = FindChildOrCreate(container, "Text_MyRole");
        EnsureTMP(roleGO, "내 역할: -", 20, new Color(0.8f, 1f, 0.8f), FontStyles.Normal);
        SetAnchoredPos(roleGO, 0f, 35f, 400f, 34f);

        // ─ 플레이어 슬롯 구분선 ─
        var slotContainer = FindChildOrCreate(container, "PlayerSlots");
        StyleSlotContainer(slotContainer);

        var slotParent = FindChildOrCreate(slotContainer, "Slot_Parent");
        EnsureTMP(slotParent, "부모  대기 중...", 22, new Color(1f, 0.9f, 0.6f), FontStyles.Normal);
        SetAnchoredPos(slotParent, 0f, 20f, 360f, 36f);

        var slotChild = FindChildOrCreate(slotContainer, "Slot_Child");
        EnsureTMP(slotChild, "자녀  대기 중...", 22, new Color(0.7f, 0.9f, 1f), FontStyles.Normal);
        SetAnchoredPos(slotChild, 0f, -20f, 360f, 36f);

        // ─ 기존 버튼 텍스트 갱신 ─
        SetChildTMP("Canvas/UIRoot/LobbyPanel/LobbyContainer/StartButton", "시작!", 30, COL_WHITE);
        SetChildTMP("Canvas/UIRoot/LobbyPanel/LobbyContainer/BackButton",  "나가기", 24, COL_WHITE);

        // ─ LobbyController 연결 ─
        var ctrl = lobbyPanel.GetComponent<LobbyController>()
                ?? lobbyPanel.AddComponent<LobbyController>();

        ctrl.roomCodeText   = codeGO.GetComponent<TMP_Text>();
        ctrl.myRoleText     = roleGO.GetComponent<TMP_Text>();
        ctrl.slotParentText = slotParent.GetComponent<TMP_Text>();
        ctrl.slotChildText  = slotChild.GetComponent<TMP_Text>();
        ctrl.startButton    = FindGO("Canvas/UIRoot/LobbyPanel/LobbyContainer/StartButton")?.GetComponent<Button>();
        EditorUtility.SetDirty(ctrl);

        // ─ 버튼 OnClick 연결 ─
        WireButtonClick("Canvas/UIRoot/LobbyPanel/LobbyContainer/StartButton", ctrl, "OnClick_Start");
        WireButtonClick("Canvas/UIRoot/LobbyPanel/LobbyContainer/BackButton",  ctrl, "OnClick_Back");
    }

    // ── UI 헬퍼 ──────────────────────────────────────────────

    static void StyleOverlay(GameObject go, Color bg)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite        = _uiSprite;
        img.color         = bg;
        img.type          = Image.Type.Simple; // Simple → sprite 없어도 색상 렌더링
        img.raycastTarget = true;
        EditorUtility.SetDirty(go);
    }

    static void StyleSlotContainer(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(380f, 90f);
        rt.anchoredPosition = new Vector2(0f, -30f);

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite = _uiSprite;
        img.color  = new Color(0f, 0f, 0f, 0.25f);
        img.type   = Image.Type.Sliced;
        img.raycastTarget = false;
    }

    static void StyleButton(GameObject go, string label, int fontSize,
                            Color color, float w, float h, float offsetX, float offsetY)
    {
        // RectTransform
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(offsetX, offsetY);

        // Image
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite = _uiSprite;
        img.color  = color;
        img.type   = Image.Type.Sliced;

        // Button
        if (go.GetComponent<Button>() == null) go.AddComponent<Button>();

        // Text child
        var textGO = FindChildOrCreate(go, "Label");
        var t = textGO.GetComponent<TextMeshProUGUI>() ?? textGO.AddComponent<TextMeshProUGUI>();
        t.text      = label;
        t.fontSize  = fontSize;
        t.color     = Color.white;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        if (_korFont != null) t.font = _korFont; // 한국어 폰트 할당

        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    static TMP_Text EnsureTMP(GameObject go, string text, int size, Color color, FontStyles style)
    {
        if (go.GetComponent<RectTransform>() == null)
            go.AddComponent<RectTransform>();

        var t = go.GetComponent<TextMeshProUGUI>()
             ?? go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.color     = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        if (_korFont != null) t.font = _korFont;
        return t;
    }

    static void SetAnchoredPos(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    static void StretchRect(RectTransform rt, float ax, float ay, float bx, float by,
                            float offX, float offY)
    {
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(0, offX);
        rt.offsetMax = new Vector2(0, offY);
    }

    // 기존 버튼의 자식 TMP_Text 텍스트만 바꿈
    static void SetChildTMP(string path, string text, int size, Color color)
    {
        var go = FindGO(path); if (go == null) return;
        var t  = go.GetComponentInChildren<TMP_Text>(); if (t == null) return;
        t.text     = text;
        t.fontSize = size;
        t.color    = color;
        t.fontStyle = FontStyles.Bold;
    }

    // ── OnClick 연결 ──────────────────────────────────────────

    static void WireButtonClick(string path, Object target, string methodName)
    {
        var go  = FindGO(path); if (go == null) return;
        var btn = go.GetComponent<Button>(); if (btn == null) return;

        // 기존 persistent 리스너 전부 제거 (중복/충돌 방지)
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.ClearArray();
        so.ApplyModifiedProperties();

        // 새 리스너 추가
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            System.Delegate.CreateDelegate(
                typeof(UnityAction),
                target,
                target.GetType().GetMethod(methodName)
            ) as UnityAction
        );
        EditorUtility.SetDirty(go);
    }

    // ── GameObject 검색 헬퍼 ─────────────────────────────────

    static GameObject FindGO(string path)
    {
        var go = GameObject.Find(path);
        if (go != null) return go;

        var parts = path.Split('/');
        foreach (var root in UnityEngine.SceneManagement.SceneManager
                     .GetActiveScene().GetRootGameObjects())
        {
            if (root.name != parts[0]) continue;
            Transform t = root.transform;
            bool ok = true;
            for (int i = 1; i < parts.Length; i++)
            {
                t = t.Find(parts[i]);
                if (t == null) { ok = false; break; }
            }
            if (ok) return t.gameObject;
        }
        Debug.LogWarning($"[RoomSceneSetup] Not found: {path}");
        return null;
    }

    static GameObject FindChildOrCreate(GameObject parent, string childName)
    {
        if (parent == null) return new GameObject(childName);
        var t = parent.transform.Find(childName);
        if (t != null)
        {
            // 기존 오브젝트에도 RectTransform 보장
            var existing = t.gameObject;
            if (parent.GetComponent<RectTransform>() != null && existing.GetComponent<RectTransform>() == null)
                existing.AddComponent<RectTransform>();
            return existing;
        }
        var child = new GameObject(childName);
        child.layer = parent.layer;
        // UI 계층 안이면 RectTransform을 먼저 추가 (SetParent 전에)
        if (parent.GetComponent<RectTransform>() != null)
            child.AddComponent<RectTransform>();
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    static GameObject FindChildGO(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        return t != null ? t.gameObject : null;
    }

    static GameObject FindDeep(GameObject root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }
}
