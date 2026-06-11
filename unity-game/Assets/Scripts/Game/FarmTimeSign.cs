using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 재배기 가까이에 갔을 때 보이는 큰 시간 표지판.
///
/// 왜 기존 말풍선과 별도 컴포넌트인가?
///   현재 씬에는 빌더가 만든 고정 Label과 런타임 InteractionBubble이 섞여 있다.
///   그래서 기존 말풍선을 수정해도 실제 화면에서 어떤 UI가 보이는지 헷갈린다.
///   이 컴포넌트는 FarmStation 위에 독립적인 파란 시간 표지판을 만들어
///   플레이어가 가까이 왔을 때 시간 정보가 확실히 보이게 한다.
/// </summary>
[RequireComponent(typeof(FarmStation))]
[ExecuteAlways]
public class FarmTimeSign : MonoBehaviour
{
    public float showDistance = 2.8f;
    public Vector3 offset = new Vector3(0f, 1.72f, 0f);

    FarmStation _farm;
    PlayerController _player;
    GameObject _root;
    GameObject _iconRoot;
    GameObject _digitsRoot;
    string _lastClock = "";

    void OnEnable()
    {
        EnsureReady();
    }

    void Start()
    {
        EnsureReady();
    }

    void EnsureReady()
    {
        _farm = GetComponent<FarmStation>();
        _player = FindFirstObjectByType<PlayerController>();
        if (NeedsRebuild())
            DestroyRoot();
        if (_root == null)
            BuildSign();
    }

    void Update()
    {
        if (_player == null)
            _player = FindFirstObjectByType<PlayerController>();

        if (NeedsRebuild())
            DestroyRoot();

        if (_root == null)
            BuildSign();

        if (_player == null || _root == null) return;

        bool visible = Vector3.Distance(transform.position, _player.transform.position) <= showDistance;
        _root.SetActive(visible);
        if (!visible) return;

        if (Camera.main != null)
            _root.transform.rotation = Camera.main.transform.rotation;

        _root.transform.localPosition = offset;
        RefreshVisuals();
    }

    bool NeedsRebuild()
    {
        return _root != null && _root.transform.Find("BG") != null;
    }

    void DestroyRoot()
    {
        if (_root == null) return;

        if (Application.isPlaying)
            Destroy(_root);
        else
            DestroyImmediate(_root);

        _root = null;
        _iconRoot = null;
        _digitsRoot = null;
        _lastClock = "";
    }

    void BuildSign()
    {
        _root = new GameObject("FarmTimeSign");
        _root.transform.SetParent(transform, false);
        _root.transform.localPosition = offset;

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 80;

        var rt = _root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(156f, 42f);
        rt.localScale = Vector3.one * 0.0085f;

        _iconRoot = new GameObject("CropIcon");
        _iconRoot.transform.SetParent(_root.transform, false);
        var iconRT = _iconRoot.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = new Vector2(14f, 0f);
        iconRT.sizeDelta = new Vector2(26f, 26f);

        _digitsRoot = new GameObject("DigitalClock");
        _digitsRoot.transform.SetParent(_root.transform, false);
        var digitsRT = _digitsRoot.AddComponent<RectTransform>();
        digitsRT.anchorMin = new Vector2(0f, 0.5f);
        digitsRT.anchorMax = new Vector2(0f, 0.5f);
        digitsRT.pivot = new Vector2(0f, 0.5f);
        digitsRT.anchoredPosition = new Vector2(38f, 0f);
        digitsRT.sizeDelta = new Vector2(110f, 26f);

        BuildCropIcon();

        _root.SetActive(false);
    }

    void RefreshVisuals()
    {
        string clock = BuildClockText();
        if (clock == _lastClock) return;

        _lastClock = clock;
        RebuildDigits(clock);
    }

    string BuildClockText()
    {
        if (DailyBurgerRunManager.I == null)
            return "0:00";

        int remaining = DailyBurgerRunManager.I.GetRemainingSeconds(_farm.cropType);

        return _farm.Stage switch
        {
            FarmStation.FarmStage.Idle       => "2:00:00",
            FarmStation.FarmStage.Seeded     => Clock(remaining),
            FarmStation.FarmStage.NeedsWater => "0:02:25",
            FarmStation.FarmStage.Growing    => Clock(remaining),
            FarmStation.FarmStage.Ready      => "0:00:00",
            FarmStation.FarmStage.Harvested  => "0:00:00",
            _                                => "0:00:00"
        };
    }

