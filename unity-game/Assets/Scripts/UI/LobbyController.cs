using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LobbyPanel에 붙이는 컨트롤러.
/// 방 코드 표시, 플레이어 슬롯 상태, 시작/뒤로 버튼 활성화.
/// </summary>
public class LobbyController : MonoBehaviour
{
    [Header("방 정보 텍스트")]
    public TMP_Text roomCodeText;
    public TMP_Text myRoleText;

    [Header("플레이어 슬롯")]
    public TMP_Text slotParentText;
    public TMP_Text slotChildText;

    [Header("버튼")]
    public Button startButton;

    void OnEnable()
    {
        if (RoomManager.I != null)
        {
            RoomManager.I.OnRoomChanged += Refresh;
            Refresh();
        }
    }

    void OnDisable()
    {
        if (RoomManager.I != null)
            RoomManager.I.OnRoomChanged -= Refresh;
    }

    void Refresh()
    {
        var rm = RoomManager.I;
        if (rm == null || rm.CurrentSession == null) return;

        // 방 코드
        if (roomCodeText)
            roomCodeText.text = $"방 코드: {rm.GetRoomCode()}";

        // 내 역할
        if (myRoleText)
            myRoleText.text = rm.MyRole == PlayerRole.Parent
                ? "내 역할: 부모"
                : "내 역할: 자녀";

        // 부모 슬롯
        bool parentIn = rm.CurrentSession.hostRole == PlayerRole.Parent
                     || (rm.CurrentSession.guestJoined && rm.CurrentSession.hostRole == PlayerRole.Child);
        // 자녀 슬롯
        bool childIn  = rm.CurrentSession.hostRole == PlayerRole.Child
                     || (rm.CurrentSession.guestJoined && rm.CurrentSession.hostRole == PlayerRole.Parent);

        if (slotParentText) slotParentText.text = parentIn ? "부모  [O] 입장" : "부모  대기 중...";
        if (slotChildText)  slotChildText.text  = childIn  ? "자녀  [O] 입장" : "자녀  대기 중...";

        // 시작 버튼: 두 명 다 있으면 활성화
        if (startButton) startButton.interactable = rm.CanStart();
    }

    // ── 버튼 핸들러 ───────────────────────────────────────────

    public void OnClick_Start()
    {
        if (RoomManager.I == null || !RoomManager.I.CanStart()) return;
        GameManager.I?.state.Reset();
        FindFirstObjectByType<UIScreenController>()?.ShowInGame();
    }

    public void OnClick_Back()
    {
        RoomManager.I.LeaveRoom();
        FindFirstObjectByType<UIScreenController>()?.ShowMainMenu();
    }
}
