using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 상태 관리 싱글톤.
/// 웹앱(100-burger)의 gameService.js completeBurger() 로직을 포팅:
///   - 버거 카운트 누적
///   - 스트릭 계산 (연속 일수)
///   - 업적 자동 체크 (12종)
///
/// 왜 스트릭을 GameManager에서 관리하나?
///   ServeBurger()가 호출될 때마다 날짜 기반으로 연속 달성 여부를 확인해야 하므로,
///   싱글톤인 GameManager가 적합.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Runtime")]
    public RoomState state = new RoomState();

    // ── 스트릭 (웹앱 streak 시스템 포팅) ───────────────────────
    public int CurrentStreak { get; private set; }
    public int MaxStreak     { get; private set; }
    string _lastBurgerDate = ""; // "yyyy-MM-dd" 형식

    // ── 업적 (웹앱 achievements.js 포팅) ────────────────────────
    public HashSet<string> UnlockedAchievements { get; private set; } = new();

    // ── 이벤트 ───────────────────────────────────────────────────
    public event Action        OnStateChanged;
    /// <summary>새 업적 달성 시 발생 (업적 ID, 표시 라벨)</summary>
    public event Action<string, string> OnAchievementUnlocked;

    DateTime _lastDailyResetLocalDate;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        state.burgerCount = 0;
        state.lastMessage = "안녕하세요! 오늘도 같이 햄버거 만들어요";
        _lastDailyResetLocalDate = DateTime.Now.Date;
    }

    void Update()
    {
        // 자정 기준 일일 리셋
        if (DateTime.Now.Date != _lastDailyResetLocalDate)
        {
            _lastDailyResetLocalDate = DateTime.Now.Date;
            state.ResetDaily();
            NotifyChanged();
        }
    }

    // ── 핵심 액션 ────────────────────────────────────────────────

    /// <summary>
    /// 버거 서빙 완료. ServeCounter가 호출.
    /// 웹앱 completeBurger()에 해당.
    /// </summary>
    public void ServeBurger()
    {
        var dailyRun = DailyBurgerRunManager.I;
        if (!dailyRun.TryCompleteBurger(out float activeSeconds))
        {
            state.lastMessage = "오늘은 이미 버거를 완성했습니다.";
            NotifyChanged();
            return;
        }

        state.burgerCount++;
        UpdateStreak();
        CheckAchievements();
        state.lastMessage = $"버거 완성! 기록 {DailyBurgerRunManager.FormatSeconds(activeSeconds)} / 누적 {state.burgerCount}개";
        NotifyChanged();
        Debug.Log($"[GameManager] 버거 서빙! 기록 {DailyBurgerRunManager.FormatSeconds(activeSeconds)} / 총 {state.burgerCount}개 / 스트릭 {CurrentStreak}일");
    }

    /// <summary>부모 물주기 (협력 메커니즘 유지)</summary>
    public void ParentWater()
    {
        if (state.wateredToday) return;
        state.wateredToday = true;
        state.lastMessage = "오늘 물 줬어요!";
        NotifyChanged();
    }

    public void InvokeStateChanged() => NotifyChanged();

    // ── 하위 호환 메서드 (UIController / InGameController에서 사용) ──

    /// <summary>메시지 수동 입력 (프리셋 버튼 등)</summary>
    public void PostPresetMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        state.lastMessage = text;
        NotifyChanged();
    }

    /// <summary>
    /// 구버전 버거 만들기 버튼용 스텁.
    /// 실제 조리는 이제 3D 월드 CookStation에서 진행.
    /// </summary>
    public void ChildMakeBurger()
    {
        state.lastMessage = "조리대 앞에서 E 키를 눌러 버거를 만드세요!";
        NotifyChanged();
    }

    /// <summary>
    /// 구버전 타이머 UI 호환용 스텁.
    /// 실시간 쿠킹 타이머는 CookStation이 자체 관리.
    /// </summary>
    public int GetRemainingSeconds() => 0;

    // ── 스트릭 계산 ──────────────────────────────────────────────

    /// <summary>
    /// 웹앱 completeBurger()의 streak 계산 로직 포팅.
    /// 어제 날짜에 버거를 만들었으면 +1, 아니면 1로 리셋.
    /// </summary>
    void UpdateStreak()
    {
        string today     = DateTime.Now.ToString("yyyy-MM-dd");
        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        if (_lastBurgerDate == today)
            return; // 오늘 이미 스트릭 업데이트됨

        if (_lastBurgerDate == yesterday)
            CurrentStreak++;   // 연속!
        else
            CurrentStreak = 1; // 끊겼거나 첫 시작

        _lastBurgerDate = today;

        if (CurrentStreak > MaxStreak)
            MaxStreak = CurrentStreak;
    }

    // ── 업적 체크 ────────────────────────────────────────────────

    /// <summary>
    /// 웹앱 achievements.js 12가지 업적을 Unity 로컬 버전으로 포팅.
    /// TryUnlock: 조건 충족 + 미달성이면 언락 + 이벤트 발행.
    /// </summary>
    void CheckAchievements()
    {
        int cnt    = state.burgerCount;
        int streak = CurrentStreak;

        // 버거 수 업적
        TryUnlock("first_burger", cnt >= 1,   "첫 번째 버거!");
        TryUnlock("burger_5",     cnt >= 5,   "5개 달성!");
        TryUnlock("burger_10",    cnt >= 10,  "10개 달성!");
        TryUnlock("burger_25",    cnt >= 25,  "25개 달성!");
        TryUnlock("burger_50",    cnt >= 50,  "반백 달성!");
        TryUnlock("burger_100",   cnt >= 100, "100개 전설!");

        // 스트릭 업적
        TryUnlock("streak_3",  streak >= 3,  "3일 연속!");
        TryUnlock("streak_7",  streak >= 7,  "7일 연속!");
        TryUnlock("streak_14", streak >= 14, "2주 연속!");
        TryUnlock("streak_30", streak >= 30, "한 달 연속!");
    }

    void TryUnlock(string id, bool condition, string label)
    {
        if (!condition || UnlockedAchievements.Contains(id)) return;
        UnlockedAchievements.Add(id);
        state.lastMessage = $"업적 달성: {label}";
        OnAchievementUnlocked?.Invoke(id, label);
        Debug.Log($"[Achievement] {label}");
    }

    void NotifyChanged() => OnStateChanged?.Invoke();
}
