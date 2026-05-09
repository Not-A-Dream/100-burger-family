using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// FarmStation 전용 시각적 그림 도움말 말풍선.
///
/// ■ 기존 InteractionBubble과의 차이
///   - 기존: 텍스트만 표시
///   - 이것: 각 단계마다 그림(아이콘 패널) + 텍스트 조합
///
/// ■ 3단계 아이콘 구조
///
///   [씨앗 심기]          [새싹 + 물 주기]       [완성 수확]
///    ● ● ●                  ╱│╲  💧💧            🍅🍅  or 🥬
///   ▓▓▓▓▓▓                ▓▓▓▓▓▓               ✓
///    ↓ 씨앗 심기            ↓ 물 주기             ↓ 수확하기
///
/// ■ 작물 구분
///   - TomatoFarm  → 씨앗 빨강 포인트 / 토마토 열매 (빨간 원형)
///   - LettuceFarm → 씨앗 초록 포인트 / 양상추 열매 (겹겹이 초록 잎)
///
/// ■ 왜 FarmStation에서 분리?
///   FarmStation은 게임 로직(타이머, 상태 전환)만 담당하고,
///   UI/표현은 이 컴포넌트가 전담 → Single Responsibility 원칙
/// </summary>
[RequireComponent(typeof(FarmStation))]
public class FarmInteractionBubble : MonoBehaviour
{
    [Header("말풍선 설정")]
    public float   showDistance = 2.5f;
    public float   bobAmplitude = 0.06f;
    public float   bobSpeed     = 2.0f;
    public Vector3 offset       = new Vector3(0f, 2.3f, 0f);

    // ── 내부 참조 ─────────────────────────────────────────────────
    FarmStation      _farm;
    PlayerController _player;
    GameObject       _root;        // 말풍선 루트 Canvas GO
    float            _bobTimer;

    // 세 가지 아이콘 패널 (하나씩 번갈아 켬)
    GameObject _pSeed;    // 씨앗 심기
    GameObject _pSprout;  // 새싹 + 물
    GameObject _pCrop;    // 완성 작물

    // 텍스트 레이어
    TMP_Text _txtMain;    // "토마토 씨앗을 심어요!"
    TMP_Text _txtAction;  // "[E] 씨앗 심기"
    TMP_Text _txtTimer;   // "⏱ 8초 남았어요"
    TMP_Text _txtTimeBadge; // 말풍선에서 가장 눈에 띄는 시간 배지

    FarmStation.FarmStage _prevStage = (FarmStation.FarmStage)255;

    // ── 공유 색상 ─────────────────────────────────────────────────
    static readonly Color C_BG     = new Color(0.98f, 0.96f, 0.87f, 0.97f);
    static readonly Color C_BORDER = new Color(0.60f, 0.38f, 0.10f, 1.00f);
    static readonly Color C_SOIL   = new Color(0.38f, 0.23f, 0.08f, 1.00f);
    static readonly Color C_STEM   = new Color(0.22f, 0.62f, 0.14f, 1.00f);
    static readonly Color C_WATER  = new Color(0.28f, 0.52f, 0.90f, 1.00f);
    static readonly Color C_TEXT1  = new Color(0.20f, 0.12f, 0.04f, 1.00f);
    static readonly Color C_TEXT2  = new Color(0.52f, 0.08f, 0.04f, 1.00f);
    static readonly Color C_TIMER  = new Color(0.42f, 0.28f, 0.10f, 1.00f);

    // ══════════════════════════════════════════════════════════════
    //  초기화
    // ══════════════════════════════════════════════════════════════
    void Start()
    {
        _farm   = GetComponent<FarmStation>();
        _player = FindFirstObjectByType<PlayerController>();
        BuildBubble();
    }

