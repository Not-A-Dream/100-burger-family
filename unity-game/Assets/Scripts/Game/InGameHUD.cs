using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game HUD controller.
/// Keeps the on-screen feedback compact and readable.
/// </summary>
public class InGameHUD : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text burgerCountText;
    public TMP_Text heldItemText;
    public TMP_Text promptText;
    [Tooltip("Inventory summary text")]
    public TMP_Text inventoryText;

    [Header("Cook Progress")]
    public GameObject cookProgressBar;
    public Image cookFillImage;

    [Header("Grill Progress")]
    public GameObject grillProgressBar;
    public Image grillFillImage;
    public TMP_Text grillStatusText;

    [Header("Menu")]
    public Button backButton;

    Button _debugCompleteCropsButton;
    CookStation _cookStation;
    GrillStation _grillStation;
    PlayerController _player;

    void Start()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged += RefreshBurgerCount;
            GameManager.I.OnAchievementUnlocked += ShowAchievement;
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
            _player.OnItemChanged += UpdateHeldItem;
        }

        _cookStation = FindFirstObjectByType<CookStation>();
        _grillStation = FindFirstObjectByType<GrillStation>();

        if (cookProgressBar) cookProgressBar.SetActive(false);
        if (grillProgressBar) grillProgressBar.SetActive(false);
        if (promptText) promptText.text = "";
        if (heldItemText) heldItemText.text = "";

        EnsureDebugCompleteButton();
    }

    void OnDestroy()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged -= RefreshBurgerCount;
            GameManager.I.OnAchievementUnlocked -= ShowAchievement;
        }

        if (InventoryManager.I != null)
            InventoryManager.I.OnInventoryChanged -= RefreshInventory;

        if (_player != null)
        {
            _player.OnPromptChanged -= UpdatePrompt;
            _player.OnItemChanged -= UpdateHeldItem;
        }
    }

    void Update()
    {
        if (_cookStation != null && cookProgressBar != null)
        {
            bool cooking = _cookStation.IsCooking;
            cookProgressBar.SetActive(cooking);
            if (cooking && cookFillImage != null)
                cookFillImage.fillAmount = _cookStation.GetProgress();
        }

        if (_grillStation != null && grillProgressBar != null)
        {
            bool active = _grillStation.IsGrilling || _grillStation.IsDone || _grillStation.IsBurned;
            grillProgressBar.SetActive(active);

            if (active && grillFillImage != null)
                grillFillImage.fillAmount = _grillStation.GetProgress();

            if (grillStatusText != null)
            {
                if (_grillStation.IsBurned) grillStatusText.text = "BURN";
                else if (_grillStation.IsDone) grillStatusText.text = "[E]";
                else if (_grillStation.IsGrilling) grillStatusText.text = "GRILL";
            }
        }
    }

    void RefreshBurgerCount()
    {
        if (burgerCountText == null || GameManager.I == null) return;
        int cnt = GameManager.I.state.burgerCount;
        burgerCountText.text = $"BURGER {cnt}/100";
    }

    void RefreshInventory()
    {
        if (inventoryText == null || InventoryManager.I == null) return;
        var inv = InventoryManager.I;
        inventoryText.text =
            $"T {inv.Count(IngredientType.Tomato)}  " +
            $"L {inv.Count(IngredientType.Lettuce)}  " +
            $"B {inv.Count(IngredientType.Bread)}  " +
            $"P {inv.Count(IngredientType.RawPatty)}|{inv.Count(IngredientType.GrilledPatty)}  " +
            $"K {inv.Count(IngredientType.RawBacon)}|{inv.Count(IngredientType.GrilledBacon)}  " +
            $"S {inv.Count(IngredientType.Sauce)}";
    }

    void ShowAchievement(string id, string label)
    {
        Debug.Log($"[HUD] {label}");
        if (burgerCountText != null)
            StartCoroutine(FlashAchievement(label));
    }

    IEnumerator FlashAchievement(string label)
    {
        if (burgerCountText == null)
            yield break;

        string original = burgerCountText.text;
        burgerCountText.text = $"★ {label} ★";
        yield return new WaitForSeconds(2.5f);
        burgerCountText.text = original;
        RefreshBurgerCount();
    }

    void UpdatePrompt(string prompt)
    {
        if (promptText)
            promptText.text = string.IsNullOrEmpty(prompt) ? "WASD 이동  E 사용" : prompt;
    }

    void UpdateHeldItem(string itemName)
    {
        if (heldItemText)
            heldItemText.text = string.IsNullOrEmpty(itemName) ? "손: -" : $"손: {itemName}";
    }

    public void OnClick_BackToMenu()
    {
        RoomManager.I?.LeaveRoom();
        GameManager.I?.state.Reset();
        InventoryManager.I?.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
    }

    public void OnClick_DebugCompleteCrops()
    {
        DailyBurgerRunManager.I.DebugCompleteAllCrops();
        UpdatePrompt("테스트: 작물 완성");
    }

    void EnsureDebugCompleteButton()
    {
        if (_debugCompleteCropsButton != null) return;

        var parent = transform;
        var old = parent.Find("Btn_DebugCompleteCrops");
        if (old != null)
        {
            _debugCompleteCropsButton = old.GetComponent<Button>();
            if (_debugCompleteCropsButton != null)
            {
                _debugCompleteCropsButton.onClick.RemoveAllListeners();
                _debugCompleteCropsButton.onClick.AddListener(OnClick_DebugCompleteCrops);
            }
            return;
        }

        var go = new GameObject("Btn_DebugCompleteCrops");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-24f, -24f);
        rt.sizeDelta = new Vector2(170f, 42f);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.42f, 0.28f, 0.92f);

        _debugCompleteCropsButton = go.AddComponent<Button>();
        _debugCompleteCropsButton.onClick.AddListener(OnClick_DebugCompleteCrops);

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "작물완성";
        label.fontSize = 18f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }
}
