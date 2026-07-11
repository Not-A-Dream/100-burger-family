using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MVP gameplay HUD. 주문·시간·손·상호작용·성공/실패 feedback을 한곳에서 갱신한다.
/// </summary>
public class InGameHUD : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text burgerCountText;
    public TMP_Text orderText;
    public TMP_Text statusText;
    public TMP_Text timeText;
    public TMP_Text heldItemText;
    public TMP_Text promptText;
    public TMP_Text feedbackText;
    [Tooltip("Success/failure summary text")]
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

    CookStation _cookStation;
    GrillStation _grillStation;
    PlayerController _player;
    AudioSource _audioSource;
    AudioClip _successClip;
    AudioClip _failureClip;
    Coroutine _feedbackRoutine;

    void Start()
    {
        AutoBindFallbackReferences();
        EnsureRuntimeTheme();
        EnsureFeedbackAudio();

        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged += RefreshState;
            GameManager.I.OnAchievementUnlocked += ShowAchievement;
            GameManager.I.OnFeedback += ShowFeedback;
        }

        DailyBurgerRunManager.I.OnOrderChanged += RefreshOrder;

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
        if (feedbackText) feedbackText.text = "";

        RefreshState();
        RefreshOrder();
        UpdatePrompt("");
        UpdateHeldItem("");

#if UNITY_EDITOR
        EnsureDebugCompleteButton();