    void BuildBubble()
    {
        bool   isTomato  = _farm.cropType == IngredientType.Tomato;
        Color  cropColor = isTomato
            ? new Color(0.85f, 0.18f, 0.10f)   // 토마토 빨강
            : new Color(0.22f, 0.70f, 0.18f);  // 양상추 초록
        string cropName  = isTomato ? "토마토" : "양배추";

        // ── 루트 World-Space Canvas ──────────────────────────────
        _root = new GameObject("FarmBubble");
        _root.transform.SetParent(transform, false);
        _root.transform.localPosition = offset;

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rt = _root.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(250f, 250f);
        rt.localScale = Vector3.one * 0.007f;

        // ── 배경 + 테두리 ────────────────────────────────────────
        Stretch(_root.transform, "BG", C_BG);
        Outline(_root.transform, C_BORDER, 3f, 250f, 250f);

        // ── 헤더 바 (작물명, 색상 강조) ───────────────────────────
        Rect(_root.transform, "HeaderBar",
            new Vector2(0f, 89f), new Vector2(230f, 27f), cropColor * 0.82f);
        Text(_root.transform, "HeaderTxt",
            new Vector2(0f, 89f), new Vector2(220f, 27f),
            cropName + " 재배기", 12.5f, Color.white, bold: true);

        BuildCropBadge(_root.transform, isTomato, cropColor);

        // ── 아이콘 패널 3개 (같은 위치, 하나씩 켬) ───────────────
        // 왜 같은 위치? SetActive로 교체 — 코드 단순화
        Vector2 iconPos  = new Vector2(0f, 22f);
        Vector2 iconSize = new Vector2(220f, 108f);

        _pSeed   = BuildSeedPanel  (iconPos, iconSize, cropColor, isTomato, cropName);
        _pSprout = BuildSproutPanel(iconPos, iconSize, cropColor);
        _pCrop   = BuildCropPanel  (iconPos, iconSize, cropColor, isTomato, cropName);

        // ── 구분선 ───────────────────────────────────────────────
        Rect(_root.transform, "Divider",
            new Vector2(0f, -36f), new Vector2(206f, 2f), C_BORDER * 0.5f);

        // ── 텍스트 (하단 3줄) ────────────────────────────────────
        _txtMain   = Text(_root.transform, "TxtMain",
            new Vector2(0f, -53f), new Vector2(218f, 36f),
            "", 13f, C_TEXT1);
        _txtAction = Text(_root.transform, "TxtAction",
            new Vector2(0f, -79f), new Vector2(218f, 28f),
            "", 14f, C_TEXT2, bold: true);
        _txtTimer  = Text(_root.transform, "TxtTimer",
            new Vector2(0f, -98f), new Vector2(232f, 22f),
            "", 11.5f, C_TIMER);

        Rect(_root.transform, "TimeBadgeBg",
            new Vector2(0f, -119f), new Vector2(224f, 28f),
            new Color(0.18f, 0.34f, 0.48f, 0.92f));
        _txtTimeBadge = Text(_root.transform, "TxtTimeBadge",
            new Vector2(0f, -119f), new Vector2(216f, 28f),
            "", 15f, Color.white, bold: true);

        // ── 꼬리 삼각형 ──────────────────────────────────────────
        var tailGO = new GameObject("Tail");
        tailGO.transform.SetParent(_root.transform, false);
        var tailRT = tailGO.AddComponent<RectTransform>();
        tailRT.anchorMin        = tailRT.anchorMax = new Vector2(0.5f, 0f);
        tailRT.pivot            = new Vector2(0.5f, 1f);
        tailRT.sizeDelta        = new Vector2(22f, 14f);
        tailRT.anchoredPosition = new Vector2(0f, 0f);
        tailGO.AddComponent<Image>().color = C_BG;

        _root.SetActive(false);
        SwitchStage(FarmStation.FarmStage.Idle, cropName);
    }

    // ══════════════════════════════════════════════════════════════
    //  아이콘 패널 빌더
    // ══════════════════════════════════════════════════════════════

