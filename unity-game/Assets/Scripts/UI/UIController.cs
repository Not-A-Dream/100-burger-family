using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text statusText;
    public TMP_Text timerText;
    public TMP_Text burgerCountText;
    public TMP_Text lastMessageText;

    [Header("Preset Messages")]
    [TextArea] public string parentPreset = "오늘도 물 줬어요 💧";
    [TextArea] public string childPreset = "햄버거 거의 다 됐어요! 🍔";

    private void Start()
    {
        if (GameManager.I == null)
        {
            Debug.LogError("GameManager not found. Create GameRoot and attach GameManager.");
            return;
        }

        GameManager.I.OnStateChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameManager.I != null)
            GameManager.I.OnStateChanged -= Refresh;
    }

    public void OnClick_Water()
    {
        GameManager.I.ParentWater();
    }

    public void OnClick_MakeBurger()
    {
        GameManager.I.ChildMakeBurger();
    }

    public void OnClick_PostParentPreset()
    {
        GameManager.I.PostPresetMessage(parentPreset);
    }

    public void OnClick_PostChildPreset()
    {
        GameManager.I.PostPresetMessage(childPreset);
    }

    private void Update()
    {
        // 타이머는 매 프레임 갱신(가볍게)
        if (GameManager.I != null && GameManager.I.state.isCooking)
        {
            int sec = GameManager.I.GetRemainingSeconds();
            timerText.text = FormatTime(sec);
        }
    }

    private void Refresh()
    {
        var s = GameManager.I.state;

        burgerCountText.text = $"누적: {s.burgerCount}개";
        lastMessageText.text = s.lastMessage;

        if (s.isCooking)
        {
            statusText.text = s.wateredToday
                ? "🍔 만들기 진행 중 (빠름)"
                : "🍔 만들기 진행 중 (느림)";
        }
        else
        {
            statusText.text = s.wateredToday
                ? "💧 오늘 물 줌 (5분 모드)"
                : "💧 오늘 물 안 줌 (10분 모드)";
            timerText.text = "00:00";
        }
    }

    private string FormatTime(int totalSeconds)
    {
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return $"{m:00}:{s:00}";
    }
}