using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스테이션 위에 떠다니는 말풍선 프롬프트.
/// 플레이어가 가까이 오면 나타나고 멀어지면 사라집니다.
/// </summary>
[RequireComponent(typeof(Interactable))]
public class InteractionBubble : MonoBehaviour
{
    [Header("말풍선 설정")]
    public float showDistance  = 2.5f;    // 상호작용 감지 거리 (PlayerController.interactSearchRadius와 맞춤)
    public float bobAmplitude  = 0.06f;   // 위아래 둥실 정도
    public float bobSpeed      = 2.0f;
    public Vector3 offset      = new Vector3(0f, 1.8f, 0f);  // 오브젝트 위 표시 위치

    Interactable _interactable;
    Canvas       _canvas;
    TMP_Text     _text;
    TMP_Text     _timeBadgeText;
    GameObject   _bubbleGO;

    PlayerController _player;
    float            _bobTimer;

    void Start()
    {
        _interactable = GetComponent<Interactable>();
        _player = FindFirstObjectByType<PlayerController>();
        BuildBubble();
    }

    void BuildBubble()
    {
        // ── World Space Canvas ───────────────────────────
        _bubbleGO = new GameObject("InteractionBubble");
        _bubbleGO.transform.SetParent(transform, false);
        _bubbleGO.transform.localPosition = offset;

        _canvas = _bubbleGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.sortingOrder = 10;

        var rt = _bubbleGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(245f, 118f);
        rt.localScale = new Vector3(0.008f, 0.008f, 0.008f);

        // ── 말풍선 배경 ──────────────────────────────────
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(_bubbleGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color  = new Color(1f, 1f, 0.88f, 0.95f);   // 크림 노란색
        bgImg.sprite = null;
        bgImg.type   = Image.Type.Simple;

        // 꼬리 삼각형 (작은 쿼드를 회전시킴)
        var tailGO = new GameObject("Tail");
        tailGO.transform.SetParent(_bubbleGO.transform, false);
        var tailRT = tailGO.AddComponent<RectTransform>();
        tailRT.anchorMin = new Vector2(0.5f, 0f);
        tailRT.anchorMax = new Vector2(0.5f, 0f);
        tailRT.pivot     = new Vector2(0.5f, 1f);
        tailRT.sizeDelta = new Vector2(22f, 16f);
        tailRT.anchoredPosition = new Vector2(0f, 0f);
        tailRT.localRotation = Quaternion.Euler(0f, 0f, 0f);
        var tailImg = tailGO.AddComponent<Image>();
        tailImg.color  = new Color(1f, 1f, 0.88f, 0.95f);
        tailImg.sprite = null;
        tailImg.type   = Image.Type.Simple;

        // ── 텍스트 ───────────────────────────────────────
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(_bubbleGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10f, 42f);
        textRT.offsetMax = new Vector2(-10f, -8f);

        _text = textGO.AddComponent<TextMeshProUGUI>();
        _text.text      = "";
        _text.fontSize  = 18;
        _text.color     = new Color(0.15f, 0.10f, 0.05f);
        _text.alignment = TextAlignmentOptions.Center;
        _text.fontStyle = FontStyles.Bold;

        ApplyReadableFont(_text);

        // ── 큰 시간 배지 ─────────────────────────────────
        // 기존 말풍선 텍스트는 폰트 아틀라스 문제로 안 보일 수 있으므로,
        // 숫자가 큰 별도 배지를 만들어 시간 정보가 반드시 눈에 띄게 한다.
        var badgeGO = new GameObject("TimeBadge");
        badgeGO.transform.SetParent(_bubbleGO.transform, false);
        var badgeRT = badgeGO.AddComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.5f, 0f);
        badgeRT.anchorMax = new Vector2(0.5f, 0f);
        badgeRT.pivot = new Vector2(0.5f, 0f);
        badgeRT.anchoredPosition = new Vector2(0f, 8f);
        badgeRT.sizeDelta = new Vector2(220f, 34f);

        var badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.color = new Color(0.05f, 0.24f, 0.42f, 0.96f);

        var badgeTextGO = new GameObject("TimeText");
        badgeTextGO.transform.SetParent(badgeGO.transform, false);
        var badgeTextRT = badgeTextGO.AddComponent<RectTransform>();
        badgeTextRT.anchorMin = Vector2.zero;
        badgeTextRT.anchorMax = Vector2.one;
        badgeTextRT.offsetMin = Vector2.zero;
        badgeTextRT.offsetMax = Vector2.zero;

        _timeBadgeText = badgeTextGO.AddComponent<TextMeshProUGUI>();
        _timeBadgeText.text = "";
        _timeBadgeText.fontSize = 22;
        _timeBadgeText.color = Color.white;
        _timeBadgeText.alignment = TextAlignmentOptions.Center;
        _timeBadgeText.fontStyle = FontStyles.Bold;
        ApplyReadableFont(_timeBadgeText);

        _bubbleGO.SetActive(false);
    }

    void Update()
    {
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
            return;
        }

        if (_bubbleGO != null && _timeBadgeText == null)
        {
            Destroy(_bubbleGO);
            BuildBubble();
        }

        float dist = Vector3.Distance(transform.position, _player.transform.position);
        bool inRange = dist <= showDistance;

        if (_bubbleGO != null)
        {
            _bubbleGO.SetActive(inRange);

            if (inRange)
            {
                // 텍스트 갱신
                if (_text != null)
                    _text.text = _interactable.GetPrompt();
                if (_timeBadgeText != null)
                    _timeBadgeText.text = BuildTimeBadgeText();

                // 카메라를 향해 항상 정면
                if (Camera.main != null)
                    _bubbleGO.transform.rotation = Camera.main.transform.rotation;

                // 둥실 애니메이션
                _bobTimer += Time.deltaTime * bobSpeed;
                var localPos = offset;
                localPos.y += Mathf.Sin(_bobTimer) * bobAmplitude;
                _bubbleGO.transform.localPosition = localPos;
            }
        }
    }

    string BuildTimeBadgeText()
    {
        if (_interactable is not FarmStation farm)
            return "";

        int remain = DailyBurgerRunManager.I.GetRemainingSeconds(farm.cropType);
        return farm.Stage switch
        {
            FarmStation.FarmStage.Idle       => "SEED 2:00:00",
            FarmStation.FarmStage.Seeded     => $"SPROUT {FormatClock(remain)}",
            FarmStation.FarmStage.NeedsWater => "WATER -> 02:25",
            FarmStation.FarmStage.Growing    => DailyBurgerRunManager.I.HasActiveRun
                ? $"FRUIT {FormatClock(remain)} | RUN {DailyBurgerRunManager.FormatSeconds(DailyBurgerRunManager.I.ActiveRunSeconds)}"
                : $"FRUIT {FormatClock(remain)}",
            FarmStation.FarmStage.Ready      => "READY NOW",
            FarmStation.FarmStage.Harvested  => "DONE TODAY",
            _                                => ""
        };
    }

    static string FormatClock(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds % 3600) / 60;
        int s = seconds % 60;
        return h > 0 ? $"{h}:{m:00}:{s:00}" : $"{m:00}:{s:00}";
    }

    static void ApplyReadableFont(TMP_Text text)
    {
        if (text == null) return;

        var defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null)
        {
            text.font = defaultFont;
            return;
        }

        // MalgunGothic SDF는 현재 프로젝트에서 atlas texture 경고가 나고 있어
        // 깨진 에셋을 강제로 물리지 않는다.
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            text.font = font;
    }
}