    // ── 씨앗 심기 패널 ────────────────────────────────────────────
    //
    //    ● ●  ●          ← 씨앗 3개 (작물 색상 내부 점)
    //   ▓▓▓▓▓▓▓▓          ← 흙
    //      ↓               ← 심는 동작 화살표
    //
    GameObject BuildSeedPanel(Vector2 pos, Vector2 size,
                              Color cropColor, bool isTomato, string cropName)
    {
        var p = Panel(_root.transform, "SeedPanel", pos, size);

        // 흙 (하단)
        Rect(p.transform, "Soil",
            new Vector2(0f, -30f), new Vector2(168f, 14f), C_SOIL);

        // 씨앗 3개 위치
        float[] sx = { -42f, 2f,  40f };
        float[] sy = { -18f, -14f, -20f };
        float[] sw = {  24f, 18f,  20f };
        for (int i = 0; i < 3; i++)
        {
            // 씨앗 껍질 (갈색 타원)
            Rect(p.transform, $"Shell{i}",
                new Vector2(sx[i], sy[i]),
                new Vector2(sw[i], sw[i] * 0.62f),
                new Color(0.46f, 0.28f, 0.10f));
            // 씨앗 속 (작물 색상 포인트 — 토마토/양상추 구분)
            Rect(p.transform, $"Dot{i}",
                new Vector2(sx[i], sy[i]),
                new Vector2(sw[i] * 0.50f, sw[i] * 0.38f),
                cropColor * 0.75f);
        }

        // 손가락 3개 (씨앗을 심는 손 표현)
        float[] fx = { -12f, 0f, 12f };
        for (int i = 0; i < 3; i++)
            Rect(p.transform, $"Finger{i}",
                new Vector2(fx[i], 28f),
                new Vector2(7f, 24f),
                new Color(0.90f, 0.72f, 0.56f));

        // 손바닥
        Rect(p.transform, "Palm",
            new Vector2(0f, 14f), new Vector2(34f, 10f),
            new Color(0.90f, 0.72f, 0.56f));

        // 아래 화살표 (손 → 흙 방향)
        Rect(p.transform, "ArrStem",
            new Vector2(46f, 6f), new Vector2(5f, 28f),
            C_TEXT1 * 0.6f);
        Rect(p.transform, "ArrHead",
            new Vector2(46f, -10f), new Vector2(16f, 10f),
            C_TEXT1 * 0.6f, 45f);

        // 작물 구분 라벨 (상단)
        Text(p.transform, "Label",
            new Vector2(0f, 46f), new Vector2(200f, 24f),
            isTomato ? "토마토 씨앗" : "양배추 씨앗",
            13f, cropColor * 0.85f, bold: true);

        return p;
    }

    // ── 새싹 + 물 주기 패널 ──────────────────────────────────────
    //
    //    💧💧💧  ←  물방울 3개
    //   ╱│╲  ╱│    ←  새싹 2그루
    //   ▓▓▓▓▓▓▓
    //
    GameObject BuildSproutPanel(Vector2 pos, Vector2 size, Color cropColor)
    {
        var p = Panel(_root.transform, "SproutPanel", pos, size);

        // 흙
        Rect(p.transform, "Soil",
            new Vector2(-8f, -30f), new Vector2(150f, 12f), C_SOIL);

        // 새싹 1 (왼쪽, 큰)
        Rect(p.transform, "Stem1",
            new Vector2(-28f, -4f), new Vector2(5f, 46f), C_STEM);
        Rect(p.transform, "Leaf1L",
            new Vector2(-44f, 8f), new Vector2(20f, 10f), cropColor, -35f);
        Rect(p.transform, "Leaf1R",
            new Vector2(-12f, 10f), new Vector2(20f, 10f), cropColor, 35f);
        Rect(p.transform, "Leaf1Top",
            new Vector2(-28f, 20f), new Vector2(13f, 13f), cropColor);

        // 새싹 2 (오른쪽, 작음)
        Rect(p.transform, "Stem2",
            new Vector2(14f, -14f), new Vector2(4f, 32f), C_STEM);
        Rect(p.transform, "Leaf2R",
            new Vector2(26f, -5f), new Vector2(16f, 8f), cropColor * 1.1f, 30f);
        Rect(p.transform, "Leaf2Top",
            new Vector2(14f, 8f), new Vector2(10f, 10f), cropColor);

        // 물줄기 4개 (사선으로 떨어짐)
        for (int i = 0; i < 4; i++)
            Rect(p.transform, $"WS{i}",
                new Vector2(44f + i * 9f, 36f - i * 5f),
                new Vector2(3f, 14f),
                new Color(0.30f, 0.55f, 0.92f, 0.65f), -12f);

        // 물방울 3개
        float[] dpx = { 52f, 66f, 42f };
        float[] dpy = { 22f, 10f, 8f  };
        for (int i = 0; i < 3; i++)
        {
            // 물방울 몸통 (파란 타원)
            Rect(p.transform, $"Drop{i}",
                new Vector2(dpx[i], dpy[i]),
                new Vector2(11f, 15f), C_WATER);
            // 물방울 끝(뾰족한 꼭대기)
            Rect(p.transform, $"DropT{i}",
                new Vector2(dpx[i], dpy[i] + 8.5f),
                new Vector2(7f, 8f), C_WATER, 45f);
        }

        // 라벨 (상단)
        Text(p.transform, "Label",
            new Vector2(0f, 46f), new Vector2(200f, 24f),
            "새싹 텄어요 - 양수기 필요",
            13f, new Color(0.18f, 0.45f, 0.82f), bold: true);

        return p;
    }

