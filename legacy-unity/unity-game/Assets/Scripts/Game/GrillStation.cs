using UnityEngine;

/// <summary>
/// 불판 스테이션 (신규).
/// 웹앱(100-burger)의 Grill.jsx 포팅:
///   생 패티 + 생 베이컨 → (grillTime초 굽기) → Done → (collectWindow초 내 수거)
///   수거 못 하면 → Burned (재료 소실)
///
/// 웹앱과 다른 점:
///   - 웹앱: 60~90초 + 중간 뒤집기 미니게임
///   - Unity MVP: 15초 굽기, 뒤집기 없이 단순화 (추후 추가 가능)
///   - 재료는 PlayerHand로 하나씩 올리고 완성품도 하나씩 꺼냄
/// </summary>
public class GrillStation : Interactable
{
    public enum GrillStage { Idle, Grilling, Done, Burned }

    [Header("타이머 (초)")]
    public float grillTime     = 15f; // 굽는 시간
    public float collectWindow = 8f;  // Done 후 수거 가능 시간 (초과 시 탐)

    GrillStage _stage = GrillStage.Idle;
    float      _timer;
    bool _hasRawPatty;
    bool _hasRawBacon;
    bool _servedPatty;
    bool _servedBacon;

    void Awake()
    {
        grillTime = Mathf.Min(grillTime, 6f);
        collectWindow = Mathf.Min(collectWindow, 6f);
    }

    void Update()
    {
        if (_stage == GrillStage.Grilling)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _stage = GrillStage.Done;
                _timer = collectWindow;
                Debug.Log("[Grill] Done");
            }
        }
        else if (_stage == GrillStage.Done)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _stage = GrillStage.Burned;
                GameManager.I?.RegisterServeFailure("불판 재료가 탔어요. 다시 준비하세요.");
                Debug.Log("[Grill] Burned");
            }
        }
    }

    public override string GetPrompt()
    {
        return _stage switch
        {
            GrillStage.Idle
                => $"불판 [{LoadedLabel()}]\n[E] 생패티/생베이컨 올리기",
            GrillStage.Grilling
                => $"굽는 중... ({_timer:F0}초 남음)",
            GrillStage.Done
                => $"{IngredientNames.Korean(NextOutput())} 완성!\n[E] 꺼내기 ({_timer:F0}초)",
            GrillStage.Burned
                => "탔어요!\n[E] 정리하기",
            _ => ""
        };
    }

    public override void Interact(PlayerHand hand)
    {
        switch (_stage)
        {
            case GrillStage.Idle:
                if (hand == null || hand.IsEmpty)
                {
                    GameManager.I?.PostFeedback("생패티 또는 생베이컨을 들고 불판을 사용하세요.", false);
                    return;
                }

                if (hand.Has(IngredientType.RawPatty) && !_hasRawPatty)
                {
                    _hasRawPatty = true;
                    hand.Drop();
                }
                else if (hand.Has(IngredientType.RawBacon) && !_hasRawBacon)
                {
                    _hasRawBacon = true;
                    hand.Drop();
                }
                else
                {
                    GameManager.I?.PostFeedback("지금 불판에 올릴 수 없는 재료예요.", false);
                    return;
                }

                GameManager.I?.PostFeedback($"불판 준비: {LoadedLabel()}", true);
                if (_hasRawPatty && _hasRawBacon)
                {
                    _stage = GrillStage.Grilling;
                    _timer = grillTime;
                    DailyBurgerRunManager.I.SetOrderStatus(FixedOrderStatus.Cooking);
                    Debug.Log("[Grill] Start");
                }
                break;

            case GrillStage.Done:
                if (hand == null || !hand.IsEmpty)
                {
                    GameManager.I?.PostFeedback("손을 비운 뒤 구운 재료를 꺼내세요.", false);
                    return;
                }

                IngredientType output = NextOutput();
                if (!hand.TryPickUp(output)) return;
                if (output == IngredientType.GrilledPatty) _servedPatty = true;
                if (output == IngredientType.GrilledBacon) _servedBacon = true;

                GameManager.I?.PostFeedback($"{IngredientNames.Korean(output)} 완성!", true);
                if (_servedPatty && _servedBacon)
                    ResetStation();
                Debug.Log($"[Grill] + {output}");
                break;

            case GrillStage.Burned:
                // 탄 재료 정리 (소실)
                ResetStation();
                Debug.Log("[Grill] Reset");
                break;

            default:
                Debug.Log("[Grill] Busy");
                break;
        }
    }

    // ── HUD/InGameHUD에서 읽는 상태 ─────────────────────────────

    /// <summary>굽기 진행률 0~1. Done/Burned는 1 반환.</summary>
    public float GetProgress() => _stage switch
    {
        GrillStage.Grilling => 1f - _timer / grillTime,
        GrillStage.Done     => 1f,
        GrillStage.Burned   => 1f,
        _                   => 0f
    };

    public bool IsGrilling => _stage == GrillStage.Grilling;
    public bool IsDone     => _stage == GrillStage.Done;
    public bool IsBurned   => _stage == GrillStage.Burned;

    public float RemainingSeconds => Mathf.Max(0f, _timer);

    IngredientType NextOutput()
        => !_servedPatty ? IngredientType.GrilledPatty : IngredientType.GrilledBacon;

    string LoadedLabel()
    {
        string patty = _hasRawPatty ? "패티 O" : "패티 -";
        string bacon = _hasRawBacon ? "베이컨 O" : "베이컨 -";
        return $"{patty} / {bacon}";
    }

    void ResetStation()
    {
        _stage = GrillStage.Idle;
        _timer = 0f;
        _hasRawPatty = false;
        _hasRawBacon = false;
        _servedPatty = false;
        _servedBacon = false;
    }
}
