using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenuPanel에 붙이는 컨트롤러.
/// "방 만들기" → 역할 선택 → 로비
/// "참여하기"  → 코드 입력 → 로비
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("서브 패널")]
    public GameObject roleSelectPanel;   // 역할 선택 오버레이
    public GameObject joinPanel;         // 방 참여 오버레이

    [Header("방 참여 – 입력")]
    public TMP_InputField joinCodeInput;
    public TMP_Text       joinErrorText;

    void Start()
    {
        roleSelectPanel.SetActive(false);
        joinPanel.SetActive(false);
    }

    // ── 메인 버튼 ──────────────────────────────────────────────

    public void OnClick_CreateRoom()
    {
        joinPanel.SetActive(false);
        roleSelectPanel.SetActive(true);
    }

    public void OnClick_ShowJoin()
    {
        roleSelectPanel.SetActive(false);
        joinPanel.SetActive(true);
        if (joinErrorText) joinErrorText.text = "";
        if (joinCodeInput) joinCodeInput.text = "";
    }

    // ── 역할 선택 ──────────────────────────────────────────────

    public void OnClick_RoleParent() => CreateAndEnterLobby(PlayerRole.Parent);
    public void OnClick_RoleChild()  => CreateAndEnterLobby(PlayerRole.Child);

    public void OnClick_CancelRole()
    {
        roleSelectPanel.SetActive(false);
    }

    void CreateAndEnterLobby(PlayerRole role)
    {
        RoomManager.I.CreateRoom(role);
        roleSelectPanel.SetActive(false);
        GetScreenController()?.ShowLobby();
    }

    // ── 방 참여 ──────────────────────────────────────────────

    public void OnClick_JoinConfirm()
    {
        string code = joinCodeInput != null ? joinCodeInput.text : "";
        if (RoomManager.I.JoinRoom(code))
        {
            joinPanel.SetActive(false);
            GetScreenController()?.ShowLobby();
        }
        else
        {
            if (joinErrorText)
                joinErrorText.text = "방을 찾을 수 없어요.\n코드를 다시 확인해 주세요.";
        }
    }

    public void OnClick_CancelJoin()
    {
        joinPanel.SetActive(false);
    }

    // ── 헬퍼 ──────────────────────────────────────────────────

    UIScreenController GetScreenController() =>
        FindFirstObjectByType<UIScreenController>();
}