    // ── 완성 작물 패널 ────────────────────────────────────────────
    //
    // 토마토:                 양상추:
    //  🟢(꼭지)               겹겹이 잎
    //  🔴🔴 (열매 2개)        🥬🥬
    //   ✓ (수확 표시)          ✓
    //
    GameObject BuildCropPanel(Vector2 pos, Vector2 size,
                              Color cropColor, bool isTomato, string cropName)
    {
        var p = Panel(_root.transform, "CropPanel", pos, size);

        if (isTomato)
        {
            // ── 토마토 ────────────────────────────────────────────
            // 열매 1 (큰)
            Rect(p.transform, "Tom1",
                new Vector2(-22f, -6f), new Vector2(58f, 58f),
                new Color(0.86f, 0.18f, 0.10f));
            // 꼭지 3갈래
            Rect(p.transform, "CapC",
                new Vector2(-22f, 24f), new Vector2(7f, 14f),
                new Color(0.18f, 0.58f, 0.12f));
            Rect(p.transform, "CapL",
                new Vector2(-36f, 20f), new Vector2(14f, 6f),
                new Color(0.18f, 0.58f, 0.12f), -22f);
            Rect(p.transform, "CapR",
                new Vector2(-8f, 20f), new Vector2(14f, 6f),
                new Color(0.18f, 0.58f, 0.12f), 22f);
            // 광택
            Rect(p.transform, "Shine1",
                new Vector2(-36f, 6f), new Vector2(13f, 8f),
                new Color(1f, 1f, 1f, 0.40f));

            // 열매 2 (작은)
            Rect(p.transform, "Tom2",
                new Vector2(28f, -18f), new Vector2(38f, 38f),
                new Color(0.78f, 0.14f, 0.08f));
            Rect(p.transform, "Cap2",
                new Vector2(28f, 4f), new Vector2(5f, 9f),
                new Color(0.18f, 0.58f, 0.12f));
            Rect(p.transform, "Shine2",
                new Vector2(18f, -10f), new Vector2(9f, 6f),
                new Color(1f, 1f, 1f, 0.35f));
        }
        else
        {
            // ── 양상추 ────────────────────────────────────────────
            // 겹겹이 잎 (바깥 → 안쪽, 점점 밝고 작아짐)
            Color[] leafCol = {
                new Color(0.14f, 0.46f, 0.10f),
                new Color(0.20f, 0.58f, 0.14f),
                new Color(0.26f, 0.70f, 0.18f),
                new Color(0.32f, 0.80f, 0.24f),
            };
            float[] lw = { 78f, 64f, 50f, 36f };
            float[] lh = { 52f, 46f, 38f, 28f };
            float[] ly = { -8f, -4f, 0f, 5f   };
            for (int i = 0; i < 4; i++)
                Rect(p.transform, $"Leaf{i}",
                    new Vector2(-8f, ly[i]),
                    new Vector2(lw[i], lh[i]),
                    leafCol[i]);

            // 좌우 곁잎
            Rect(p.transform, "LfL",
                new Vector2(-52f, -4f), new Vector2(30f, 42f),
                leafCol[1], -18f);
            Rect(p.transform, "LfR",
                new Vector2(34f, -2f), new Vector2(26f, 36f),
                leafCol[2], 18f);
        }

        // ── ✓ 수확 완료 뱃지 (우측 하단) ─────────────────────────
        Rect(p.transform, "CheckBg",
            new Vector2(62f, -30f), new Vector2(30f, 30f),
            new Color(0.16f, 0.60f, 0.18f));
        Text(p.transform, "CheckMark",
            new Vector2(62f, -30f), new Vector2(30f, 30f),
            "✓", 18f, Color.white, bold: true);

        // 라벨 (상단)
        Text(p.transform, "Label",
            new Vector2(0f, 46f), new Vector2(200f, 24f),
            isTomato ? "토마토 완성!" : "양배추 완성!",
            13f, cropColor, bold: true);

        return p;
    }

    void BuildCropBadge(Transform parent, bool isTomato, Color cropColor)
    {
        var badge = Panel(parent, "CropBadge", new Vector2(-86f, 88f), new Vector2(30f, 24f));
        badge.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);

