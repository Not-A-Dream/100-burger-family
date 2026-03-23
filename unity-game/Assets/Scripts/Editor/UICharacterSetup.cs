using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenuPanel에 캐릭터 선택 패널을 추가합니다.
/// Menu: Tools / 100 Burger Family / UI 캐릭터 선택 패널 추가
///
/// ※ ■ Stop 상태에서 실행하세요.
/// </summary>
public static class UICharacterSetup
{
    // ── 폰트 경로 ──────────────────────────────────────────────────
    const string MALGUN_PATH = "Assets/Fonts/MalgunGothic SDF.asset";

    // 한옥 테마 색상
    static readonly Color C_BTN_DAD      = new Color(0.22f, 0.40f, 0.78f);
    static readonly Color C_BTN_MOM      = new Color(0.95f, 0.75f, 0.20f);
    static readonly Color C_BTN_SON      = new Color(0.22f, 0.62f, 0.28f);
    static readonly Color C_BTN_DAUGHTER = new Color(0.90f, 0.35f, 0.55f);
    static readonly Color C_BTN_BACK     = new Color(0.55f, 0.45f, 0.35f);
    static readonly Color C_PANEL_BG     = new Color(0.96f, 0.92f, 0.84f, 0.97f);

    static TMP_FontAsset _cachedFont;

    // ──────────────────────────────────────────────────────────────
    //  메뉴 1: 폰트 Atlas 수정 (단독 실행 가능)
    // ──────────────────────────────────────────────────────────────
    [MenuItem("Tools/100 Burger Family/MalgunGothic 폰트 수정 (Dynamic 모드)")]
    public static void RepairFont()
    {
        _cachedFont = null;  // 캐시 리셋
        var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MALGUN_PATH);

        if (fa == null)
        {
            Debug.LogError($"[UICharacterSetup] 폰트를 찾을 수 없습니다: {MALGUN_PATH}");
            EditorUtility.DisplayDialog("폰트 수정 실패",
                $"{MALGUN_PATH} 파일을 찾을 수 없습니다.", "확인");
            return;
        }

        //  ─────────────────────────────────────────────────────────
        //  문제:
        //    Static 모드의 TMP_FontAsset은 미리 구운 Atlas Texture를
        //    필요로 함. 이 Atlas가 유실되면 텍스트가 렌더링되지 않음.
        //
        //  해결:
        //    Dynamic 모드로 전환하면 .ttf 파일에서 실시간으로
        //    SDF 글자를 생성하므로 미리 구운 Atlas가 없어도 됨.
        //
        //  비교:
        //    Static  : Atlas 미리 구워둠 → 빠르지만 유실 시 텍스트 불가
        //    Dynamic : .ttf에서 실시간 생성 → 처음 글자 로드 시 약간 느림
        //  ─────────────────────────────────────────────────────────
        if (fa.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            fa.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            Debug.Log("[UICharacterSetup] MalgunGothic SDF → Dynamic 모드 전환 완료");
        }
        else
        {
            Debug.Log("[UICharacterSetup] MalgunGothic SDF는 이미 Dynamic 모드입니다.");
        }

