using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// UI를 완전히 삭제하고 처음부터 새로 만듭니다.
/// Menu: Tools/Rebuild UI From Scratch
/// </summary>
public static class RebuildUI
{
    // ── 색상 ──────────────────────────────────────────────────
    static readonly Color COL_BG_MAIN  = new Color(0.13f, 0.20f, 0.35f);
    static readonly Color COL_BG_LOBBY = new Color(0.10f, 0.18f, 0.28f);
    static readonly Color COL_OVERLAY  = new Color(0.05f, 0.08f, 0.15f, 0.96f);
    static readonly Color COL_GREEN    = new Color(0.22f, 0.70f, 0.32f);
    static readonly Color COL_BLUE     = new Color(0.22f, 0.52f, 0.88f);
    static readonly Color COL_RED      = new Color(0.88f, 0.28f, 0.22f);
    static readonly Color COL_ORANGE   = new Color(0.95f, 0.58f, 0.12f);
    static readonly Color COL_GRAY     = new Color(0.40f, 0.42f, 0.48f);
    static readonly Color COL_WHITE    = Color.white;
    static readonly Color COL_LIGHT    = new Color(0.85f, 0.92f, 1.00f);

    static TMP_FontAsset _font;

    // ─────────────────────────────────────────────────────────
    [MenuItem("Tools/Rebuild UI From Scratch")]
    public static void Rebuild()
    {
        // 폰트 로드
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");
        if (_font == null)
            Debug.LogWarning("[RebuildUI] MalgunGothic SDF.asset 없음 — 기본 폰트로 진행");

        // Canvas 찾기
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("[RebuildUI] Canvas not found"); return; }

        // UIRoot: 없으면 생성, 있으면 stretch 보정
        var uiRoot = EnsureUIRoot(canvas);

        // UIRoot 자식 전부 삭제
        DestroyAllChildren(uiRoot);

        // ─ 패널 4개 생성 ─
        var mainPanel   = BuildMainMenuPanel(uiRoot);
        var lobbyPanel  = BuildLobbyPanel(uiRoot);
        var inGamePanel = BuildInGamePanel(uiRoot);
        var resultPanel = BuildResultPanel(uiRoot);

        // ─ UIScreenController ─
        SetupUIScreenController(canvas, mainPanel, lobbyPanel, inGamePanel, resultPanel);

        // ─ RoomManager ─
        EnsureRoomManager();