        if (isTomato)
        {
            Rect(badge.transform, "Fruit",
                new Vector2(0f, -2f), new Vector2(18f, 18f),
                new Color(0.88f, 0.16f, 0.10f));
            Rect(badge.transform, "LeafC",
                new Vector2(0f, 8f), new Vector2(4f, 8f),
                new Color(0.16f, 0.56f, 0.12f));
            Rect(badge.transform, "LeafL",
                new Vector2(-6f, 6f), new Vector2(9f, 4f),
                new Color(0.16f, 0.56f, 0.12f), -25f);
            Rect(badge.transform, "LeafR",
                new Vector2(6f, 6f), new Vector2(9f, 4f),
                new Color(0.16f, 0.56f, 0.12f), 25f);
            Rect(badge.transform, "Shine",
                new Vector2(-5f, 1f), new Vector2(5f, 3f),
                new Color(1f, 1f, 1f, 0.38f));
        }
        else
        {
            Rect(badge.transform, "Leaf0",
                new Vector2(0f, -2f), new Vector2(22f, 16f),
                new Color(0.16f, 0.50f, 0.12f));
            Rect(badge.transform, "Leaf1",
                new Vector2(0f, 0f), new Vector2(17f, 14f),
                new Color(0.24f, 0.66f, 0.18f));
            Rect(badge.transform, "Leaf2",
                new Vector2(0f, 2f), new Vector2(12f, 10f),
                cropColor);
            Rect(badge.transform, "SideL",
                new Vector2(-9f, -1f), new Vector2(8f, 13f),
                new Color(0.22f, 0.60f, 0.16f), -18f);
            Rect(badge.transform, "SideR",
                new Vector2(9f, -1f), new Vector2(8f, 13f),
                new Color(0.26f, 0.70f, 0.18f), 18f);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Update: 거리 감지 + 상태 전환 + 타이머 갱신
    // ══════════════════════════════════════════════════════════════
    void Update()
    {
        // Play 중 스크립트를 다시 컴파일하면 기존 FarmBubble 오브젝트가 남아
        // 새 시간 배지가 없는 상태가 될 수 있다. 그때는 한 번만 재생성한다.
        if (_root != null && _txtTimeBadge == null)
        {
            Destroy(_root);
            BuildBubble();
            _prevStage = (FarmStation.FarmStage)255;
        }

        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
            return;
        }

        float dist    = Vector3.Distance(transform.position, _player.transform.position);
        bool  visible = dist <= showDistance;
        _root.SetActive(visible);
        if (!visible) return;

        // 카메라를 향해 항상 정면
        if (Camera.main != null)
            _root.transform.rotation = Camera.main.transform.rotation;

        // 둥실 애니메이션
        _bobTimer += Time.deltaTime * bobSpeed;
        var lp = offset;
        lp.y += Mathf.Sin(_bobTimer) * bobAmplitude;
        _root.transform.localPosition = lp;

        // 상태 변화 시 아이콘/텍스트 교체
        var stage     = _farm.Stage;
        bool isTomato = _farm.cropType == IngredientType.Tomato;
        string name   = isTomato ? "토마토" : "양배추";

        if (stage != _prevStage)
        {
            SwitchStage(stage, name);
            _prevStage = stage;
        }

        // 타이머 텍스트 실시간 갱신
        RefreshTimer(stage);
    }

    void SwitchStage(FarmStation.FarmStage stage, string cropName)
    {
        // 아이콘 패널 전환
        _pSeed  .SetActive(stage == FarmStation.FarmStage.Idle);
        _pSprout.SetActive(stage == FarmStation.FarmStage.Seeded ||
                           stage == FarmStation.FarmStage.NeedsWater);
        _pCrop  .SetActive(stage == FarmStation.FarmStage.Growing ||
                           stage == FarmStation.FarmStage.Ready ||
                           stage == FarmStation.FarmStage.Harvested);

        // 텍스트 전환
        switch (stage)
        {
            case FarmStation.FarmStage.Idle:
                SetText(_txtMain,   $"{cropName} 씨앗을 심어요!");
                SetText(_txtAction, "[E] 씨앗 심기");
                break;
            case FarmStation.FarmStage.Seeded:
                SetText(_txtMain,   "새싹이 자라는 중...");
                SetText(_txtAction, "새싹까지 2시간");
                break;
            case FarmStation.FarmStage.NeedsWater:
                SetText(_txtMain,   "새싹이 텄어요!\n양수기를 들고 오세요");
                SetText(_txtAction, "[E] 물 주기 - 열매까지 2:25");
                break;
            case FarmStation.FarmStage.Growing:
                SetText(_txtMain,   "열매 맺는 중...");
                SetText(_txtAction, "랭킹 타이머 진행 중");
                break;
            case FarmStation.FarmStage.Ready:
                SetText(_txtMain,   $"{cropName} 완성!\n수확해요!");
                SetText(_txtAction, "[E] 수확하기 ✓");
                break;
            case FarmStation.FarmStage.Harvested:
                SetText(_txtMain,   $"{cropName} 오늘 수확 완료");
                SetText(_txtAction, "내일 다시 심어요");
                break;
        }
    }

