using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 패널/버튼에 색상 및 기본 스프라이트를 자동 적용합니다.
/// Menu: Tools → Apply Casual UI
/// </summary>
public static class CasualUISetup
{
    // ── 색상 팔레트 ────────────────────────────────────────────────
    static readonly Color COL_GREEN  = new Color(0.25f, 0.72f, 0.35f);
    static readonly Color COL_BLUE   = new Color(0.25f, 0.55f, 0.90f);
    static readonly Color COL_RED    = new Color(0.90f, 0.30f, 0.25f);
    static readonly Color COL_ORANGE = new Color(0.95f, 0.60f, 0.15f);
    static readonly Color COL_TEAL   = new Color(0.15f, 0.55f, 0.65f, 0.94f);
    static readonly Color COL_WHITE  = Color.white;
    static readonly Color COL_PANEL_BG = new Color(0.10f, 0.18f, 0.28f, 0.92f);
    static TMP_FontAsset _korFont;

    [MenuItem("Tools/Apply Casual UI")]
    public static void Apply()
    {
        _korFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");
        // Unity 내장 9-slice 스프라이트 사용 (atlas 슬라이싱 불필요)
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (uiSprite == null)
            Debug.LogWarning("[CasualUISetup] Built-in UISprite not found.");

        // 1. 패널 배경
        SetPanelBG("Canvas/UIRoot/MainMenuPanel", uiSprite, COL_TEAL);
        SetPanelBG("Canvas/UIRoot/LobbyPanel",    uiSprite, COL_TEAL);
        SetPanelBG("Canvas/UIRoot/InGamePanel",   null,     new Color(0,0,0,0));
        SetPanelBG("Canvas/UIRoot/ResultPanel",   uiSprite, COL_ORANGE);
        SetPanelBG("Canvas/TopPanel",             uiSprite, new Color(0.08f, 0.08f, 0.08f, 0.78f));

        // 2. 메인메뉴 버튼 텍스트
        SetTMP("Canvas/UIRoot/MainMenuPanel/MenuContainer/CreateRoomButton/CreateRoom",
               "방 만들기", 28, COL_WHITE);
        SetTMP("Canvas/UIRoot/MainMenuPanel/MenuContainer/JoinRoomButton/JoinRoom",
               "참여하기",  28, COL_WHITE);
        SetBtn("Canvas/UIRoot/MainMenuPanel/MenuContainer/CreateRoomButton", uiSprite, COL_GREEN);
        SetBtn("Canvas/UIRoot/MainMenuPanel/MenuContainer/JoinRoomButton",   uiSprite, COL_BLUE);

        // 3. 로비 버튼
        SetTMP("Canvas/UIRoot/LobbyPanel/LobbyContainer/StartButton/Text (TMP)",
               "시작!", 30, COL_WHITE);
        SetTMP("Canvas/UIRoot/LobbyPanel/LobbyContainer/BackButton/Text (TMP)",
               "뒤로",  26, COL_WHITE);
        SetBtn("Canvas/UIRoot/LobbyPanel/LobbyContainer/StartButton", uiSprite, COL_GREEN);
        SetBtn("Canvas/UIRoot/LobbyPanel/LobbyContainer/BackButton",  uiSprite, COL_RED);

        // 4. 인게임 버튼
        SetTMP("Canvas/UIRoot/InGamePanel/InGameContainer/EndButton/Text (TMP)",
               "완료", 26, COL_WHITE);
        SetBtn("Canvas/UIRoot/InGamePanel/InGameContainer/EndButton", uiSprite, COL_ORANGE);

        // 5. 결과 버튼
        SetTMP("Canvas/UIRoot/ResultPanel/ResultContatiner/MainButton/Text (TMP)",
               "메인으로", 28, COL_WHITE);
        SetBtn("Canvas/UIRoot/ResultPanel/ResultContatiner/MainButton", uiSprite, COL_BLUE);

        // 6. 타이틀 텍스트
        SetTMPIfExists("Canvas/UIRoot/MainMenuPanel/MenuContainer/Title",
                       "100 버거 패밀리", 36, COL_WHITE);
        SetTMPIfExists("Canvas/UIRoot/ResultPanel/ResultContatiner/Title",
                       "결과",           34, COL_WHITE);

        // 7. HUD 텍스트 색상
        SetTMPColor("Canvas/TopPanel/Text_BurgerCount", COL_WHITE);
        SetTMPColor("Canvas/TopPanel/Text_Timer",       COL_WHITE);
        SetTMPColor("Canvas/TopPanel/Text_Status",      COL_WHITE);
        SetTMPColor("Canvas/TopPanel/Text_LastMessage", new Color(1f, 0.95f, 0.7f));

        // 8. HUD 액션 버튼
        SetBtn("Canvas/Buttons/Button_Water",      uiSprite, COL_BLUE);
        SetBtn("Canvas/Buttons/Button_MakeBurger", uiSprite, COL_GREEN);
        SetBtn("Canvas/Buttons/Button_PostParent", uiSprite, COL_TEAL);
        SetBtn("Canvas/Buttons/Button_PostChild",  uiSprite, COL_ORANGE);
        SetTMPChild("Canvas/Buttons/Button_Water",      "물주기",    22, COL_WHITE);
        SetTMPChild("Canvas/Buttons/Button_MakeBurger", "만들기",    22, COL_WHITE);
        SetTMPChild("Canvas/Buttons/Button_PostParent", "부모 메시지", 20, COL_WHITE);
        SetTMPChild("Canvas/Buttons/Button_PostChild",  "자녀 메시지", 20, COL_WHITE);

        // 9. 씬 저장
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[CasualUISetup] UI 적용 완료!");
    }