#endif
    }

    void AutoBindFallbackReferences()
    {
        burgerCountText ??= FindText("Text_BurgerCount", "BurgerCountText");
        orderText ??= FindText("Text_Order", "OrderText");
        statusText ??= FindText("Text_Status", "StatusText");
        timeText ??= FindText("Text_Timer", "TimerText");
        heldItemText ??= FindText("Text_HeldItem", "HeldItemText", "HeldItem");
        promptText ??= FindText("Text_Prompt", "PromptText");
        feedbackText ??= FindText("Text_Feedback", "Text_LastMessage");
        inventoryText ??= FindText("Text_Inventory", "InventoryText", "Text_ResultSummary");
    }

    TMP_Text FindText(params string[] names)
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return null;

        var preferredRoot = canvas.transform.Find("AssetPackHUD");
        foreach (string name in names)
        {
            var found = preferredRoot != null ? FindRecursive(preferredRoot, name) : null;
            found ??= FindRecursive(canvas.transform, name);
            if (found != null)
                return found.GetComponent<TMP_Text>() ?? found.GetComponentInChildren<TMP_Text>(true);
        }
        return null;
    }

    static Transform FindRecursive(Transform root, string targetName)
    {
        if (root.name == targetName) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindRecursive(root.GetChild(i), targetName);
            if (found != null) return found;
        }
        return null;
    }

    void EnsureRuntimeTheme()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform hudRoot = canvas.transform.Find("AssetPackHUD") ?? canvas.transform;
        Sprite panelSprite = hudRoot.GetComponentInChildren<Image>(true)?.sprite
                          ?? Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        if (orderText == null)
        {
            var card = CreatePanel(hudRoot, "RuntimeOrderCard", panelSprite);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 1f);
            cardRt.pivot = new Vector2(0.5f, 1f);
            cardRt.anchoredPosition = new Vector2(0f, -24f);
            cardRt.sizeDelta = new Vector2(390f, 92f);
            orderText = CreateRuntimeText(card.transform, "Text_Order", 20f, TextAlignmentOptions.Center);
            var orderRt = orderText.rectTransform;
            orderRt.anchorMin = Vector2.zero;
            orderRt.anchorMax = Vector2.one;
            orderRt.offsetMin = new Vector2(22f, 12f);
            orderRt.offsetMax = new Vector2(-22f, -12f);
            orderText.color = new Color(1f, 0.86f, 0.38f);
        }

        if (promptText != null && promptText.transform.parent.name != "RuntimePromptBubble")
        {
            var bubble = CreatePanel(hudRoot, "RuntimePromptBubble", panelSprite);
            var bubbleRt = bubble.GetComponent<RectTransform>();
            bubbleRt.anchorMin = bubbleRt.anchorMax = new Vector2(0.5f, 0f);
            bubbleRt.pivot = new Vector2(0.5f, 0f);
            bubbleRt.anchoredPosition = new Vector2(0f, 22f);
            bubbleRt.sizeDelta = new Vector2(700f, 112f);

            promptText.transform.SetParent(bubble.transform, false);
            var promptRt = promptText.rectTransform;
            promptRt.anchorMin = Vector2.zero;
            promptRt.anchorMax = Vector2.one;
            promptRt.offsetMin = new Vector2(24f, 10f);
            promptRt.offsetMax = new Vector2(-24f, -38f);
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.fontSize = 16f;

            if (feedbackText != null && feedbackText != promptText)
            {
                feedbackText.transform.SetParent(bubble.transform, false);
                var feedbackRt = feedbackText.rectTransform;
                feedbackRt.anchorMin = new Vector2(0f, 1f);
                feedbackRt.anchorMax = new Vector2(1f, 1f);
                feedbackRt.pivot = new Vector2(0.5f, 1f);
                feedbackRt.anchoredPosition = new Vector2(0f, -10f);
                feedbackRt.sizeDelta = new Vector2(-48f, 28f);
                feedbackText.alignment = TextAlignmentOptions.Center;
                feedbackText.fontSize = 17f;
            }

            var tail = new GameObject("Tail", typeof(RectTransform), typeof(Image));
            tail.transform.SetParent(bubble.transform, false);
            var tailRt = tail.GetComponent<RectTransform>();
            tailRt.anchorMin = tailRt.anchorMax = new Vector2(0.5f, 0f);
            tailRt.anchoredPosition = new Vector2(-190f, -7f);
            tailRt.sizeDelta = new Vector2(22f, 22f);
            tailRt.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tail.GetComponent<Image>().color = new Color(0.20f, 0.16f, 0.16f, 0.96f);
        }
    }

    static GameObject CreatePanel(Transform parent, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, 0.97f);
        image.raycastTarget = false;
        return go;
    }

    static TMP_Text CreateRuntimeText(Transform parent, string name, float size, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = ReadableTMPFont.Resolve();
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    void OnDestroy()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged -= RefreshState;
            GameManager.I.OnAchievementUnlocked -= ShowAchievement;
            GameManager.I.OnFeedback -= ShowFeedback;
        }

        if (DailyBurgerRunManager.HasInstance)
            DailyBurgerRunManager.I.OnOrderChanged -= RefreshOrder;

        if (_player != null)
        {
            _player.OnPromptChanged -= UpdatePrompt;
            _player.OnItemChanged -= UpdateHeldItem;
        }
    }

    void Update()
    {
        if (timeText != null)
            timeText.text = $"시간  {DailyBurgerRunManager.FormatSeconds(DailyBurgerRunManager.I.OrderElapsedSeconds)}";

        if (_cookStation != null && cookProgressBar != null)
        {
            bool active = _cookStation.IsCooking;
            cookProgressBar.SetActive(active);
            if (active && cookFillImage != null)
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
                if (_grillStation.IsBurned) grillStatusText.text = "탔어요";
                else if (_grillStation.IsDone) grillStatusText.text = "꺼내기";
                else if (_grillStation.IsGrilling) grillStatusText.text = "굽는 중";
            }
        }
    }

    void RefreshState()
    {
        if (GameManager.I == null) return;
        var state = GameManager.I.state;
        if (burgerCountText) burgerCountText.text = $"완성  {state.burgerCount}";
        if (inventoryText) inventoryText.text = $"성공 {state.burgerCount}  ·  실패 {state.failureCount}";
    }

    void RefreshOrder()
    {
        var order = DailyBurgerRunManager.I.CurrentOrder;
        if (order == null) return;

        if (orderText)
            orderText.text = $"오늘의 주문\n{order.displayName}";
        if (statusText)
            statusText.text = $"상태  {StatusLabel(order.status)}";
    }

    static string StatusLabel(FixedOrderStatus status) => status switch
    {
        FixedOrderStatus.Active => "재료 준비",
        FixedOrderStatus.Cooking => "조리 중",
        FixedOrderStatus.ReadyToServe => "전달 대기",
        FixedOrderStatus.Failed => "실패 · 다시 도전",
        FixedOrderStatus.Succeeded => "주문 성공",
        _ => "준비"
    };

    void ShowAchievement(string id, string label)
    {
        ShowFeedback($"업적 달성: {label}", true);
    }

    void ShowFeedback(string message, bool success)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(message, success));

        if (_audioSource != null)
            _audioSource.PlayOneShot(success ? _successClip : _failureClip);
    }

    IEnumerator ShowFeedbackRoutine(string message, bool success)
    {
        if (feedbackText != null)
        {
            feedbackText.color = success
                ? new Color(1f, 0.88f, 0.35f)
                : new Color(1f, 0.48f, 0.40f);
            feedbackText.text = message;
        }

        yield return new WaitForSeconds(1.8f);
        if (feedbackText != null) feedbackText.text = "";
        _feedbackRoutine = null;
    }

    void UpdatePrompt(string prompt)
    {
        if (!promptText) return;
        string action = string.IsNullOrEmpty(prompt) ? "가까운 대상에 접근하세요" : prompt;
        promptText.text = $"{action}\nWASD 이동  ·  E 상호작용  ·  Q 내려놓기";
    }

    void UpdateHeldItem(string itemName)
    {
        if (heldItemText)
            heldItemText.text = string.IsNullOrEmpty(itemName) ? "손  비어 있음" : $"손  {itemName}";
    }

    void EnsureFeedbackAudio()
    {
        _audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0.55f;
        _successClip = CreateTone("Success", new[] { 660f, 880f }, 0.22f);
        _failureClip = CreateTone("Failure", new[] { 220f }, 0.18f);
    }

    static AudioClip CreateTone(string clipName, IReadOnlyList<float> frequencies, float duration)
    {
        const int sampleRate = 22050;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float frequency = frequencies[Mathf.Min(frequencies.Count - 1, i * frequencies.Count / samples)];
            float envelope = Mathf.Sin(Mathf.PI * i / samples);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.32f;
        }

        var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
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
        ShowFeedback("테스트용 작물이 준비됐어요.", true);
    }

#if UNITY_EDITOR
    void EnsureDebugCompleteButton()
    {
        if (transform.Find("Btn_DebugCompleteCrops") != null) return;

        var go = new GameObject("Btn_DebugCompleteCrops", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-24f, -24f);
        rt.sizeDelta = new Vector2(150f, 38f);
        go.GetComponent<Image>().color = new Color(0.32f, 0.22f, 0.16f, 0.94f);
        go.GetComponent<Button>().onClick.AddListener(OnClick_DebugCompleteCrops);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;
        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.font = ReadableTMPFont.Resolve();
        label.text = "작물 바로 준비";
        label.fontSize = 15f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }
#endif
}
