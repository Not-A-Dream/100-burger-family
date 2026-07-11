using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LobbyPanel 컨트롤러.
/// 방 코드 표시, 플레이어 슬롯 상태, 시작/뒤로 버튼 활성화.
///
/// 변경 사항 (Relay + Lobby 전환):
///   - 로딩 중 텍스트 (방 생성/참여 비동기 처리 중 표시)
///   - 에러 텍스트 (RoomManager.OnError 구독)
///   - 방 코드를 실제 Lobby 코드로 표시 (6자리 대문자)
/// </summary>
public class LobbyController : MonoBehaviour
{
    [Header("방 정보 텍스트")]
    public TMP_Text roomCodeText;
    public TMP_Text myRoleText;

    [Header("플레이어 슬롯")]
    public TMP_Text slotParentText;
    public TMP_Text slotChildText;

    [Header("상태 텍스트")]
    public TMP_Text statusText;   // 로딩 / 에러 메시지

    [Header("버튼")]
    public Button startButton;

    void OnEnable()
    {
        RoomManager.I.OnRoomChanged += Refresh;
        RoomManager.I.OnError       += ShowError;
        Refresh();
    }

    void OnDisable()
    {
        if (RoomManager.I == null) return;
        RoomManager.I.OnRoomChanged -= Refresh;
        RoomManager.I.OnError       -= ShowError;
    }

    void Refresh()
    {
        var rm = RoomManager.I;

        // ── 로딩 중 ────────────────────────────────────────────
        if (rm.IsBusy)
        {
            SetStatus("연결 중...");
            if (startButton) startButton.interactable = false;
            return;
        }

        // ── 세션 없음 ─────────────────────────────────────────
        if (rm.CurrentSession == null)
        {
            SetStatus("방 정보를 불러오는 중...");
            return;
        }

        ClearStatus();

        // 방 코드
        if (roomCodeText)
            roomCodeText.text = $"방 코드:  {rm.GetRoomCode()}";

        // 내 역할
        if (myRoleText)
            myRoleText.text = rm.MyRole == PlayerRole.Parent ? "내 역할: 부모" : "내 역할: 자녀";

        // 슬롯 표시
        // Host의 역할을 기준으로 부모/자녀 슬롯 채움
        bool hostIsParent = rm.CurrentSession.hostRole == PlayerRole.Parent;
        bool parentIn = hostIsParent || rm.CurrentSession.guestJoined;
        bool childIn  = !hostIsParent || rm.CurrentSession.guestJoined;

        if (slotParentText) slotParentText.text = parentIn ? "부모  [O] 입장" : "부모  대기 중...";
        if (slotChildText)  slotChildText.text  = childIn  ? "자녀  [O] 입장" : "자녀  대기 중...";

        // 시작 버튼: 두 명 모두 입장 시 활성화
        if (startButton) startButton.interactable = rm.CanStart();

        // 대기 안내
        if (!rm.CanStart())
            SetStatus("상대방이 방 코드로 입장하면 시작할 수 있어요.");
    }

    void ShowError(string msg)
    {
        SetStatus($"오류: {msg}");
        Debug.LogWarning($"[LobbyController] {msg}");
    }

    void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }

    void ClearStatus()
    {
        if (statusText) statusText.text = "";
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
