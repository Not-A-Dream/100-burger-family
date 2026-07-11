using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 테스트용 디버그 패널.
/// - 방 전체 초기화
/// - 현재 방 코드 표시
/// - GameManager 상태 초기화
/// </summary>
public class DebugController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text  statusText;   // 현재 방 코드 / 상태 표시
    public GameObject panel;       // 디버그 패널 (토글)

    bool _visible = false;

    public void TogglePanel()
    {
        _visible = !_visible;
        if (panel) panel.SetActive(_visible);
        if (_visible) RefreshStatus();
    }

    public void RefreshStatus()
    {
        if (statusText == null) return;
        var rm = RoomManager.I;
        if (rm == null || !rm.IsInRoom())
        {
            statusText.text = "방: 없음";
            return;
        }
        var s = rm.CurrentSession;
        statusText.text =
            $"코드: {s.lobbyCode}\n" +
            $"호스트 역할: {s.hostRole}\n" +
            $"게스트 입장: {(s.guestJoined ? "Y" : "N")}\n" +
            $"내 역할: {rm.MyRole}\n" +
            $"버거: {(GameManager.I != null ? GameManager.I.state.burgerCount.ToString() : "-")}개";
    }

    // ── 버튼 핸들러 ───────────────────────────────────────────

    /// <summary>모든 방 + 게임 상태 초기화</summary>
    public void OnClick_ResetAll()
    {
        RoomManager.I?.ClearAllRooms();
        GameManager.I?.state.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
        RefreshStatus();
        Debug.Log("[Debug] 전체 초기화 완료");
    }

    /// <summary>현재 세션만 나가기 (방은 유지)</summary>
    public void OnClick_LeaveRoom()
    {
        RoomManager.I?.LeaveRoom();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
        RefreshStatus();
    }
}