        // ─ 씬 저장 ─
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[RebuildUI] 완료!");
    }

    // ═══════════════════════════════════════════════════════
    // UIRoot
    // ═══════════════════════════════════════════════════════

    static GameObject EnsureUIRoot(GameObject canvas)
    {
        var t = canvas.transform.Find("UIRoot");
        GameObject uiRoot = t != null ? t.gameObject : null;
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UIRoot");
            uiRoot.AddComponent<RectTransform>();
            uiRoot.transform.SetParent(canvas.transform, false);
        }
        var rt = uiRoot.GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        EditorUtility.SetDirty(uiRoot);
        return uiRoot;
    }

    static void DestroyAllChildren(GameObject parent)
    {
        var children = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < parent.transform.childCount; i++)
            children.Add(parent.transform.GetChild(i).gameObject);
        foreach (var c in children)
            Object.DestroyImmediate(c);
    }

    // ═══════════════════════════════════════════════════════
    // MainMenuPanel
    // ═══════════════════════════════════════════════════════

    static GameObject BuildMainMenuPanel(GameObject uiRoot)
    {
        var panel = NewStretchPanel(uiRoot, "MainMenuPanel", COL_BG_MAIN);

        // ── 타이틀 (화면 상단 15~25% 영역) ──────────────────
        var titleGO = NewGO(panel, "TitleText");
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 0.72f);
        titleRT.anchorMax        = new Vector2(1f, 0.90f);
        titleRT.offsetMin        = Vector2.zero;
        titleRT.offsetMax        = Vector2.zero;
        MakeTMP(titleGO, "100 버거 패밀리", 52, COL_ORANGE, FontStyles.Bold);

        // ── 부제목 ────────────────────────────────────────
        var subGO = NewGO(panel, "SubText");
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin  = new Vector2(0f, 0.63f);
        subRT.anchorMax  = new Vector2(1f, 0.72f);
        subRT.offsetMin  = Vector2.zero;
        subRT.offsetMax  = Vector2.zero;
        MakeTMP(subGO, "가족이 함께 만드는 버거!", 22, COL_LIGHT, FontStyles.Normal);

        // ── 버튼 컨테이너 (화면 중앙) ─────────────────────
        var btnCreateGO = NewButton(panel, "Btn_CreateRoom", "방 만들기", 34, COL_GREEN);
        var btncRT = btnCreateGO.GetComponent<RectTransform>();
        btncRT.anchorMin        = new Vector2(0.5f, 0.5f);
        btncRT.anchorMax        = new Vector2(0.5f, 0.5f);
        btncRT.pivot            = new Vector2(0.5f, 0.5f);
        btncRT.sizeDelta        = new Vector2(400f, 90f);
        btncRT.anchoredPosition = new Vector2(0f, 30f);

        var btnJoinGO = NewButton(panel, "Btn_JoinRoom", "참여하기", 34, COL_BLUE);
        var btnJRT = btnJoinGO.GetComponent<RectTransform>();
        btnJRT.anchorMin        = new Vector2(0.5f, 0.5f);
        btnJRT.anchorMax        = new Vector2(0.5f, 0.5f);
        btnJRT.pivot            = new Vector2(0.5f, 0.5f);
        btnJRT.sizeDelta        = new Vector2(400f, 90f);
        btnJRT.anchoredPosition = new Vector2(0f, -80f);

        // ── RoleSelectPanel (오버레이) ─────────────────────
        var rolePanel = BuildRoleSelectPanel(panel);

        // ── JoinPanel (오버레이) ──────────────────────────
        var joinPanel = BuildJoinPanel(panel);

        // ── MainMenuController 연결 ────────────────────────
        var ctrl = panel.GetComponent<MainMenuController>()
                ?? panel.AddComponent<MainMenuController>();
        ctrl.roleSelectPanel = rolePanel;
        ctrl.joinPanel       = joinPanel;
        ctrl.joinCodeInput   = joinPanel.transform.Find("InputContainer/Input_RoomCode")
                                       ?.GetComponent<TMP_InputField>();
        ctrl.joinErrorText   = joinPanel.transform.Find("Text_JoinError")
                                       ?.GetComponent<TMP_Text>();
        EditorUtility.SetDirty(ctrl);

        // ── 버튼 연결 ─────────────────────────────────────
        WireButton(btnCreateGO, ctrl, "OnClick_CreateRoom");
        WireButton(btnJoinGO,   ctrl, "OnClick_ShowJoin");
        WireButton(FindDeep(rolePanel, "Btn_RoleParent"),  ctrl, "OnClick_RoleParent");
        WireButton(FindDeep(rolePanel, "Btn_RoleChild"),   ctrl, "OnClick_RoleChild");
        WireButton(FindDeep(rolePanel, "Btn_CancelRole"),  ctrl, "OnClick_CancelRole");
        WireButton(FindDeep(joinPanel,  "Btn_JoinConfirm"), ctrl, "OnClick_JoinConfirm");
        WireButton(FindDeep(joinPanel,  "Btn_CancelJoin"),  ctrl, "OnClick_CancelJoin");

        return panel;
    }

    static GameObject BuildRoleSelectPanel(GameObject parent)
    {
        // 전체 화면을 덮는 불투명 오버레이
        var panel = NewStretchPanel(parent, "RoleSelectPanel", COL_OVERLAY);
        panel.SetActive(false);

        // 타이틀
        var titleGO = NewGO(panel, "Title");
        var tRT = titleGO.GetComponent<RectTransform>();
        tRT.anchorMin  = new Vector2(0f, 0.60f);
        tRT.anchorMax  = new Vector2(1f, 0.72f);
        tRT.offsetMin  = Vector2.zero;
        tRT.offsetMax  = Vector2.zero;
        MakeTMP(titleGO, "누가 플레이할까요?", 36, COL_WHITE, FontStyles.Bold);

        // 부모 버튼
        var btnParent = NewButton(panel, "Btn_RoleParent", "부모로 시작", 30, COL_ORANGE);
        PosButton(btnParent, 0f, 80f, 380f, 80f);

        // 자녀 버튼
        var btnChild = NewButton(panel, "Btn_RoleChild", "자녀로 시작", 30, COL_BLUE);
        PosButton(btnChild, 0f, -20f, 380f, 80f);

        // 취소 버튼
        var btnCancel = NewButton(panel, "Btn_CancelRole", "취소", 24, COL_GRAY);
        PosButton(btnCancel, 0f, -130f, 220f, 60f);

        return panel;
    }

    static GameObject BuildJoinPanel(GameObject parent)
    {
        var panel = NewStretchPanel(parent, "JoinPanel", COL_OVERLAY);
        panel.SetActive(false);

        // 타이틀
        var titleGO = NewGO(panel, "Title");
        var tRT = titleGO.GetComponent<RectTransform>();
        tRT.anchorMin  = new Vector2(0f, 0.62f);
        tRT.anchorMax  = new Vector2(1f, 0.74f);
        tRT.offsetMin  = Vector2.zero;
        tRT.offsetMax  = Vector2.zero;
        MakeTMP(titleGO, "방 코드를 입력하세요", 34, COL_WHITE, FontStyles.Bold);

        // InputField 컨테이너
        var inputContainer = NewGO(panel, "InputContainer");
        var icRT = inputContainer.GetComponent<RectTransform>();
        icRT.anchorMin        = new Vector2(0.5f, 0.5f);
        icRT.anchorMax        = new Vector2(0.5f, 0.5f);
        icRT.pivot            = new Vector2(0.5f, 0.5f);
        icRT.sizeDelta        = new Vector2(340f, 64f);
        icRT.anchoredPosition = new Vector2(0f, 50f);

        var icImg = inputContainer.AddComponent<Image>();
        icImg.color  = new Color(0.88f, 0.92f, 1f);
        icImg.sprite = null;
        icImg.type   = Image.Type.Simple;

        // TMP_InputField
        var inputGO = NewGO(inputContainer, "Input_RoomCode");
        var inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.anchorMin = Vector2.zero;
        inputRT.anchorMax = Vector2.one;
        inputRT.offsetMin = new Vector2(10f, 4f);
        inputRT.offsetMax = new Vector2(-10f, -4f);

        var inputField = inputGO.AddComponent<TMP_InputField>();
        inputField.characterLimit = 4;

        // TextViewport (TMP_InputField 필수 자식)
        var viewportGO = NewGO(inputGO, "Text Area");
        var viewportRT = viewportGO.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = new Vector2(4f, 2f);
        viewportRT.offsetMax = new Vector2(-4f, -2f);
        var viewportMask = viewportGO.AddComponent<RectMask2D>();
        inputField.textViewport = viewportRT;

        // InputField 텍스트 자식 (TextArea 아래)
        var textGO = NewGO(viewportGO, "Text");
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.fontSize  = 30;
        textTMP.color     = new Color(0.08f, 0.08f, 0.15f);
        textTMP.alignment = TextAlignmentOptions.Center;
        if (_font != null) textTMP.font = _font;
        inputField.textComponent = textTMP;

        // Placeholder (TextArea 아래)
        var phGO = NewGO(viewportGO, "Placeholder");
        var phRT = phGO.GetComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero;
        phRT.offsetMax = Vector2.zero;
        var phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text      = "1234";
        phTMP.fontSize  = 30;
        phTMP.color     = new Color(0.55f, 0.58f, 0.68f);
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.alignment = TextAlignmentOptions.Center;
        if (_font != null) phTMP.font = _font;
        inputField.placeholder = phTMP;

        // 에러 텍스트
        var errGO = NewGO(panel, "Text_JoinError");
        var errRT = errGO.GetComponent<RectTransform>();
        errRT.anchorMin        = new Vector2(0.5f, 0.5f);
        errRT.anchorMax        = new Vector2(0.5f, 0.5f);
        errRT.pivot            = new Vector2(0.5f, 0.5f);
        errRT.sizeDelta        = new Vector2(420f, 48f);
        errRT.anchoredPosition = new Vector2(0f, -10f);
        var errTMP = errGO.AddComponent<TextMeshProUGUI>();
        errTMP.text      = "";
        errTMP.fontSize  = 20;
        errTMP.color     = new Color(1f, 0.48f, 0.38f);
        errTMP.alignment = TextAlignmentOptions.Center;
        if (_font != null) errTMP.font = _font;

        // 참여 버튼
        var btnConfirm = NewButton(panel, "Btn_JoinConfirm", "참여하기", 28, COL_GREEN);
        PosButton(btnConfirm, 90f, -90f, 200f, 68f);

        // 취소 버튼
        var btnCancel = NewButton(panel, "Btn_CancelJoin", "취소", 24, COL_GRAY);
        PosButton(btnCancel, -110f, -90f, 170f, 64f);

        return panel;
    }

    // ═══════════════════════════════════════════════════════
    // LobbyPanel
    // ═══════════════════════════════════════════════════════

    static GameObject BuildLobbyPanel(GameObject uiRoot)
    {
        var panel = NewStretchPanel(uiRoot, "LobbyPanel", COL_BG_LOBBY);
        panel.SetActive(false);

        // ── 타이틀 ──────────────────────────────────────────
        var titleGO = NewGO(panel, "TitleText");
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin  = new Vector2(0f, 0.80f);
        titleRT.anchorMax  = new Vector2(1f, 0.92f);
        titleRT.offsetMin  = Vector2.zero;
        titleRT.offsetMax  = Vector2.zero;
        MakeTMP(titleGO, "대기실", 44, COL_ORANGE, FontStyles.Bold);

        // ── 방 코드 ──────────────────────────────────────────
        var codeGO = NewGO(panel, "Text_RoomCode");
        var codeRT = codeGO.GetComponent<RectTransform>();
        codeRT.anchorMin        = new Vector2(0.5f, 0.5f);
        codeRT.anchorMax        = new Vector2(0.5f, 0.5f);
        codeRT.pivot            = new Vector2(0.5f, 0.5f);
        codeRT.sizeDelta        = new Vector2(500f, 50f);
        codeRT.anchoredPosition = new Vector2(0f, 110f);
        MakeTMP(codeGO, "방 코드: ----", 30, COL_WHITE, FontStyles.Bold);

        // ── 내 역할 ──────────────────────────────────────────
        var roleGO = NewGO(panel, "Text_MyRole");
        var roleRT = roleGO.GetComponent<RectTransform>();
        roleRT.anchorMin        = new Vector2(0.5f, 0.5f);
        roleRT.anchorMax        = new Vector2(0.5f, 0.5f);
        roleRT.pivot            = new Vector2(0.5f, 0.5f);
        roleRT.sizeDelta        = new Vector2(500f, 44f);
        roleRT.anchoredPosition = new Vector2(0f, 55f);
        MakeTMP(roleGO, "내 역할: -", 24, COL_LIGHT, FontStyles.Normal);

        // ── 슬롯 배경 ─────────────────────────────────────
        var slotsBox = NewGO(panel, "SlotsBox");
        var sbRT = slotsBox.GetComponent<RectTransform>();
        sbRT.anchorMin        = new Vector2(0.5f, 0.5f);
        sbRT.anchorMax        = new Vector2(0.5f, 0.5f);
        sbRT.pivot            = new Vector2(0.5f, 0.5f);
        sbRT.sizeDelta        = new Vector2(460f, 110f);
        sbRT.anchoredPosition = new Vector2(0f, -30f);
        var sbImg = slotsBox.AddComponent<Image>();
        sbImg.color  = new Color(0f, 0f, 0f, 0.30f);
        sbImg.sprite = null;
        sbImg.type   = Image.Type.Simple;

        // 부모 슬롯
        var slotParent = NewGO(slotsBox, "Slot_Parent");
        var spRT = slotParent.GetComponent<RectTransform>();
        spRT.anchorMin        = new Vector2(0.5f, 0.5f);
        spRT.anchorMax        = new Vector2(0.5f, 0.5f);
        spRT.pivot            = new Vector2(0.5f, 0.5f);
        spRT.sizeDelta        = new Vector2(420f, 44f);
        spRT.anchoredPosition = new Vector2(0f, 26f);
        MakeTMP(slotParent, "부모  대기 중...", 24, new Color(1f, 0.88f, 0.55f), FontStyles.Normal);

        // 자녀 슬롯
        var slotChild = NewGO(slotsBox, "Slot_Child");
        var scRT = slotChild.GetComponent<RectTransform>();
        scRT.anchorMin        = new Vector2(0.5f, 0.5f);
        scRT.anchorMax        = new Vector2(0.5f, 0.5f);
        scRT.pivot            = new Vector2(0.5f, 0.5f);
        scRT.sizeDelta        = new Vector2(420f, 44f);
        scRT.anchoredPosition = new Vector2(0f, -26f);
        MakeTMP(slotChild, "자녀  대기 중...", 24, new Color(0.65f, 0.88f, 1f), FontStyles.Normal);

        // ── 시작 버튼 ─────────────────────────────────────
        var startBtn = NewButton(panel, "Btn_Start", "게임 시작!", 32, COL_GREEN);
        PosButton(startBtn, 0f, -150f, 360f, 90f);

        // ── 나가기 버튼 ───────────────────────────────────
        var backBtn = NewButton(panel, "Btn_Back", "나가기", 24, COL_GRAY);
        PosButton(backBtn, 0f, -255f, 220f, 60f);

        // ── LobbyController 연결 ──────────────────────────
        var ctrl = panel.GetComponent<LobbyController>()
                ?? panel.AddComponent<LobbyController>();
        ctrl.roomCodeText   = codeGO.GetComponent<TMP_Text>();
        ctrl.myRoleText     = roleGO.GetComponent<TMP_Text>();
        ctrl.slotParentText = slotParent.GetComponent<TMP_Text>();
        ctrl.slotChildText  = slotChild.GetComponent<TMP_Text>();
        ctrl.startButton    = startBtn.GetComponent<Button>();
        EditorUtility.SetDirty(ctrl);

        // ── 버튼 연결 ─────────────────────────────────────
        WireButton(startBtn, ctrl, "OnClick_Start");
        WireButton(backBtn,  ctrl, "OnClick_Back");

        return panel;
    }

    // ═══════════════════════════════════════════════════════
    // InGame / Result placeholder
    // ═══════════════════════════════════════════════════════

    // ═══════════════════════════════════════════════════════
    // InGamePanel
    // ═══════════════════════════════════════════════════════

    static GameObject BuildInGamePanel(GameObject uiRoot)
    {
        var panel = NewStretchPanel(uiRoot, "InGamePanel", new Color(0.08f, 0.13f, 0.22f));
        panel.SetActive(false);

        // ── 상단 타이틀 바 ────────────────────────────────
        var topBar = NewGO(panel, "TopBar");
        var topRT  = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 0.88f);
        topRT.anchorMax = new Vector2(1f, 1.00f);
        topRT.offsetMin = Vector2.zero;
        topRT.offsetMax = Vector2.zero;
        var topImg = topBar.AddComponent<Image>();
        topImg.color = new Color(0.05f, 0.08f, 0.16f);
        topImg.sprite = null; topImg.type = Image.Type.Simple;

        var titleGO = NewGO(topBar, "Title");
        StretchTo(titleGO, 0f, 0f, 1f, 1f);
        MakeTMP(titleGO, "100 버거 패밀리", 28, COL_ORANGE, FontStyles.Bold);

        // ── 버거 카운트 (중앙 상단) ───────────────────────
        var countGO = NewGO(panel, "BurgerCount");
        var countRT = countGO.GetComponent<RectTransform>();
        countRT.anchorMin = new Vector2(0f, 0.76f);
        countRT.anchorMax = new Vector2(1f, 0.88f);
        countRT.offsetMin = Vector2.zero;
        countRT.offsetMax = Vector2.zero;
        MakeTMP(countGO, "버거 0개 완성!", 38, COL_ORANGE, FontStyles.Bold);

        // ── 상태 + 타이머 박스 ────────────────────────────
        var stateBox = NewGO(panel, "StateBox");
        var sbRT = stateBox.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0.05f, 0.60f);
        sbRT.anchorMax = new Vector2(0.95f, 0.76f);
        sbRT.offsetMin = Vector2.zero;
        sbRT.offsetMax = Vector2.zero;
        var sbImg = stateBox.AddComponent<Image>();
        sbImg.color = new Color(0f, 0f, 0f, 0.35f);
        sbImg.sprite = null; sbImg.type = Image.Type.Simple;

        var statusGO = NewGO(stateBox, "StatusText");
        var stRT = statusGO.GetComponent<RectTransform>();
        stRT.anchorMin = new Vector2(0f, 0.5f);
        stRT.anchorMax = new Vector2(1f, 1f);
        stRT.offsetMin = Vector2.zero;
        stRT.offsetMax = Vector2.zero;
        MakeTMP(statusGO, "물 안 줌 — 느린 조리 모드", 20, COL_LIGHT, FontStyles.Normal);

        var timerGO = NewGO(stateBox, "TimerText");
        var tiRT = timerGO.GetComponent<RectTransform>();
        tiRT.anchorMin = new Vector2(0f, 0f);
        tiRT.anchorMax = new Vector2(1f, 0.5f);
        tiRT.offsetMin = Vector2.zero;
        tiRT.offsetMax = Vector2.zero;
        MakeTMP(timerGO, "00:00", 32, COL_WHITE, FontStyles.Bold);

        // ── 부모 패널 ──────────────────────────────────────
        var parentPanelGO = NewGO(panel, "ParentPanel");
        var ppRT = parentPanelGO.GetComponent<RectTransform>();
        ppRT.anchorMin = new Vector2(0f, 0.36f);
        ppRT.anchorMax = new Vector2(0.5f, 0.60f);
        ppRT.offsetMin = new Vector2(16f, 0f);
        ppRT.offsetMax = new Vector2(-8f, -8f);
        var ppImg = parentPanelGO.AddComponent<Image>();
        ppImg.color = new Color(0.95f, 0.55f, 0.1f, 0.15f);
        ppImg.sprite = null; ppImg.type = Image.Type.Simple;

        var parentLabel = NewGO(parentPanelGO, "Label");
        var plRT = parentLabel.GetComponent<RectTransform>();
        plRT.anchorMin = new Vector2(0f, 0.6f);
        plRT.anchorMax = new Vector2(1f, 1f);
        plRT.offsetMin = Vector2.zero;
        plRT.offsetMax = Vector2.zero;
        MakeTMP(parentLabel, "부모", 18, COL_ORANGE, FontStyles.Bold);

        var waterBtn = NewButton(parentPanelGO, "Btn_Water", "물주기", 26, COL_ORANGE);
        var wRT = waterBtn.GetComponent<RectTransform>();
        wRT.anchorMin = new Vector2(0.1f, 0.05f);
        wRT.anchorMax = new Vector2(0.9f, 0.58f);
        wRT.offsetMin = Vector2.zero;
        wRT.offsetMax = Vector2.zero;

        // ── 자녀 패널 ──────────────────────────────────────
        var childPanelGO = NewGO(panel, "ChildPanel");
        var cpRT = childPanelGO.GetComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.5f, 0.36f);
        cpRT.anchorMax = new Vector2(1f, 0.60f);
        cpRT.offsetMin = new Vector2(8f, 0f);
        cpRT.offsetMax = new Vector2(-16f, -8f);
        var cpImg = childPanelGO.AddComponent<Image>();
        cpImg.color = new Color(0.2f, 0.5f, 0.9f, 0.15f);
        cpImg.sprite = null; cpImg.type = Image.Type.Simple;

        var childLabel = NewGO(childPanelGO, "Label");
        var clRT = childLabel.GetComponent<RectTransform>();
        clRT.anchorMin = new Vector2(0f, 0.6f);
        clRT.anchorMax = new Vector2(1f, 1f);
        clRT.offsetMin = Vector2.zero;
        clRT.offsetMax = Vector2.zero;
        MakeTMP(childLabel, "자녀", 18, COL_BLUE, FontStyles.Bold);

        var burgerBtn = NewButton(childPanelGO, "Btn_MakeBurger", "버거 만들기", 26, COL_BLUE);
        var bRT = burgerBtn.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0.1f, 0.05f);
        bRT.anchorMax = new Vector2(0.9f, 0.58f);
        bRT.offsetMin = Vector2.zero;
        bRT.offsetMax = Vector2.zero;

        // ── 메시지 프리셋 버튼 ────────────────────────────
        var msgParentBtn = NewButton(panel, "Btn_MsgParent", "물 줬어요!", 20, new Color(0.7f, 0.4f, 0.1f));
        var mpRT = msgParentBtn.GetComponent<RectTransform>();
        mpRT.anchorMin = new Vector2(0.05f, 0.24f);
        mpRT.anchorMax = new Vector2(0.48f, 0.35f);
        mpRT.offsetMin = Vector2.zero;
        mpRT.offsetMax = Vector2.zero;

        var msgChildBtn = NewButton(panel, "Btn_MsgChild", "빨리 먹고싶다!", 20, new Color(0.1f, 0.35f, 0.65f));
        var mcRT = msgChildBtn.GetComponent<RectTransform>();
        mcRT.anchorMin = new Vector2(0.52f, 0.24f);
        mcRT.anchorMax = new Vector2(0.95f, 0.35f);
        mcRT.offsetMin = Vector2.zero;
        mcRT.offsetMax = Vector2.zero;

        // ── 메시지 표시 ───────────────────────────────────
        var msgBox = NewGO(panel, "MessageBox");
        var mbRT = msgBox.GetComponent<RectTransform>();
        mbRT.anchorMin = new Vector2(0.05f, 0.13f);
        mbRT.anchorMax = new Vector2(0.95f, 0.23f);
        mbRT.offsetMin = Vector2.zero;
        mbRT.offsetMax = Vector2.zero;
        var mbImg = msgBox.AddComponent<Image>();
        mbImg.color = new Color(0f, 0f, 0f, 0.3f);
        mbImg.sprite = null; mbImg.type = Image.Type.Simple;

        var msgTextGO = NewGO(msgBox, "MessageText");
        StretchTo(msgTextGO, 0f, 0f, 1f, 1f);
        MakeTMP(msgTextGO, "같이 햄버거 만들어요!", 20, new Color(0.9f, 0.95f, 1f), FontStyles.Normal);

        // ── 하단 버튼 ─────────────────────────────────────
        var resultBtn = NewButton(panel, "Btn_Result", "결과 보기", 26, COL_GREEN);
        var resRT = resultBtn.GetComponent<RectTransform>();
        resRT.anchorMin = new Vector2(0.35f, 0.02f);
        resRT.anchorMax = new Vector2(0.65f, 0.12f);
        resRT.offsetMin = Vector2.zero;
        resRT.offsetMax = Vector2.zero;

        // ── InGameController 연결 ─────────────────────────
        var ctrl = panel.GetComponent<InGameController>()
                ?? panel.AddComponent<InGameController>();
        ctrl.burgerCountText  = countGO.GetComponent<TMP_Text>();
        ctrl.statusText       = statusGO.GetComponent<TMP_Text>();
        ctrl.timerText        = timerGO.GetComponent<TMP_Text>();
        ctrl.messageText      = msgTextGO.GetComponent<TMP_Text>();
        ctrl.parentPanel      = parentPanelGO;
        ctrl.childPanel       = childPanelGO;
        ctrl.waterButton      = waterBtn.GetComponent<Button>();
        ctrl.makeBurgerButton = burgerBtn.GetComponent<Button>();
        ctrl.resultButton     = resultBtn.GetComponent<Button>();
        EditorUtility.SetDirty(ctrl);

        // ── 버튼 연결 ─────────────────────────────────────
        WireButton(waterBtn,      ctrl, "OnClick_Water");
        WireButton(burgerBtn,     ctrl, "OnClick_MakeBurger");
        WireButton(msgParentBtn,  ctrl, "OnClick_MessageParent");
        WireButton(msgChildBtn,   ctrl, "OnClick_MessageChild");
        WireButton(resultBtn,     ctrl, "OnClick_ViewResult");

        return panel;
    }

    // ═══════════════════════════════════════════════════════
    // ResultPanel
    // ═══════════════════════════════════════════════════════

    static GameObject BuildResultPanel(GameObject uiRoot)
    {
        var panel = NewStretchPanel(uiRoot, "ResultPanel", new Color(0.06f, 0.10f, 0.18f));
        panel.SetActive(false);

        // 타이틀
        var titleGO = NewGO(panel, "Title");
        var tRT = titleGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0f, 0.74f);
        tRT.anchorMax = new Vector2(1f, 0.88f);
        tRT.offsetMin = Vector2.zero;
        tRT.offsetMax = Vector2.zero;
        MakeTMP(titleGO, "오늘의 결과", 44, COL_ORANGE, FontStyles.Bold);

        // 버거 카운트
        var countGO = NewGO(panel, "BurgerCount");
        var cRT = countGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0f, 0.58f);
        cRT.anchorMax = new Vector2(1f, 0.74f);
        cRT.offsetMin = Vector2.zero;
        cRT.offsetMax = Vector2.zero;
        MakeTMP(countGO, "오늘 버거 0개 완성!", 40, COL_WHITE, FontStyles.Bold);

        // 코멘트
        var commentGO = NewGO(panel, "Comment");
        var cmRT = commentGO.GetComponent<RectTransform>();
        cmRT.anchorMin = new Vector2(0.1f, 0.46f);
        cmRT.anchorMax = new Vector2(0.9f, 0.58f);
        cmRT.offsetMin = Vector2.zero;
        cmRT.offsetMax = Vector2.zero;
        MakeTMP(commentGO, "", 22, COL_LIGHT, FontStyles.Normal);

        // 다시 만들기 버튼
        var playAgainBtn = NewButton(panel, "Btn_PlayAgain", "다시 만들기", 28, COL_GREEN);
        var paRT = playAgainBtn.GetComponent<RectTransform>();
        paRT.anchorMin = new Vector2(0.5f, 0.30f);
        paRT.anchorMax = new Vector2(0.5f, 0.30f);
        paRT.pivot     = new Vector2(0.5f, 0.5f);
        paRT.sizeDelta        = new Vector2(320f, 72f);
        paRT.anchoredPosition = new Vector2(0f, 0f);

        // 처음으로 버튼
        var menuBtn = NewButton(panel, "Btn_BackToMenu", "처음으로", 24, COL_GRAY);
        var mbtnRT = menuBtn.GetComponent<RectTransform>();
        mbtnRT.anchorMin = new Vector2(0.5f, 0.30f);
        mbtnRT.anchorMax = new Vector2(0.5f, 0.30f);
        mbtnRT.pivot     = new Vector2(0.5f, 0.5f);
        mbtnRT.sizeDelta        = new Vector2(240f, 58f);
        mbtnRT.anchoredPosition = new Vector2(0f, -80f);

        // ResultController 연결
        var ctrl = panel.GetComponent<ResultController>()
                ?? panel.AddComponent<ResultController>();
        ctrl.burgerCountText = countGO.GetComponent<TMP_Text>();
        ctrl.commentText     = commentGO.GetComponent<TMP_Text>();
        EditorUtility.SetDirty(ctrl);

        WireButton(playAgainBtn, ctrl, "OnClick_PlayAgain");
        WireButton(menuBtn,      ctrl, "OnClick_BackToMenu");

        return panel;
    }

    static void StretchTo(GameObject go, float ax, float ay, float bx, float by)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ═══════════════════════════════════════════════════════
    // UIScreenController
    // ═══════════════════════════════════════════════════════

    static void SetupUIScreenController(GameObject canvas,
        GameObject mainPanel, GameObject lobbyPanel,
        GameObject inGamePanel, GameObject resultPanel)
    {
        var ctrl = canvas.GetComponent<UIScreenController>()
                ?? canvas.AddComponent<UIScreenController>();

        var so = new SerializedObject(ctrl);
        var panelsProp = so.FindProperty("panels");
        panelsProp.ClearArray();

        void AddEntry(int idx, UIPanelType type, GameObject go)
        {
            panelsProp.InsertArrayElementAtIndex(idx);
            var el = panelsProp.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("type").enumValueIndex  = (int)type;
            el.FindPropertyRelative("panel").objectReferenceValue = go;
        }

        AddEntry(0, UIPanelType.MainMenu, mainPanel);
        AddEntry(1, UIPanelType.Lobby,    lobbyPanel);
        AddEntry(2, UIPanelType.InGame,   inGamePanel);
        AddEntry(3, UIPanelType.Result,   resultPanel);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(ctrl);
    }

    // ═══════════════════════════════════════════════════════
    // RoomManager
    // ═══════════════════════════════════════════════════════

    static void EnsureRoomManager()
    {
        if (GameObject.Find("RoomManager") != null) return;
        var go = new GameObject("RoomManager");
        go.AddComponent<RoomManager>();
    }

    // ═══════════════════════════════════════════════════════
    // UI 빌딩 헬퍼
    // ═══════════════════════════════════════════════════════

    /// <summary>부모를 꽉 채우는 패널 (전체 stretch)</summary>
    static GameObject NewStretchPanel(GameObject parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.layer = parent.layer;
        var rt = go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color  = bg;
        img.sprite = null;
        img.type   = Image.Type.Simple;
        img.raycastTarget = true;
        return go;
    }

    /// <summary>RectTransform만 있는 빈 GameObject 생성 (부모 하위)</summary>
    static GameObject NewGO(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.layer = parent.layer;
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    /// <summary>버튼 GameObject 생성 (Image + Button + Label TMP)</summary>
    static GameObject NewButton(GameObject parent, string name,
                                string label, int fontSize, Color color)
    {
        var go = NewGO(parent, name);

        var img = go.AddComponent<Image>();
        img.color  = color;
        img.sprite = null;
        img.type   = Image.Type.Simple;

        go.AddComponent<Button>();

        // 라벨 텍스트
        var textGO = NewGO(go, "Label");
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(6f, 4f);
        textRT.offsetMax = new Vector2(-6f, -4f);
        MakeTMP(textGO, label, fontSize, COL_WHITE, FontStyles.Bold);

        return go;
    }

    /// <summary>버튼을 화면 중앙 기준으로 배치</summary>
    static void PosButton(GameObject go, float ax, float ay, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(ax, ay);
        rt.sizeDelta        = new Vector2(w, h);
    }

    /// <summary>TextMeshProUGUI 생성 또는 갱신 (한국어 폰트 필수)</summary>
    static TMP_Text MakeTMP(GameObject go, string text, int size,
                            Color color, FontStyles style)
    {
        var t = go.GetComponent<TextMeshProUGUI>()
             ?? go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.color     = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        if (_font != null) t.font = _font;
        EditorUtility.SetDirty(go);
        return t;
    }

    // ═══════════════════════════════════════════════════════
    // 버튼 OnClick 연결
    // ═══════════════════════════════════════════════════════

    static void WireButton(GameObject go, Object target, string method)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn == null) return;

        // 기존 리스너 전부 제거
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
                target.GetType().GetMethod(method)
            ) as UnityAction
        );
        EditorUtility.SetDirty(go);
    }

    static GameObject FindDeep(GameObject root, string name)
    {
        if (root == null) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }
}
