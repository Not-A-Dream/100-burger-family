using System;
using UnityEngine;

/// <summary>
/// 작물 하나의 실시간 성장 상태.
///
/// 왜 MonoBehaviour가 아닌 순수 데이터인가?
///   성장 시간은 앱이 꺼져 있어도 흘러야 한다.
///   그래서 Time.deltaTime 누적값이 아니라 UTC 시각을 저장하고,
///   현재 UTC와 비교해서 단계를 계산한다.
/// </summary>
[Serializable]
public class CropGrowthState
{
    public IngredientType cropType;
    public CropGrowthPhase phase = CropGrowthPhase.Empty;
    public long seedPlantedAtUtcTicks;
    public long wateredAtUtcTicks;
    public long harvestedAtUtcTicks;

    public void Refresh(DateTime utcNow)
    {
        if (phase == CropGrowthPhase.SeedPlanted && HasElapsed(seedPlantedAtUtcTicks, utcNow, DailyBurgerRunManager.SeedToSproutSeconds))
            phase = CropGrowthPhase.SproutReady;

        if (phase == CropGrowthPhase.Watered && HasElapsed(wateredAtUtcTicks, utcNow, DailyBurgerRunManager.WaterToFruitSeconds))
            phase = CropGrowthPhase.FruitReady;
    }

    public int GetRemainingSeconds(DateTime utcNow)
    {
        return phase switch
        {
            CropGrowthPhase.SeedPlanted => Remaining(seedPlantedAtUtcTicks, utcNow, DailyBurgerRunManager.SeedToSproutSeconds),
            CropGrowthPhase.Watered     => Remaining(wateredAtUtcTicks, utcNow, DailyBurgerRunManager.WaterToFruitSeconds),
            _                           => 0
        };
    }

    public void PlantSeed(DateTime utcNow)
    {
        phase = CropGrowthPhase.SeedPlanted;
        seedPlantedAtUtcTicks = utcNow.Ticks;
        wateredAtUtcTicks = 0;
        harvestedAtUtcTicks = 0;
    }

    public void Water(DateTime utcNow)
    {
        phase = CropGrowthPhase.Watered;
        wateredAtUtcTicks = utcNow.Ticks;
    }

    public void Harvest(DateTime utcNow)
    {
        phase = CropGrowthPhase.Harvested;
        harvestedAtUtcTicks = utcNow.Ticks;
    }

    public void Reset()
    {
        phase = CropGrowthPhase.Empty;
        seedPlantedAtUtcTicks = 0;
        wateredAtUtcTicks = 0;
        harvestedAtUtcTicks = 0;
    }

    static bool HasElapsed(long startTicks, DateTime utcNow, int requiredSeconds)
        => startTicks > 0 && (utcNow - new DateTime(startTicks, DateTimeKind.Utc)).TotalSeconds >= requiredSeconds;

    static int Remaining(long startTicks, DateTime utcNow, int requiredSeconds)
    {
        if (startTicks <= 0) return requiredSeconds;
        double elapsed = (utcNow - new DateTime(startTicks, DateTimeKind.Utc)).TotalSeconds;
        return Mathf.Max(0, Mathf.CeilToInt((float)(requiredSeconds - elapsed)));
    }
}

public enum CropGrowthPhase
{
    Empty,
    SeedPlanted,
    SproutReady,
    Watered,
    FruitReady,
    Harvested
}