        EditorUtility.DisplayDialog("폰트 수정 완료",
            "MalgunGothic SDF → Dynamic 모드 전환 완료\n\n" +
            "이제 .ttf 파일에서 실시간으로 글자를 생성합니다.\n" +
            "UI 캐릭터 선택 패널 추가를 다시 실행하세요.", "확인");
    }

    // ──────────────────────────────────────────────────────────────
    //  메뉴 2: 캐릭터 선택 패널 추가
    // ──────────────────────────────────────────────────────────────
    [MenuItem("Tools/100 Burger Family/UI 캐릭터 선택 패널 추가", true)]
    static bool Validate() => !Application.isPlaying;

    [MenuItem("Tools/100 Burger Family/UI 캐릭터 선택 패널 추가")]
    public static void Setup()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("실행 불가", "■ Stop 후 실행하세요.", "확인");
            return;
        }

        // ── 0. 폰트 자동 수리 ────────────────────────────────────
        _cachedFont = null;
        EnsureFontDynamic();

        // ── 1. MainMenuPanel 찾기 (inactive 포함) ────────────────
        var mainMenu = FindInScene("MainMenuPanel");
        if (mainMenu == null)
        {
            Debug.LogError("[UICharacterSetup] MainMenuPanel을 찾을 수 없습니다.");
            return;
        }

        // ── 2. 기존 패널 삭제 ────────────────────────────────────
        DestroyIfExists(mainMenu, "ParentCharPanel");
        DestroyIfExists(mainMenu, "ChildCharPanel");

        // ── 3. 부모 선택 패널 ────────────────────────────────────
        var parentPanel = CreateCharPanel(mainMenu, "ParentCharPanel",
            "누구로 시작할까요?",
            new[]
            {
                ("아빠로 시작", C_BTN_DAD),
                ("엄마로 시작", C_BTN_MOM),
                ("뒤로",        C_BTN_BACK),
            });

        // ── 4. 자녀 선택 패널 ────────────────────────────────────
        var childPanel = CreateCharPanel(mainMenu, "ChildCharPanel",
            "어떤 자녀인가요?",
            new[]
            {
                ("아들로 시작",  C_BTN_SON),
                ("딸로 시작",    C_BTN_DAUGHTER),
                ("뒤로",         C_BTN_BACK),
            });

        // ── 5. MainMenuController Inspector 연결 ─────────────────
        var ctrl = mainMenu.GetComponent<MainMenuController>();
        if (ctrl != null)
        {
            ctrl.parentCharPanel = parentPanel;
            ctrl.childCharPanel  = childPanel;
            EditorUtility.SetDirty(ctrl);
            Debug.Log("[UICharacterSetup] 패널 연결 완료");
        }

        // ── 6. 씬 저장 ───────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[UICharacterSetup] 완료! ▶ Play로 확인하세요.");
        EditorUtility.DisplayDialog("완료",
            "캐릭터 선택 패널 추가 완료!\n\n▶ Play 후 '부모로 시작' 클릭", "확인");
    }

    // ══════════════════════════════════════════════════════════════
    //  내부 헬퍼
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// MalgunGothic SDF를 Dynamic 모드로 보장.
    /// Atlas 유실 여부와 무관하게 .ttf에서 실시간 생성하도록 함.
    /// </summary>
    static void EnsureFontDynamic()
    {
        var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MALGUN_PATH);
        if (fa == null) return;

        if (fa.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            fa.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            Debug.Log("[UICharacterSetup] MalgunGothic SDF → Dynamic 모드 (자동 수정)");
        }
    }

    /// <summary>씬에서 폰트를 가져옴. Dynamic 모드 우선.</summary>
    static TMP_FontAsset GetSceneFont()
    {
        if (_cachedFont != null) return _cachedFont;

        // ── 1순위: MalgunGothic SDF (Dynamic 모드) ───────────────
        var malgun = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MALGUN_PATH);
        if (malgun != null &&
            malgun.atlasPopulationMode == AtlasPopulationMode.Dynamic)
        {
            _cachedFont = malgun;
            Debug.Log("[UICharacterSetup] MalgunGothic SDF (Dynamic) 사용");
            return _cachedFont;
        }

        // ── 2순위: 씬에서 atlas가 있는 TMP 탐색 ─────────────────
        foreach (var t in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (t?.font == null)                           continue;
            if (!t.gameObject.scene.IsValid())             continue;
            if (EditorUtility.IsPersistent(t.gameObject)) continue;

            var fa = t.font;
            bool ok = fa.atlasPopulationMode == AtlasPopulationMode.Dynamic
                   || fa.atlasTexture != null;
            if (!ok) continue;

            _cachedFont = fa;
            Debug.Log($"[UICharacterSetup] 씬 폰트 사용: {fa.name}");
            return _cachedFont;
        }

        // ── 3순위: TMP 기본 폰트 ─────────────────────────────────
        _cachedFont = TMP_Settings.defaultFontAsset;
        if (_cachedFont != null)
            Debug.Log($"[UICharacterSetup] TMP 기본 폰트: {_cachedFont.name}");
        else
            Debug.LogWarning("[UICharacterSetup] 사용 가능한 폰트가 없습니다!");

        return _cachedFont;
    }

    static GameObject CreateCharPanel(GameObject parent, string panelName,
                                       string title,
                                       (string label, Color color)[] buttons)
    {
        var panelGO = new GameObject(panelName);
        panelGO.transform.SetParent(parent.transform, false);

        var rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(400f, 380f);

        panelGO.AddComponent<Image>().color = C_PANEL_BG;

        // ── 제목 ─────────────────────────────────────────────────
        MakeTMPText(panelGO, "Title",
            title,
            anchorMin: new Vector2(0f, 1f),
            anchorMax: new Vector2(1f, 1f),
            pivot    : new Vector2(0.5f, 1f),
            pos      : new Vector2(0f, -18f),
            size     : new Vector2(0f, 60f),
            fontSize : 26,
            bold     : true,
            color    : new Color(0.28f, 0.18f, 0.08f));

        // ── 버튼들 ───────────────────────────────────────────────
        float btnH   = 65f;
        float btnGap = 14f;
        float startY = -90f;

        for (int i = 0; i < buttons.Length; i++)
        {
            var (lbl, col) = buttons[i];
            float y = startY - i * (btnH + btnGap);

            var btnGO = new GameObject($"Btn_{lbl.Trim()}");
            btnGO.transform.SetParent(panelGO.transform, false);

            var brt = btnGO.AddComponent<RectTransform>();
            brt.anchorMin        = new Vector2(0.5f, 1f);
            brt.anchorMax        = new Vector2(0.5f, 1f);
            brt.pivot            = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0f, y);
            brt.sizeDelta        = new Vector2(300f, btnH);

            btnGO.AddComponent<Image>().color = col;
            btnGO.AddComponent<Button>();

            MakeTMPText(btnGO, "Label",
                lbl.Trim(),
                anchorMin: Vector2.zero,
                anchorMax: Vector2.one,
                pivot    : new Vector2(0.5f, 0.5f),
                pos      : Vector2.zero,
                size     : Vector2.zero,
                fontSize : 24,
                bold     : true,
                color    : Color.white);

            EditorUtility.SetDirty(btnGO);
        }

        panelGO.SetActive(false);
        EditorUtility.SetDirty(panelGO);
        Debug.Log($"[UICharacterSetup] {panelName} 생성 완료");
        return panelGO;
    }

    /// <summary>TextMeshProUGUI를 안전하게 생성 (폰트 명시 할당)</summary>
    static TextMeshProUGUI MakeTMPText(
        GameObject parent, string goName, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 pos, Vector2 size,
        float fontSize, bool bold, Color color)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var tmp = go.AddComponent<TextMeshProUGUI>();

        //  ─────────────────────────────────────────────────────────
        //  중요: AddComponent<TextMeshProUGUI>() 는 에디터 클릭과 달리
        //  폰트를 자동으로 할당하지 않음 → 명시적으로 지정해야 함
        //  ─────────────────────────────────────────────────────────
        tmp.font      = GetSceneFont();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;

        EditorUtility.SetDirty(go);
        return tmp;
    }

    static void DestroyIfExists(GameObject parent, string name)
    {
        var existing = parent.transform.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
            Debug.Log($"[UICharacterSetup] {name} 삭제");
        }
    }

    static GameObject FindInScene(string goName)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name != goName)              continue;
            if (!go.scene.IsValid())             continue;
            if (EditorUtility.IsPersistent(go)) continue;
            return go;
        }
        return null;
    }
}
