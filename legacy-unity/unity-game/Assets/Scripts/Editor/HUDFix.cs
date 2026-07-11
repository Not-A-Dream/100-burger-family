using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD 레이아웃 수정.
/// Menu: Tools / 100 Burger Family / HUD 수정
///
/// ※ ■ Stop 상태(Edit 모드)에서 실행하세요.
///
/// 변경 내용:
///   - TopPanel 배경 제거 (투명)
///   - TopPanel 크기를 좌상단 작은 영역으로 축소
///   - Text_Status / Text_Timer / Text_LastMessage 비활성화
///   - Text_BurgerCount → 좌상단, 흰색 소형 텍스트
/// </summary>
public static class HUDFix
{
    [MenuItem("Tools/100 Burger Family/HUD 수정 (배너 제거 + 좌상단 버거카운트)", true)]
    static bool Validate() => !Application.isPlaying;

    [MenuItem("Tools/100 Burger Family/HUD 수정 (배너 제거 + 좌상단 버거카운트)")]
    public static void FixHUD()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("실행 불가", "■ Stop 후 실행하세요.", "확인");
            return;
        }

        // ── 1. Canvas 찾기 ─────────────────────────────────────
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[HUDFix] Canvas를 찾을 수 없습니다.");
            return;
        }

        // ── 2. TopPanel 찾기 (Canvas 직접 자식) ────────────────
        Transform topPanelTf = null;
        foreach (Transform child in canvas.transform)
        {
            if (child.name == "TopPanel") { topPanelTf = child; break; }
        }

        if (topPanelTf == null)
        {
            Debug.LogError("[HUDFix] TopPanel을 찾을 수 없습니다. 씬을 확인하세요.");
            return;
        }

        var topPanel = topPanelTf.gameObject;
        Debug.Log($"[HUDFix] TopPanel 발견 (자식 {topPanelTf.childCount}개)");

        // ── 3. 배경 Image 완전 투명 ─────────────────────────────
        var img = topPanel.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0f);
            EditorUtility.SetDirty(topPanel);
            Debug.Log("[HUDFix] TopPanel 배경 투명 처리");
        }

        // ── 4. TopPanel 크기: 좌상단 작은 영역 ─────────────────
        //   anchorMin(0, 0.93) ~ anchorMax(0.22, 1.0)
        //   → 화면 왼쪽 22%, 상단 7% 높이
        var rt = topPanel.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f,    0.93f);
        rt.anchorMax        = new Vector2(0.22f, 1.0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
        EditorUtility.SetDirty(topPanel);
        Debug.Log("[HUDFix] TopPanel 크기 → 좌상단 소형");

        // ── 5. 불필요한 텍스트 비활성화 ────────────────────────
        foreach (var hideName in new[] { "Text_Status", "Text_Timer", "Text_LastMessage" })
        {
            var tf = topPanelTf.Find(hideName);
            if (tf != null)
            {
                tf.gameObject.SetActive(false);
                EditorUtility.SetDirty(tf.gameObject);
                Debug.Log($"[HUDFix] {hideName} 비활성화");
            }
        }

        // ── 6. Text_BurgerCount → 좌정렬, 소형, 흰색 ──────────
        var burgerTf = topPanelTf.Find("Text_BurgerCount");
        if (burgerTf != null)
        {
            // RectTransform: 부모 채우기
            var brt = burgerTf.GetComponent<RectTransform>();
            brt.anchorMin        = Vector2.zero;
            brt.anchorMax        = Vector2.one;
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta        = Vector2.zero;

            // TMP 스타일
            var tmp = burgerTf.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment  = TextAlignmentOptions.MidlineLeft;
                tmp.fontSize   = 20f;
                tmp.color      = new Color(1f, 1f, 1f, 0.95f);
                tmp.fontStyle  = FontStyles.Bold;
                tmp.margin     = new Vector4(12f, 0f, 0f, 0f);
            }

            EditorUtility.SetDirty(burgerTf.gameObject);
            Debug.Log("[HUDFix] Text_BurgerCount → 좌상단 소형 완료");
        }
        else
        {
            Debug.LogWarning("[HUDFix] Text_BurgerCount를 찾지 못했습니다.");
        }

        // ── 7. 씬 저장 ─────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[HUDFix] 완료!");
        EditorUtility.DisplayDialog("HUD 수정 완료",
            "배너 제거 + 버거카운트 좌상단 배치 완료!\n▶ Play로 확인하세요.", "확인");
    }
}
