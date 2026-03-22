using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인게임 HUD (투명 오버레이).
/// - 버거 카운트
/// - 손에 든 아이템
/// - 상호작용 프롬프트
/// - 조리 진행 바
/// </summary>
public class InGameHUD : MonoBehaviour
{
    [Header("텍스트")]
    public TMP_Text burgerCountText;
    public TMP_Text heldItemText;
    public TMP_Text promptText;

    [Header("조리 진행 바")]
    public GameObject cookProgressBar;   // 부모 GameObject (숨기기/보이기)
    public Image      cookFillImage;     // fillAmount 제어

    [Header("나가기")]
    public Button backButton;

    CookStation _cookStation;

    void Start()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged += RefreshBurgerCount;
            RefreshBurgerCount();
        }

        // PlayerController 이벤트 구독
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnPromptChanged += UpdatePrompt;
            player.OnItemChanged   += UpdateHeldItem;
        }

        // CookStation 참조
        _cookStation = FindFirstObjectByType<CookStation>();

        if (cookProgressBar) cookProgressBar.SetActive(false);
        if (promptText)      promptText.text = "";
        if (heldItemText)    heldItemText.text = "";
    }

    void OnDestroy()
    {
        if (GameManager.I != null)
            GameManager.I.OnStateChanged -= RefreshBurgerCount;

        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnPromptChanged -= UpdatePrompt;
            player.OnItemChanged   -= UpdateHeldItem;
        }
    }

    void Update()
    {
        // 조리 진행 바
        if (_cookStation != null && cookProgressBar != null)
        {
            bool cooking = _cookStation.IsCooking;
            cookProgressBar.SetActive(cooking);
            if (cooking && cookFillImage != null)
                cookFillImage.fillAmount = _cookStation.GetProgress();
        }
    }

    void RefreshBurgerCount()
    {
        if (burgerCountText == null || GameManager.I == null) return;
        int cnt = GameManager.I.state.burgerCount;
        burgerCountText.text = $"버거 {cnt} / 100";
    }

    void UpdatePrompt(string prompt)
    {
        if (promptText) promptText.text = prompt;
    }

    void UpdateHeldItem(string itemName)
    {
        if (heldItemText) heldItemText.text =
            string.IsNullOrEmpty(itemName) ? "" : $"들고 있음: {itemName}";
    }

    // ── 버튼 핸들러 ───────────────────────────────────────────
    public void OnClick_BackToMenu()
    {
        RoomManager.I?.LeaveRoom();
        GameManager.I?.state.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
    }
}