    // ── 헬퍼 ─────────────────────────────────────────────────────

    static GameObject Find(string path)
    {
        var go = GameObject.Find(path);
        if (go != null) return go;

        string[] parts = path.Split('/');
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
        Debug.LogWarning($"[CasualUISetup] GameObject not found: {path}");
        return null;
    }

    static void SetPanelBG(string path, Sprite sprite, Color color)
    {
        var go = Find(path); if (go == null) return;
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite = sprite;
        img.color  = color;
        img.type   = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        img.raycastTarget = false;
    }

    static void SetBtn(string path, Sprite sprite, Color color)
    {
        var go = Find(path); if (go == null) return;
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite = sprite;
        img.color  = color;
        img.type   = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
    }

    static void SetTMP(string path, string text, int size, Color color)
    {
        var go = Find(path); if (go == null) return;
        var t = go.GetComponent<TextMeshProUGUI>() ?? go.GetComponent<TMP_Text>() as TextMeshProUGUI;
        if (t == null)
            t = go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.color     = color;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        if (_korFont != null) t.font = _korFont;
    }

    static void SetTMPColor(string path, Color color)
    {
        var go = Find(path); if (go == null) return;
        var t = go.GetComponent<TMP_Text>();
        if (t != null) { t.color = color; t.fontStyle = FontStyles.Bold; }
    }

    static void SetTMPIfExists(string path, string text, int size, Color color)
    {
        var go = Find(path); if (go == null) return;
        var t = go.GetComponent<TextMeshProUGUI>() ?? go.GetComponent<TMP_Text>() as TextMeshProUGUI;
        if (t == null)
            t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        if (_korFont != null) t.font = _korFont;
    }

    static void SetTMPChild(string path, string text, int size, Color color)
    {
        var go = Find(path); if (go == null) return;
        var t = go.GetComponentInChildren<TextMeshProUGUI>();
        if (t == null)
        {
            var label = go.transform.Find("Label")?.gameObject;
            if (label == null)
            {
                label = new GameObject("Label");
                label.layer = go.layer;
                label.AddComponent<RectTransform>();
                label.transform.SetParent(go.transform, false);
            }

            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(4f, 2f);
            labelRT.offsetMax = new Vector2(-4f, -2f);
            t = label.GetComponent<TextMeshProUGUI>() ?? label.AddComponent<TextMeshProUGUI>();
        }
        t.text      = text;
        t.fontSize  = size;
        t.color     = color;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        if (_korFont != null) t.font = _korFont;
    }
}
