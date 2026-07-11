using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하루 1버거 제한과 랭킹용 액티브 런타임을 관리한다.
///
/// 시간 기준:
///   - 씨앗 → 새싹: 2시간. 랭킹 제외.
///   - 물 주기 → 열매: 2분 25초. 랭킹 포함.
///   - 랭킹 시작: 첫 작물에 물을 주는 순간.
///   - 랭킹 종료: 완성 버거를 서빙하는 순간.
///
/// 왜 GameManager와 분리했나?
///   GameManager는 누적 버거/스트릭/업적을 담당한다.
///   DailyBurgerRunManager는 "오늘 만들 수 있는가", "작물 시간이 지났는가",
///   "이번 기록이 몇 초인가"처럼 하루 런 규칙을 담당한다.
/// </summary>
public class DailyBurgerRunManager : MonoBehaviour
{
    // WebGL MVP: 한 세션에서 전체 loop를 확인할 수 있는 짧은 성장 시간.
    public const int SeedToSproutSeconds = 4;
    public const int WaterToFruitSeconds = 6;

    const string SaveKey = "100BurgerFamily.DailyBurgerRun.v1";
    static DailyBurgerRunManager _instance;

    public static DailyBurgerRunManager I
    {
        get
        {
            if (_instance != null) return _instance;

            var existing = FindFirstObjectByType<DailyBurgerRunManager>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var go = new GameObject("[DailyBurgerRunManager]");
            _instance = go.AddComponent<DailyBurgerRunManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    public event Action OnRunChanged;
    public event Action OnOrderChanged;

    DailyBurgerSaveData _data = new();
    Coroutine _recoverOrderRoutine;
    FixedOrderStatus _statusAfterFailure = FixedOrderStatus.Active;

    public FixedBurgerOrder CurrentOrder { get; private set; }
    public bool IsCompletedToday => _data.completedBurgerDate == TodayKey;
    public bool HasActiveRun => _data.activeRunStartedAtUtcTicks > 0 && !IsCompletedToday;
    public float LastCompletedActiveSeconds => _data.lastCompletedActiveSeconds;
    public float OrderElapsedSeconds
    {
        get
        {
            if (CurrentOrder == null || CurrentOrder.createdAtUtcTicks <= 0)
                return 0f;

            long endTicks = CurrentOrder.completedAtUtcTicks > 0
                ? CurrentOrder.completedAtUtcTicks
                : DateTime.UtcNow.Ticks;
            return Mathf.Max(0f, (float)new TimeSpan(endTicks - CurrentOrder.createdAtUtcTicks).TotalSeconds);
        }
    }

    public float ActiveRunSeconds
    {
        get
        {
            if (_data.activeRunStartedAtUtcTicks <= 0) return 0f;
            return Mathf.Max(0f, (float)(DateTime.UtcNow - new DateTime(_data.activeRunStartedAtUtcTicks, DateTimeKind.Utc)).TotalSeconds);
        }
    }

    public DailyBurgerRunStage OverallStage
    {
        get
        {
            RefreshAllCrops(false);

            if (IsCompletedToday) return DailyBurgerRunStage.CompletedToday;

            bool anySeed = false;
            bool anySprout = false;
            bool anyGrowing = false;
            bool anyFruit = false;
            bool anyHarvested = false;

            foreach (var crop in _data.crops)
            {
                anySeed     |= crop.phase == CropGrowthPhase.SeedPlanted;
                anySprout   |= crop.phase == CropGrowthPhase.SproutReady;
                anyGrowing  |= crop.phase == CropGrowthPhase.Watered;
                anyFruit    |= crop.phase == CropGrowthPhase.FruitReady;
                anyHarvested|= crop.phase == CropGrowthPhase.Harvested;
            }

            if (anyFruit) return DailyBurgerRunStage.FruitReady;
            if (anyGrowing) return DailyBurgerRunStage.GrowingFruit;
            if (anySprout) return DailyBurgerRunStage.SproutReady;
            if (anySeed) return DailyBurgerRunStage.SeedPlanted;
            if (anyHarvested) return DailyBurgerRunStage.Cooking;
            return DailyBurgerRunStage.NoSeed;
        }
    }

    string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        CreateFixedOrder();
    }

    void Update()
    {
        CheckLocalDay();
        RefreshAllCrops(true);
    }

    public CropGrowthState GetCrop(IngredientType cropType)
    {
        CheckLocalDay();
        var crop = EnsureCrop(cropType);
        crop.Refresh(DateTime.UtcNow);
        return crop;
    }

    public CropGrowthPhase GetCropPhase(IngredientType cropType) => GetCrop(cropType).phase;

    public int GetRemainingSeconds(IngredientType cropType)
        => GetCrop(cropType).GetRemainingSeconds(DateTime.UtcNow);

    public bool TryPlantSeed(IngredientType cropType, out string message)
    {
        var crop = GetCrop(cropType);
        string cropName = IngredientNames.Korean(cropType);

        if (crop.phase != CropGrowthPhase.Empty)
        {
            message = $"{cropName} 재배기는 이미 사용 중입니다.";
            return false;
        }

        crop.PlantSeed(DateTime.UtcNow);
        SaveAndNotify();
        message = $"{cropName} 심음";
        Debug.Log($"[DailyRun] {message}");
        return true;
    }

    public bool TryWater(IngredientType cropType, out string message)
    {
        var crop = GetCrop(cropType);
        string cropName = IngredientNames.Korean(cropType);

        if (IsCompletedToday)
        {
            message = "오늘의 버거는 이미 완성했습니다. 내일 다시 도전하세요.";
            return false;
        }

        if (crop.phase != CropGrowthPhase.SproutReady)
        {
            message = $"{cropName}은 아직 물 줄 단계가 아닙니다.";
            return false;
        }

        crop.Water(DateTime.UtcNow);
        StartActiveRunIfNeeded();
        SaveAndNotify();
        message = $"{cropName} 물";
        Debug.Log($"[DailyRun] {message}");
        return true;
    }

    public bool TryHarvest(IngredientType cropType, out string message)
    {
        var crop = GetCrop(cropType);
        string cropName = IngredientNames.Korean(cropType);

        if (crop.phase != CropGrowthPhase.FruitReady)
        {
            message = $"{cropName}은 아직 수확할 수 없습니다.";
            return false;
        }

        crop.Harvest(DateTime.UtcNow);
        SaveAndNotify();
        message = $"{cropName} 수확";
        Debug.Log($"[DailyRun] {message}");
        return true;
    }

    public bool CanServeBurgerToday(out string message)
    {
        CheckLocalDay();
        if (IsCompletedToday)
        {
            message = "오늘은 이미 버거를 완성했습니다.";
            return false;
        }

        message = "";
        return true;
    }

    public bool TryCompleteBurger(out float activeSeconds)
    {
        activeSeconds = 0f;
        if (!CanServeBurgerToday(out string message))
        {
            Debug.Log($"[DailyRun] {message}");
            return false;
        }

        if (CurrentOrder == null || CurrentOrder.status != FixedOrderStatus.ReadyToServe)
        {
            Debug.Log("[Order] Burger is not ready to serve");
            return false;
        }

        activeSeconds = ActiveRunSeconds;
        _data.lastCompletedActiveSeconds = activeSeconds;
        _data.completedBurgerDate = TodayKey;
        _data.activeRunStartedAtUtcTicks = 0;
        CurrentOrder.status = FixedOrderStatus.Succeeded;
        CurrentOrder.completedAtUtcTicks = DateTime.UtcNow.Ticks;
        SaveAndNotify();
        OnOrderChanged?.Invoke();

        Debug.Log($"[DailyRun] 완료 {FormatSeconds(activeSeconds)}");
        return true;
    }

    public void ResetAllProgress()
    {
        _data = new DailyBurgerSaveData();
        EnsureDefaults();
        CreateFixedOrder();
        SaveAndNotify();
    }

    public void CreateFixedOrder()
    {
        CurrentOrder = FixedBurgerOrder.CreateFamilyClassic();
        if (IsCompletedToday)
        {
            CurrentOrder.status = FixedOrderStatus.Succeeded;
            CurrentOrder.completedAtUtcTicks = DateTime.UtcNow.Ticks;
        }
        OnOrderChanged?.Invoke();
        Debug.Log($"[Order] Created: {CurrentOrder.displayName}");
    }

    public void SetOrderStatus(FixedOrderStatus status)
    {
        if (CurrentOrder == null || CurrentOrder.status == FixedOrderStatus.Succeeded)
            return;

        if (CurrentOrder.status == status)
            return;

        CurrentOrder.status = status;
        OnOrderChanged?.Invoke();
        Debug.Log($"[Order] {status}");
    }

    public void RegisterOrderFailure()
    {
        if (CurrentOrder == null || CurrentOrder.status == FixedOrderStatus.Succeeded)
            return;

        _statusAfterFailure = CurrentOrder.status == FixedOrderStatus.ReadyToServe
            ? FixedOrderStatus.ReadyToServe
            : FixedOrderStatus.Active;
        CurrentOrder.failedAttempts++;
        CurrentOrder.status = FixedOrderStatus.Failed;
        OnOrderChanged?.Invoke();

        if (_recoverOrderRoutine != null)
            StopCoroutine(_recoverOrderRoutine);
        _recoverOrderRoutine = StartCoroutine(RecoverOrderAfterFailure());
    }

    IEnumerator RecoverOrderAfterFailure()
    {
        yield return new WaitForSeconds(1.25f);
        _recoverOrderRoutine = null;

        if (CurrentOrder != null && CurrentOrder.status == FixedOrderStatus.Failed)
        {
            CurrentOrder.status = _statusAfterFailure;
            OnOrderChanged?.Invoke();
        }
    }

    public void DebugCompleteAllCrops()
    {
        DateTime utcNow = DateTime.UtcNow;
        foreach (var crop in _data.crops)
        {
            crop.PlantSeed(utcNow.AddSeconds(-SeedToSproutSeconds));
            crop.phase = CropGrowthPhase.FruitReady;
            crop.wateredAtUtcTicks = utcNow.AddSeconds(-WaterToFruitSeconds).Ticks;
        }

        StartActiveRunIfNeeded();
        SaveAndNotify();
        Debug.Log("[DailyRun] 테스트 완료");
    }

    public static string FormatSeconds(float seconds)
    {
        int total = Mathf.Max(0, Mathf.RoundToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }

    void StartActiveRunIfNeeded()
    {
        if (_data.activeRunStartedAtUtcTicks > 0) return;
        _data.activeRunStartedAtUtcTicks = DateTime.UtcNow.Ticks;
        _data.activeRunDate = TodayKey;
    }

    void CheckLocalDay()
    {
        string today = TodayKey;
        if (_data.lastSeenLocalDate == today) return;

        _data.lastSeenLocalDate = today;

        // 전날 이미 수확한 재배기는 새 날에 다시 심을 수 있게 비운다.
        foreach (var crop in _data.crops)
        {
            if (crop.phase == CropGrowthPhase.Harvested)
                crop.Reset();
        }

        SaveAndNotify();
    }

    void RefreshAllCrops(bool notifyOnChange)
    {
        DateTime utcNow = DateTime.UtcNow;
        bool changed = false;

        foreach (var crop in _data.crops)
        {
            var before = crop.phase;
            crop.Refresh(utcNow);
            changed |= before != crop.phase;
        }

        if (changed)
        {
            Save();
            if (notifyOnChange) OnRunChanged?.Invoke();
        }
    }

    CropGrowthState EnsureCrop(IngredientType cropType)
    {
        EnsureDefaults();
        foreach (var crop in _data.crops)
            if (crop.cropType == cropType)
                return crop;

        var created = new CropGrowthState { cropType = cropType };
        _data.crops.Add(created);
        Save();
        return created;
    }

    void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (!string.IsNullOrEmpty(json))
            _data = JsonUtility.FromJson<DailyBurgerSaveData>(json) ?? new DailyBurgerSaveData();

        EnsureDefaults();
        CheckLocalDay();
        RefreshAllCrops(false);
    }

    void EnsureDefaults()
    {
        if (_data.crops == null)
            _data.crops = new List<CropGrowthState>();

        EnsureCropExists(IngredientType.Tomato);
        EnsureCropExists(IngredientType.Lettuce);

        if (string.IsNullOrEmpty(_data.lastSeenLocalDate))
            _data.lastSeenLocalDate = TodayKey;
    }

    void EnsureCropExists(IngredientType cropType)
    {
        foreach (var crop in _data.crops)
            if (crop.cropType == cropType)
                return;

        _data.crops.Add(new CropGrowthState { cropType = cropType });
    }

    void SaveAndNotify()
    {
        Save();
        OnRunChanged?.Invoke();
    }

    void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data));
        PlayerPrefs.Save();
    }
}

