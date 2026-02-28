using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Config")]
    [Tooltip("If parent watered today, cooking time is 5 minutes; otherwise 10 minutes.")]
    public int cookingMinutesIfWatered = 5;
    public int cookingMinutesIfNotWatered = 10;

    [Header("Runtime")]
    public RoomState state = new RoomState();

    public event Action OnStateChanged;

    private DateTime _lastDailyResetLocalDate; // local date boundary

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);

        // init
        state.burgerCount = 0;
        state.lastMessage = "안녕하세요! 오늘도 같이 햄버거 만들어요.";
        _lastDailyResetLocalDate = DateTime.Now.Date;
    }

    private void Update()
    {
        // Daily reset (local midnight 기준)
        if (DateTime.Now.Date != _lastDailyResetLocalDate)
        {
            _lastDailyResetLocalDate = DateTime.Now.Date;
            state.ResetDaily();
            PushSystemMessage("새로운 하루가 시작됐어요 🌞");
            NotifyChanged();
        }

        // Cooking completion check
        if (state.isCooking && DateTime.UtcNow >= state.cookingEndsAtUtc)
        {
            state.isCooking = false;
            state.burgerCount += 1;
            PushSystemMessage($"햄버거 완성! 🍔 (누적 {state.burgerCount}개)");
            NotifyChanged();
        }
    }

    // 부모: 하루 1회 물주기
    public void ParentWater()
    {
        if (state.wateredToday)
        {
            PushSystemMessage("오늘은 이미 물을 줬어요 💧");
            NotifyChanged();
            return;
        }

        state.wateredToday = true;
        PushSystemMessage("오늘 물 줬어요 💧");
        NotifyChanged();
    }

    // 자녀: 햄버거 만들기 시작
    public void ChildMakeBurger()
    {
        if (state.isCooking)
        {
            PushSystemMessage("이미 햄버거를 만들고 있어요…");
            NotifyChanged();
            return;
        }

        int minutes = state.wateredToday ? cookingMinutesIfWatered : cookingMinutesIfNotWatered;
        state.isCooking = true;
        state.cookingEndsAtUtc = DateTime.UtcNow.AddMinutes(minutes);

        if (state.wateredToday)
            PushSystemMessage($"햄버거 만들기 시작! (약 {minutes}분) 👍");
        else
            PushSystemMessage($"재료가 조금 부족해요. (약 {minutes}분) 😅");

        NotifyChanged();
    }

    // 메시지 남기기(수동)
    public void PostPresetMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        state.lastMessage = text;
        NotifyChanged();
    }

    // 시스템 메시지
    private void PushSystemMessage(string text)
    {
        state.lastMessage = text;
    }

    private void NotifyChanged()
    {
        OnStateChanged?.Invoke();
    }

    // UI에서 남은 시간 표시용
    public int GetRemainingSeconds()
    {
        if (!state.isCooking) return 0;
        var remain = state.cookingEndsAtUtc - DateTime.UtcNow;
        return Mathf.Max(0, (int)remain.TotalSeconds);
    }
}