    static string Clock(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds % 3600) / 60;
        int s = seconds % 60;
        return $"{h}:{m:00}:{s:00}";
    }

    void BuildCropIcon()
    {
        ClearChildren(_iconRoot.transform);

        if (_farm.cropType == IngredientType.Tomato)
        {
            Rect(_iconRoot.transform, "TomatoBody",
                new Vector2(0f, -4f), new Vector2(44f, 44f),
                new Color(0.88f, 0.10f, 0.08f));
            Rect(_iconRoot.transform, "TomatoShine",
                new Vector2(-11f, 4f), new Vector2(12f, 7f),
                new Color(1f, 0.78f, 0.70f, 0.65f));
            Rect(_iconRoot.transform, "LeafC",
                new Vector2(0f, 21f), new Vector2(7f, 17f),
                new Color(0.10f, 0.58f, 0.12f));
            Rect(_iconRoot.transform, "LeafL",
                new Vector2(-14f, 16f), new Vector2(20f, 7f),
                new Color(0.10f, 0.58f, 0.12f), -24f);
            Rect(_iconRoot.transform, "LeafR",
                new Vector2(14f, 16f), new Vector2(20f, 7f),
                new Color(0.10f, 0.58f, 0.12f), 24f);
        }
        else
        {
            Rect(_iconRoot.transform, "CabbageOuter",
                new Vector2(0f, -2f), new Vector2(52f, 40f),
                new Color(0.12f, 0.46f, 0.12f));
            Rect(_iconRoot.transform, "CabbageMid",
                new Vector2(0f, 0f), new Vector2(42f, 34f),
                new Color(0.22f, 0.66f, 0.18f));
            Rect(_iconRoot.transform, "CabbageInner",
                new Vector2(0f, 3f), new Vector2(28f, 24f),
                new Color(0.46f, 0.86f, 0.28f));
            Rect(_iconRoot.transform, "CabbageL",
                new Vector2(-21f, -2f), new Vector2(17f, 31f),
                new Color(0.20f, 0.58f, 0.16f), -18f);
            Rect(_iconRoot.transform, "CabbageR",
                new Vector2(21f, -2f), new Vector2(17f, 31f),
                new Color(0.28f, 0.72f, 0.20f), 18f);
        }
    }

    void RebuildDigits(string clock)
    {
        if (_digitsRoot == null) return;
        ClearChildren(_digitsRoot.transform);

        float x = 0f;
        foreach (char c in clock)
        {
            if (c == ':')
            {
                DrawColon(_digitsRoot.transform, new Vector2(x + 6f, 0f));
                x += 18f;
                continue;
            }

            DrawDigit(_digitsRoot.transform, c, new Vector2(x + 13f, 0f));
            x += 30f;
        }
    }

    static void DrawDigit(Transform parent, char c, Vector2 center)
    {
        bool[] on = c switch
        {
            '0' => new[] { true,  true,  true,  false, true,  true,  true  },
            '1' => new[] { false, false, true,  false, false, true,  false },
            '2' => new[] { true,  false, true,  true,  true,  false, true  },
            '3' => new[] { true,  false, true,  true,  false, true,  true  },
            '4' => new[] { false, true,  true,  true,  false, true,  false },
            '5' => new[] { true,  true,  false, true,  false, true,  true  },
            '6' => new[] { true,  true,  false, true,  true,  true,  true  },
            '7' => new[] { true,  false, true,  false, false, true,  false },
            '8' => new[] { true,  true,  true,  true,  true,  true,  true  },
            '9' => new[] { true,  true,  true,  true,  false, true,  true  },
            _   => new[] { false, false, false, false, false, false, false }
        };

        Color lit = new Color(1f, 0.92f, 0.30f, 1f);
        Color dim = new Color(1f, 0.92f, 0.30f, 0.10f);

        Vector2[] pos =
        {
            new(0f, 22f), new(-11f, 11f), new(11f, 11f),
            new(0f, 0f), new(-11f, -11f), new(11f, -11f),
            new(0f, -22f)
        };
        Vector2[] size =
        {
            new(20f, 5f), new(5f, 18f), new(5f, 18f),
            new(20f, 5f), new(5f, 18f), new(5f, 18f),
            new(20f, 5f)
        };

        for (int i = 0; i < 7; i++)
            Rect(parent, $"Seg_{c}_{i}", center + pos[i], size[i], on[i] ? lit : dim);
    }

    static void DrawColon(Transform parent, Vector2 center)
    {
        Color lit = new Color(1f, 0.92f, 0.30f, 1f);
        Rect(parent, "ColonTop", center + new Vector2(0f, 9f), new Vector2(5f, 5f), lit);
        Rect(parent, "ColonBottom", center + new Vector2(0f, -9f), new Vector2(5f, 5f), lit);
    }

    static void Rect(Transform parent, string name, Vector2 pos, Vector2 size, Color color, float rot = 0f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        if (rot != 0f)
            rt.localEulerAngles = new Vector3(0f, 0f, rot);
        go.AddComponent<Image>().color = color;
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }
}
