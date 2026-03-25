using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인게임 HUD 오버레이.
/// 기존 항목 유지 + 새로 추가:
///   - inventoryText: 재고 현황 (InventoryManager 구독)
///   - grillProgressBar: 불판 진행 상태
///   - 업적 달성 알림 (임시 Debug.Log, 추후 팝업 패널 추가)
/// </summary>
public class InGameHUD : MonoBehaviour
{
    [Header("텍스트")]
    public TMP_Text burgerCountText;
    public TMP_Text heldItemText;
    public TMP_Text promptText;
    [Tooltip("재고 현황 표시 텍스트 (신규)")]
    public TMP_Text inventoryText;

    [Header("조리 진행 바 (CookStation)")]
    public GameObject cookProgressBar;
    public Image      cookFillImage;

    [Header("불판 진행 바 (GrillStation, 신규)")]
    public GameObject grillProgressBar;
    public Image      grillFillImage;
    public TMP_Text   grillStatusText;

    [Header("나가기")]
    public Button backButton;

    CookStation      _cookStation;
    GrillStation     _grillStation;
    PlayerController _player;

    void Start()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged         += RefreshBurgerCount;
            GameManager.I.OnAchievementUnlocked  += ShowAchievement;
            RefreshBurgerCount();
        }

        if (InventoryManager.I != null)
        {
            InventoryManager.I.OnInventoryChanged += RefreshInventory;
            RefreshInventory();
        }

        _player = FindFirstObjectByType<PlayerController>();
        if (_player != null)
        {
            _player.OnPromptChanged += UpdatePrompt;
            _player.OnItemChanged   += UpdateHeldItem;
        }

        _cookStation  = FindFirstObjectByType<CookStation>();
        _grillStation = FindFirstObjectByType<GrillStation>();

        if (cookProgressBar)  cookProgressBar.SetActive(false);
        if (grillProgressBar) grillProgressBar.SetActive(false);
        if (promptText)       promptText.text = "";
        if (heldItemText)     heldItemText.text = "";
    }

    void OnDestroy()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged        -= RefreshBurgerCount;
            GameManager.I.OnAchievementUnlocked -= ShowAchievement;
        }
        if (InventoryManager.I != null)
            InventoryManager.I.OnInventoryChanged -= RefreshInventory;

        if (_player != null)
        {
            _player.OnPromptChanged -= UpdatePrompt;
            _player.OnItemChanged   -= UpdateHeldItem;
        }
    }

    void Update()
    {
        // ── 조리 진행 바 ───────────────────────────────────────
        if (_cookStation != null && cookProgressBar != null)
        {
            bool cooking = _cookStation.IsCooking;
            cookProgressBar.SetActive(cooking);
            if (cooking && cookFillImage != null)
                cookFillImage.fillAmount = _cookStation.GetProgress();
        }

        // ── 불판 진행 바 ───────────────────────────────────────
        if (_grillStation != null && grillProgressBar != null)
        {
            bool active = _grillStation.IsGrilling
                       || _grillStation.IsDone
                       || _grillStation.IsBurned;
            grillProgressBar.SetActive(active);

            if (active && grillFillImage != null)
                grillFillImage.fillAmount = _grillStation.GetProgress();

            if (grillStatusText != null)
            {
                if      (_grillStation.IsBurned)   grillStatusText.text = "탔어요!";
                else if (_grillStation.IsDone)     grillStatusText.text = "[E] 꺼내세요!";
                else if (_grillStation.IsGrilling) grillStatusText.text = "굽는 중...";
            }
        }
    }

    // ── 갱신 메서드 ──────────────────────────────────────────────

    void RefreshBurgerCount()
    {
        if (burgerCountText == null || GameManager.I == null) return;
        int cnt = GameManager.I.state.burgerCount;
        burgerCountText.text = $"버거 {cnt} / 100";
    }

    void RefreshInventory()
    {
        if (inventoryText == null || InventoryManager.I == null) return;
        var inv = InventoryManager.I;
        inventoryText.text =
            $"🍅 {inv.Count(IngredientType.Tomato)}  " +
            $"🥬 {inv.Count(IngredientType.Lettuce)}  " +
            $"🍞 {inv.Count(IngredientType.Bread)}  " +
            $"🥩 {inv.Count(IngredientType.RawPatty)}|{inv.Count(IngredientType.GrilledPatty)}  " +
            $"🥓 {inv.Count(IngredientType.RawBacon)}|{inv.Count(IngredientType.GrilledBacon)}  " +
            $"🥫 {inv.Count(IngredientType.Sauce)}";
    }

    void ShowAchievement(string id, string label)
    {
        // 업적 달성 시 버거 카운트 텍스트에 잠시 표시
        // TODO: 별도 팝업 패널로 교체
        Debug.Log($"[HUD] 업적 달성! {label}");
        if (burgerCountText != null)
            StartCoroutine(FlashAchievement(label));
    }

    System.Collections.IEnumerator FlashAchievement(string label)
    {
        string original = burgerCountText.text;
        burgerCountText.text = $"★ {label} ★";
        yield return new WaitForSeconds(2.5f);
        RefreshBurgerCount(); // 원래 텍스트 복원
    }

    void UpdatePrompt(string prompt)
    {
        if (promptText)
            promptText.text = string.IsNullOrEmpty(prompt) ? "WASD로 이동" : prompt;
    }

    void UpdateHeldItem(string itemName)
    {
        if (heldItemText)
            heldItemText.text = string.IsNullOrEmpty(itemName) ? "빈 손" : $"들고 있음: {itemName}";
    }

    // ── 버튼 핸들러 ──────────────────────────────────────────────

    public void OnClick_BackToMenu()
    {
        RoomManager.I?.LeaveRoom();
        GameManager.I?.state.Reset();
        InventoryManager.I?.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
    }
}
