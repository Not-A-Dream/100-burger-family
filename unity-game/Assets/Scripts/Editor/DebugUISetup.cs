using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 테스트용 디버그 패널을 Canvas에 추가합니다.
/// Menu: Tools → Add Debug Panel
/// </summary>
public static class DebugUISetup
{
    static readonly Color COL_RESET  = new Color(0.85f, 0.20f, 0.15f);
    static readonly Color COL_LEAVE  = new Color(0.55f, 0.55f, 0.60f);
    static readonly Color COL_TOGGLE = new Color(0.15f, 0.15f, 0.20f, 0.85f);
    static TMP_FontAsset _korFont;
    static Sprite        _uiSprite;

    [MenuItem("Tools/Add Debug Panel")]
    public static void Setup()
    {
        _uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        _korFont  = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");

        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas not found"); return; }

        // ── 토글 버튼 (항상 보이는 작은 버튼, 우측 하단) ──────────
        var toggleGO = CreateOrFind(canvas, "DebugToggleBtn");
        StyleRect(toggleGO, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 10), new Vector2(70, 30));
        var toggleImg = EnsureImage(toggleGO, COL_TOGGLE);
        var toggleBtn = toggleGO.GetComponent<Button>() ?? toggleGO.AddComponent<Button>();
        var toggleTxt = CreateOrFind(toggleGO, "Label");
        StretchFill(toggleTxt);
        SetTMP(toggleTxt, "[D]", 14, Color.white);

        // ── 디버그 패널 (토글로 표시/숨김) ──────────────────────────
        var panelGO = CreateOrFind(canvas, "DebugPanel");
        StyleRect(panelGO, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 50), new Vector2(240, 220));
        EnsureImage(panelGO, new Color(0.05f, 0.05f, 0.10f, 0.92f));
        panelGO.SetActive(false);

        // 상태 텍스트
        var statusGO = CreateOrFind(panelGO, "StatusText");
        StyleRect(statusGO, new Vector2(0, 0.35f), new Vector2(1, 1), Vector2.zero, Vector2.zero, true);
        var statusTMP = SetTMP(statusGO, "방: 없음", 14, new Color(0.8f, 1f, 0.8f));

        // 초기화 버튼
        var resetGO = CreateOrFind(panelGO, "Btn_ResetAll");
        StyleRect(resetGO, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 65), new Vector2(200, 44));
        EnsureImage(resetGO, COL_RESET);
        var resetBtn = resetGO.GetComponent<Button>() ?? resetGO.AddComponent<Button>();
        var resetLbl = CreateOrFind(resetGO, "Label");
        StretchFill(resetLbl);
        SetTMP(resetLbl, "방 전체 초기화", 18, Color.white);

        // 나가기 버튼
        var leaveGO = CreateOrFind(panelGO, "Btn_Leave");
        StyleRect(leaveGO, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(200, 40));
        EnsureImage(leaveGO, COL_LEAVE);
        var leaveBtn = leaveGO.GetComponent<Button>() ?? leaveGO.AddComponent<Button>();
        var leaveLbl = CreateOrFind(leaveGO, "Label");
        StretchFill(leaveLbl);
        SetTMP(leaveLbl, "방 나가기 (방 유지)", 16, Color.white);

        // ── DebugController 연결 ─────────────────────────────────
        var dbgCtrl = canvas.GetComponent<DebugController>()
                   ?? canvas.AddComponent<DebugController>();
        dbgCtrl.panel      = panelGO;
        dbgCtrl.statusText = statusGO.GetComponent<TMP_Text>();
        EditorUtility.SetDirty(dbgCtrl);

        // 버튼 OnClick 연결
        WireClick(toggleBtn, dbgCtrl, "TogglePanel");
        WireClick(resetBtn,  dbgCtrl, "OnClick_ResetAll");
        WireClick(leaveBtn,  dbgCtrl, "OnClick_LeaveRoom");

        // 씬 저장
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[DebugUISetup] 디버그 패널 추가 완료 (D키 또는 [D] 버튼으로 토글)");
    }

    // ── 헬퍼 ─────────────────────────────────────────────────

    static GameObject CreateOrFind(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null)
        {
            if (parent.GetComponent<RectTransform>() != null && t.GetComponent<RectTransform>() == null)
                t.gameObject.AddComponent<RectTransform>();
            return t.gameObject;
        }
        var go = new GameObject(name);
        go.layer = parent.layer;
        if (parent.GetComponent<RectTransform>() != null)
            go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static void StyleRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                          Vector2 anchoredPos, Vector2 sizeDelta, bool stretch = false)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = anchorMin; // pivot = anchor (우측하단)
        rt.anchoredPosition = anchoredPos;
        if (!stretch) rt.sizeDelta = sizeDelta;
    }

    static void StretchFill(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4, 2);
        rt.offsetMax = new Vector2(-4, -2);
    }

    static Image EnsureImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite = _uiSprite;
        img.color  = color;
        img.type   = Image.Type.Sliced;
        return img;
    }

    static TMP_Text SetTMP(GameObject go, string text, int size, Color color)
    {
        if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
        var t = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.color     = color;
        t.alignment = TextAlignmentOptions.Center;
        if (_korFont != null) t.font = _korFont;
        return t;
    }

    static void WireClick(Button btn, Object target, string method)
    {
        var onClick = btn.onClick;
        for (int i = 0; i < onClick.GetPersistentEventCount(); i++)
            if (onClick.GetPersistentTarget(i) == target &&
                onClick.GetPersistentMethodName(i) == method) return;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            onClick,
            System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction),
                target,
                target.GetType().GetMethod(method)
            ) as UnityEngine.Events.UnityAction
        );
        EditorUtility.SetDirty(btn.gameObject);
    }
}
