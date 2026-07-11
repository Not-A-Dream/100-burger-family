using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// InGamePanel 컨트롤러.
/// 역할별 버튼(부모: 물주기 / 자녀: 버거 만들기) + 상태 표시 + 메시지 프리셋
/// </summary>
public class InGameController : MonoBehaviour
{
    [Header("상태 표시")]
    public TMP_Text burgerCountText;  // "버거 3개 완성!"
    public TMP_Text statusText;       // "조리 중..." / "재료 준비 완료"
    public TMP_Text timerText;        // "03:45"
    public TMP_Text messageText;      // lastMessage

    [Header("역할 패널")]
    public GameObject parentPanel;   // 부모 전용 버튼 묶음
    public GameObject childPanel;    // 자녀 전용 버튼 묶음

    [Header("버튼")]
    public Button waterButton;       // 부모: 물주기
    public Button makeBurgerButton;  // 자녀: 버거 만들기
    public Button resultButton;      // 결과 보기

    void OnEnable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged += Refresh;
            Refresh();
        }
        RefreshRoleUI();
    }

    void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.OnStateChanged -= Refresh;
    }

    void Update()
    {
        if (GameManager.I != null && GameManager.I.state.isCooking && timerText != null)
        {
            int sec = GameManager.I.GetRemainingSeconds();
            int m = sec / 60;
            int s = sec % 60;
            timerText.text = $"{m:00}:{s:00}";
        }
    }

    void Refresh()
    {
        if (GameManager.I == null) return;
        var st = GameManager.I.state;

        if (burgerCountText)
            burgerCountText.text = $"버거 {st.burgerCount}개 완성!";

        if (messageText)
            messageText.text = st.lastMessage;

        if (st.isCooking)
        {
            if (statusText) statusText.text = st.wateredToday
                ? "조리 중... (빠름)"
                : "조리 중... (느림)";
            if (makeBurgerButton) makeBurgerButton.interactable = false;
        }
        else
        {
            if (statusText) statusText.text = st.wateredToday
                ? "물 줌 — 빠른 조리 준비!"
                : "물 안 줌 — 느린 조리 모드";
            if (timerText) timerText.text = "00:00";
            if (makeBurgerButton) makeBurgerButton.interactable = true;
        }

        if (waterButton) waterButton.interactable = !st.wateredToday;
    }

    void RefreshRoleUI()
    {
        // 같은 기기 로컬 MVP: 두 패널 모두 표시
        if (parentPanel) parentPanel.SetActive(true);
        if (childPanel)  childPanel.SetActive(true);
    }

    // ── 버튼 핸들러 ───────────────────────────────────────────

    public void OnClick_Water()
    {
        GameManager.I?.ParentWater();
    }

    public void OnClick_MakeBurger()
    {
        GameManager.I?.ChildMakeBurger();
    }

    public void OnClick_MessageParent()
    {
        GameManager.I?.PostPresetMessage("오늘도 물 줬어요!");
    }

    public void OnClick_MessageChild()
    {
        GameManager.I?.PostPresetMessage("버거 빨리 먹고 싶어요!");
    }

    public void OnClick_ViewResult()
    {
        FindFirstObjectByType<UIScreenController>()?.ShowResult();
    }

    public void OnClick_BackToMenu()
    {
        RoomManager.I?.LeaveRoom();
        GameManager.I?.state.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
    }
}
