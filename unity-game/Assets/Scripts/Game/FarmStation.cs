using UnityEngine;

/// <summary>
/// 실내 스마트 재배기.
/// 웹앱(100-burger)의 Farm 컴포넌트 포팅:
///   씨앗 심기 → (실시간 2시간) → 물 주기 → (2분 25초) → 수확
///
/// 웹앱과 다른 점:
///   - 웹앱: 꽃 피는 데 2시간 (실시간)
///   - Unity MVP: 같은 규칙을 PlayerPrefs + UTC 시각으로 저장
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
        Ready,      // 수확 가능 → E: 수확
        Harvested   // 오늘 수확 완료
    }

    [Header("작물 설정")]
    [Tooltip("이 재배기가 키우는 작물 종류")]
    public IngredientType cropType = IngredientType.Tomato;

    public override string GetPrompt()
    {
        string name = IngredientNames.Korean(cropType);
        var stage = Stage;
        int remaining = DailyBurgerRunManager.I.GetRemainingSeconds(cropType);

        return stage switch
        {
            FarmStage.Idle       => $"{name} 재배기\n[E] 씨앗 심기\n새싹까지 2시간",
            FarmStage.Seeded     => $"새싹 기다리는 중...\n남은 시간 {FormatRemaining(remaining)} / 총 2시간",
            FarmStage.NeedsWater => $"{name} 새싹이 났어요!\n양수기를 들고 [E] 물 주기\n물 주면 열매까지 2분 25초",
            FarmStage.Growing    => $"열매 맺는 중...\n남은 시간 {FormatRemaining(remaining)} / 총 2분 25초",
            FarmStage.Ready      => $"{name} 수확 가능!\n[E] 수확하기",
            FarmStage.Harvested  => $"{name} 오늘 수확 완료",
            _                    => ""
        };
    }

    public override void Interact(PlayerHand hand)
    {
        var run = DailyBurgerRunManager.I;

        switch (Stage)
        {
            case FarmStage.Idle:
                run.TryPlantSeed(cropType, out _);
                break;

            case FarmStage.NeedsWater:
                if (hand == null || !hand.Has(IngredientType.WateringCan))
                {
                    Debug.Log("[Farm] Can 필요");
                    return;
                }
                run.TryWater(cropType, out _);
                break;

            case FarmStage.Ready:
                if (run.TryHarvest(cropType, out _))
                {
                    // 왜 PlayerHand 대신 InventoryManager? CookStation이
                    // 여러 재료를 한 번에 요구하므로, 선반에 쌓아두는 방식이 자연스럼.
                    InventoryManager.I?.Add(cropType, 1);
                    Debug.Log($"[Farm] + {IngredientNames.Korean(cropType)}");
                }
                break;

            default:
                Debug.Log("[Farm] 아직 안 됨");
                break;
        }
    }

    // 현재 단계 읽기 (InGameHUD 등에서 사용)
    public FarmStage Stage => DailyBurgerRunManager.I.GetCropPhase(cropType) switch
    {
        CropGrowthPhase.Empty       => FarmStage.Idle,
        CropGrowthPhase.SeedPlanted => FarmStage.Seeded,
        CropGrowthPhase.SproutReady => FarmStage.NeedsWater,
        CropGrowthPhase.Watered     => FarmStage.Growing,
        CropGrowthPhase.FruitReady  => FarmStage.Ready,
        CropGrowthPhase.Harvested   => FarmStage.Harvested,
        _                           => FarmStage.Idle
    };

    static string FormatRemaining(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds % 3600) / 60;
        int s = seconds % 60;

        if (h > 0) return $"{h}시간 {m:00}분";
        return $"{m:00}:{s:00}";
    }
}
