using UnityEngine;
using TMPro;

/// <summary>
/// ResultPanel 컨트롤러.
/// 완성된 버거 수 표시 + 다시 만들기 / 메인으로 이동
/// </summary>
public class ResultController : MonoBehaviour
{
    [Header("표시")]
    public TMP_Text burgerCountText;
    public TMP_Text commentText;

    void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        if (GameManager.I == null) return;
        int count = GameManager.I.state.burgerCount;

        if (burgerCountText)
            burgerCountText.text = $"오늘 버거 {count}개 완성!";

        if (commentText)
        {
            commentText.text = count == 0 ? "오늘 처음 도전이에요. 내일 또 해봐요!"
                             : count < 3  ? "잘 했어요! 내일은 더 많이 만들어봐요."
                             : count < 5  ? "대단해요! 버거 가족이 되어가고 있어요."
                                          : "최고예요! 진짜 버거 패밀리!";
        }
    }

    public void OnClick_PlayAgain()
    {
        GameManager.I?.state.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowInGame();
    }

    public void OnClick_BackToMenu()
    {
        RoomManager.I?.LeaveRoom();
        GameManager.I?.state.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
    }
}
