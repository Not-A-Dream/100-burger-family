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
///   - 재료는 PlayerHand 없이 InventoryManager에서 바로 꺼냄/넣음
/// </summary>
public class GrillStation : Interactable
{
    public enum GrillStage { Idle, Grilling, Done, Burned }

    [Header("타이머 (초)")]
    public float grillTime     = 15f; // 굽는 시간
    public float collectWindow = 8f;  // Done 후 수거 가능 시간 (초과 시 탐)

    GrillStage _stage = GrillStage.Idle;
    float      _timer;

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
                Debug.Log("[Grill] Burned");
            }
        }
    }

    public override string GetPrompt()
    {
        var inv = InventoryManager.I;
        bool hasIngredients = inv != null
            && inv.Has(IngredientType.RawPatty)
            && inv.Has(IngredientType.RawBacon);

        return _stage switch
        {
            GrillStage.Idle when hasIngredients
                => "불판\n[E] 패티+베이컨 굽기 시작",
            GrillStage.Idle
                => $"불판 (재료 부족)\n생패티: {inv?.Count(IngredientType.RawPatty) ?? 0}개  생베이컨: {inv?.Count(IngredientType.RawBacon) ?? 0}개",
            GrillStage.Grilling
                => $"굽는 중... ({_timer:F0}초 남음)",
            GrillStage.Done
                => $"완성! [E] 꺼내기 ({_timer:F0}초 남음)",
            GrillStage.Burned
                => "탔어요!\n[E] 정리하기",
            _ => ""
        };
    }

    public override void Interact(PlayerHand hand)
    {
        var inv = InventoryManager.I;

        switch (_stage)
        {
            case GrillStage.Idle:
                // 인벤토리에서 생패티+생베이컨 꺼내 굽기 시작
                if (inv == null
                    || !inv.Has(IngredientType.RawPatty)
                    || !inv.Has(IngredientType.RawBacon))
                {
                    Debug.Log("[Grill] 재료 없음");
                    return;
                }
                inv.TryUse(IngredientType.RawPatty);
                inv.TryUse(IngredientType.RawBacon);
                _stage = GrillStage.Grilling;
                _timer = grillTime;
                Debug.Log("[Grill] Start");
                break;

            case GrillStage.Done:
                // 구운 재료를 인벤토리에 추가
                inv?.Add(IngredientType.GrilledPatty);
                inv?.Add(IngredientType.GrilledBacon);
                _stage = GrillStage.Idle;
                Debug.Log("[Grill] + Grilled");
                break;

            case GrillStage.Burned:
                // 탄 재료 정리 (소실)
                _stage = GrillStage.Idle;
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
}