[Serializable]
public class FixedBurgerOrder
{
    public string id;
    public string displayName;
    public FixedOrderStatus status;
    public int failedAttempts;
    public long createdAtUtcTicks;
    public long completedAtUtcTicks;
    public List<IngredientType> requiredIngredients = new();

    public static FixedBurgerOrder CreateFamilyClassic()
    {
        return new FixedBurgerOrder
        {
            id = "family_classic",
            displayName = "패밀리 클래식 버거",
            status = FixedOrderStatus.Active,
            createdAtUtcTicks = DateTime.UtcNow.Ticks,
            requiredIngredients = new List<IngredientType>
            {
                IngredientType.Bread,
                IngredientType.GrilledPatty,
                IngredientType.GrilledBacon,
                IngredientType.Lettuce,
                IngredientType.Tomato,
                IngredientType.Sauce,
            }
        };
    }
}

public enum FixedOrderStatus
{
    Active,
    Cooking,
    ReadyToServe,
    Failed,
    Succeeded
}

[Serializable]
public class DailyBurgerSaveData
{
    public string lastSeenLocalDate;
    public string completedBurgerDate;
    public string activeRunDate;
    public long activeRunStartedAtUtcTicks;
    public float lastCompletedActiveSeconds;
    public List<CropGrowthState> crops = new();
}

public enum DailyBurgerRunStage
{
    NoSeed,
    SeedPlanted,
    SproutReady,
    GrowingFruit,
    FruitReady,
    Cooking,
    Serving,
    CompletedToday
}
