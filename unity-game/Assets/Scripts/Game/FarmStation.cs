using UnityEngine;

/// <summary>
/// 실내 스마트 재배기.
/// 웹앱(100-burger)의 Farm 컴포넌트 포팅:
///   씨앗 심기 → (seedToWaterTime초) → 물 주기 → (growTime초) → 수확
///
/// 웹앱과 다른 점:
///   - 웹앱: 꽃 피는 데 2시간 (실시간)
///   - Unity: 10초 / 10초 (게임 시간으로 압축)
///   - 수확 결과가 PlayerHand 대신 InventoryManager로 바로 입고됨
///     (이유: 여러 재료를 쌓아두고 CookStation에서 한 번에 꺼내는 구조)
///
/// Inspector에서 cropType으로 토마토/양상추 구분.
/// 씬에 FarmStation 두 개 배치 → 각각 Tomato, Lettuce 설정.
/// </summary>
public class FarmStation : Interactable
{
    public enum FarmStage
    {
        Idle,       // 비어있음 → E: 씨앗 심기
        Seeded,     // 자라는 중 (물 주기 대기)
        NeedsWater, // 꽃 핌 → E: 물 주기
        Growing,    // 물 준 후 자라는 중
        Ready       // 수확 가능 → E: 수확
    }

    [Header("작물 설정")]
    [Tooltip("이 재배기가 키우는 작물 종류")]
    public IngredientType cropType = IngredientType.Tomato;

    [Header("타이머 (초)")]
    public float seedToWaterTime = 10f; // 심기 → 물 주기 필요까지
    public float growTime        = 10f; // 물 주기 → 수확 가능까지

    FarmStage _stage = FarmStage.Idle;
    float     _timer;

    void Update()
    {
        // Seeded: seedToWaterTime 경과 → 물 주기 필요 상태로 전환
        if (_stage == FarmStage.Seeded)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _stage = FarmStage.NeedsWater;
        }
        // Growing: growTime 경과 → 수확 가능 상태로 전환
        else if (_stage == FarmStage.Growing)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _stage = FarmStage.Ready;
        }
    }

    public override string GetPrompt()
    {
        string name = IngredientNames.Korean(cropType);
        return _stage switch
        {
            FarmStage.Idle       => $"{name} 재배기\n[E] 씨앗 심기",
            FarmStage.Seeded     => $"자라는 중... ({_timer:F0}초)\n잠시 후 물을 주세요",
            FarmStage.NeedsWater => $"{name}에 꽃이 폈어요!\n[E] 물 주기",
            FarmStage.Growing    => $"무럭무럭 자라는 중... ({_timer:F0}초)",
            FarmStage.Ready      => $"{name} 수확 가능!\n[E] 수확하기",
            _                    => ""
        };
    }

    public override void Interact(PlayerHand hand)
    {
        switch (_stage)
        {
            case FarmStage.Idle:
                // 씨앗 심기 (손에 아무것도 없어도 됨)
                _stage = FarmStage.Seeded;
                _timer = seedToWaterTime;
                Debug.Log($"[Farm] {IngredientNames.Korean(cropType)} 씨앗 심기 완료!");
                break;

            case FarmStage.NeedsWater:
                // 물 주기 → 성장 시작
                _stage = FarmStage.Growing;
                _timer = growTime;
                Debug.Log("[Farm] 물 주기 완료! 성장 시작.");
                break;

            case FarmStage.Ready:
                // 수확 → InventoryManager에 추가
                // 왜 PlayerHand 대신 InventoryManager? CookStation이
                // 여러 재료를 한 번에 요구하므로, 선반에 쌓아두는 방식이 자연스럼.
                InventoryManager.I?.Add(cropType, 1);
                _stage = FarmStage.Idle;
                Debug.Log($"[Farm] {IngredientNames.Korean(cropType)} 수확 완료! 인벤토리에 추가됨.");
                break;

            default:
                Debug.Log("[Farm] 아직 준비가 안 됐어요!");
                break;
        }
    }

    // 현재 단계 읽기 (InGameHUD 등에서 사용)
    public FarmStage Stage => _stage;
}
