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
    public float showDistance  = 2.2f;
    public float bobAmplitude  = 0.08f;   // 위아래 둥실 정도
    public float bobSpeed      = 2.0f;
    public Vector3 offset      = new Vector3(0f, 2.2f, 0f);

    Interactable _interactable;
    Canvas       _canvas;
    TMP_Text     _text;
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
        rt.sizeDelta = new Vector2(200f, 70f);
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
        textRT.offsetMin = new Vector2(8f, 8f);
        textRT.offsetMax = new Vector2(-8f, -8f);

        _text = textGO.AddComponent<TextMeshProUGUI>();
        _text.text      = "";
        _text.fontSize  = 18;
        _text.color     = new Color(0.15f, 0.10f, 0.05f);
        _text.alignment = TextAlignmentOptions.Center;
        _text.fontStyle = FontStyles.Bold;

        // 한국어 폰트 자동 적용
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/MalgunGothic SDF")
                ?? FindFirstObjectByType<TMP_Text>()?.font;
        if (font != null) _text.font = font;

        _bubbleGO.SetActive(false);
    }

    void Update()
    {
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
            return;
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
}
