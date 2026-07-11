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
        if (timerText == null) return;
        timerText.text = $"시간  {DailyBurgerRunManager.FormatSeconds(DailyBurgerRunManager.I.OrderElapsedSeconds)}";
    }

    private void Refresh()
    {
        var s = GameManager.I.state;

        if (burgerCountText) burgerCountText.text = $"완성  {s.burgerCount}";
        if (lastMessageText) lastMessageText.text = s.lastMessage;

        var order = DailyBurgerRunManager.I.CurrentOrder;
        if (statusText != null && order != null)
        {
            statusText.text = order.status switch
            {
                FixedOrderStatus.Active => "상태  재료 준비",
                FixedOrderStatus.Cooking => "상태  조리 중",
                FixedOrderStatus.ReadyToServe => "상태  전달 대기",
                FixedOrderStatus.Failed => "상태  실패 · 다시 도전",
                FixedOrderStatus.Succeeded => "상태  주문 성공",
                _ => "상태  준비"
            };
        }
    }

    private string FormatTime(int totalSeconds)
    {
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return $"{m:00}:{s:00}";
    }
}