    void RefreshTimer(FarmStation.FarmStage stage)
    {
        if (_txtTimer == null) return;

        if (stage == FarmStation.FarmStage.Idle)
        {
            _txtTimer.text = "기준 시간: 씨앗 -> 새싹 2시간";
            SetTimeBadge("새싹까지 2시간");
        }
        else if (stage == FarmStation.FarmStage.NeedsWater)
        {
            _txtTimer.text = "물 주기 후 열매까지 2분 25초";
            SetTimeBadge("물 주면 02:25");
        }
        else if (stage == FarmStation.FarmStage.Seeded || stage == FarmStation.FarmStage.Growing)
        {
            int remaining = DailyBurgerRunManager.I.GetRemainingSeconds(_farm.cropType);
            string label = stage == FarmStation.FarmStage.Seeded ? "새싹까지" : "열매까지";
            _txtTimer.text = $"{label} 남은 시간: {FormatRemaining(remaining)}";
            SetTimeBadge($"{label} {FormatRemaining(remaining)}");
            if (stage == FarmStation.FarmStage.Growing && DailyBurgerRunManager.I.HasActiveRun)
            {
                _txtTimer.text += $" / 기록 {DailyBurgerRunManager.FormatSeconds(DailyBurgerRunManager.I.ActiveRunSeconds)}";
                SetTimeBadge($"{label} {FormatRemaining(remaining)} | 기록 {DailyBurgerRunManager.FormatSeconds(DailyBurgerRunManager.I.ActiveRunSeconds)}");
            }
        }
        else if (stage == FarmStation.FarmStage.Ready)
        {
            _txtTimer.text = "수확 가능";
            SetTimeBadge("지금 수확 가능");
        }
        else if (stage == FarmStation.FarmStage.Harvested)
        {
            _txtTimer.text = "오늘 수확 완료";
            SetTimeBadge("오늘 완료");
        }
        else
        {
            _txtTimer.text = "";
            SetTimeBadge("");
        }
    }

    void SetTimeBadge(string text)
    {
        if (_txtTimeBadge != null)
            _txtTimeBadge.text = text;
    }

    static string FormatRemaining(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds % 3600) / 60;
        int s = seconds % 60;

        if (h > 0) return $"{h}시간 {m:00}분";
        return $"{m:00}:{s:00}";
    }

    static void SetText(TMP_Text t, string s) { if (t != null) t.text = s; }

    // ══════════════════════════════════════════════════════════════
    //  UI 헬퍼 — 모두 _root 기준 anchoredPosition 사용
    // ══════════════════════════════════════════════════════════════

    // 꽉 채우는 배경
    static void Stretch(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
    }

    // 4면 테두리
    static void Outline(Transform parent, Color color, float t, float w, float h)
    {
        // top, bottom, left, right
        Vector2[] ps = { new Vector2(0, h/2),   new Vector2(0,-h/2),
                         new Vector2(-w/2, 0),  new Vector2(w/2, 0) };
        Vector2[] ss = { new Vector2(w, t),     new Vector2(w, t),
                         new Vector2(t, h),     new Vector2(t, h) };
        string[] ns = { "BT","BB","BL","BR" };
        for (int i = 0; i < 4; i++) Rect(parent, ns[i], ps[i], ss[i], color);
    }

    // 직사각형 Image
    static void Rect(Transform parent, string name,
                     Vector2 pos, Vector2 size, Color color, float rot = 0f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        if (rot != 0f) rt.localEulerAngles = new Vector3(0f, 0f, rot);
        go.AddComponent<Image>().color = color;
    }

    // 투명 컨테이너 패널 (아이콘 루트)
    GameObject Panel(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = Color.clear;
        return go;
    }

    // TMP 텍스트
    TMP_Text Text(Transform parent, string name,
                  Vector2 pos, Vector2 size,
                  string text, float fontSize, Color color,
                  bool bold = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text            = text;
        tmp.fontSize        = fontSize;
        tmp.color           = color;
        tmp.alignment       = TextAlignmentOptions.Center;
        tmp.fontStyle       = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        // 한국어 폰트 자동 탐색
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/MalgunGothic SDF")
                ?? FindFirstObjectByType<TMP_Text>()?.font;
        if (font != null) tmp.font = font;

        return tmp;
    }
}
