using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Interactable))]
public class InteractionBubble : MonoBehaviour
{
    public float showDistance = 2.5f;
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 2.0f;
    public Vector3 offset = new Vector3(0f, 1.8f, 0f);

    static readonly Color BgColor = new Color(0.98f, 0.96f, 0.87f, 0.95f);
    static readonly Color BorderColor = new Color(0.55f, 0.36f, 0.10f, 1f);
    static readonly Color TextColor = new Color(0.16f, 0.10f, 0.04f, 1f);
    static readonly Color BadgeColor = new Color(0.06f, 0.26f, 0.46f, 0.96f);

    Interactable _interactable;
    PlayerController _player;
    GameObject _root;
    TMP_Text _text;
    TMP_Text _badgeText;
    float _bobTimer;
    string _lastPrompt = "";
    string _lastBadge = "";

    void Start()
    {
        _interactable = GetComponent<Interactable>();
        _player = FindFirstObjectByType<PlayerController>();
        BuildBubble();
    }

    void BuildBubble()
    {
        _root = new GameObject("InteractionBubble");
        _root.transform.SetParent(transform, false);
        _root.transform.localPosition = offset;

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 15;

        var rt = _root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 92f);
        rt.localScale = Vector3.one * 0.0076f;

        var bg = AddRect(_root.transform, "BG", Vector2.zero, new Vector2(220f, 92f), BgColor);
        var outline = bg.gameObject.AddComponent<Outline>();
        outline.effectColor = BorderColor;
        outline.effectDistance = new Vector2(1.4f, -1.4f);

        var badge = AddRect(_root.transform, "Badge", new Vector2(0f, 31f), new Vector2(64f, 10f), BadgeColor);
        badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 1f);
        badge.pivot = new Vector2(0.5f, 1f);
        badge.anchoredPosition = new Vector2(0f, -5f);
        _badgeText = AddText(badge, "BadgeText", Vector2.zero, new Vector2(60f, 8f), "", 5.0f, Color.white, true);

        _text = AddText(_root.GetComponent<RectTransform>(), "PromptText", new Vector2(0f, -4f), new Vector2(198f, 40f), "", 9.8f, TextColor, true);
        _text.rectTransform.anchorMin = _text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        _text.rectTransform.anchoredPosition = new Vector2(0f, -6f);
        _text.alignment = TextAlignmentOptions.Center;
        _text.textWrappingMode = TextWrappingModes.Normal;

        _root.SetActive(false);
    }

    void Update()
    {
        if (_player == null)
            _player = FindFirstObjectByType<PlayerController>();

        if (_player == null || _root == null)
            return;

        bool inRange = Vector3.Distance(transform.position, _player.transform.position) <= showDistance;
        _root.SetActive(inRange);
        if (!inRange)
            return;

        if (Camera.main != null)
            _root.transform.rotation = Camera.main.transform.rotation;

        _bobTimer += Time.deltaTime * bobSpeed;
        var pos = offset;
        pos.y += Mathf.Sin(_bobTimer) * bobAmplitude;
        _root.transform.localPosition = pos;

        if (_interactable is FarmStation farm && DailyBurgerRunManager.I == null)
            return;

        string prompt = _interactable.GetPrompt();
        string badge = BuildTimeBadgeText();
        if (prompt != _lastPrompt)
        {
            _lastPrompt = prompt;
            _text.text = prompt;
        }

        if (badge != _lastBadge)
        {
            _lastBadge = badge;
            _badgeText.text = badge;
        }
    }

    string BuildTimeBadgeText()
    {
        if (_interactable is not FarmStation farm)
            return "";

        if (DailyBurgerRunManager.I == null)
            return "";

        int remain = DailyBurgerRunManager.I.GetRemainingSeconds(farm.cropType);
        return farm.Stage switch
        {
            FarmStation.FarmStage.Idle => "SEED 2:00:00",
            FarmStation.FarmStage.Seeded => $"SPROUT {FormatClock(remain)}",
            FarmStation.FarmStage.NeedsWater => "WATER 02:25",
            FarmStation.FarmStage.Growing => DailyBurgerRunManager.I.HasActiveRun
                ? $"GROW {FormatClock(remain)} | RUN {DailyBurgerRunManager.FormatSeconds(DailyBurgerRunManager.I.ActiveRunSeconds)}"
                : $"GROW {FormatClock(remain)}",
            FarmStation.FarmStage.Ready => "READY NOW",
            FarmStation.FarmStage.Harvested => "DONE TODAY",
            _ => ""
        };
    }

    static string FormatClock(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds % 3600) / 60;
        int s = seconds % 60;
        return h > 0 ? $"{h}:{m:00}:{s:00}" : $"{m:00}:{s:00}";
    }

    static RectTransform AddRect(Transform parent, string name, Vector2 center, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = color;
        return rt;
    }

    static TMP_Text AddText(RectTransform parent, string name, Vector2 center, Vector2 size, string text, float fontSize, Color color, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = center;
        rt.sizeDelta = size;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        ApplyReadableFont(tmp);
        return tmp;
    }

    static void ApplyReadableFont(TMP_Text text)
    {
        var font = ResolveReadableFont();
        if (font != null)
            text.font = font;
    }

    static TMP_FontAsset ResolveReadableFont()
    {
        var preferred = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (preferred != null)
            return preferred;

        if (TMP_Settings.defaultFontAsset != null && TMP_Settings.defaultFontAsset.material != null)
            return TMP_Settings.defaultFontAsset;

        return TMP_Settings.defaultFontAsset;
    }
}
