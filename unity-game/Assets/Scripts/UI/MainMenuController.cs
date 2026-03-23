using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenuPanel 컨트롤러 — 4캐릭터 선택 구조.
///
/// 흐름:
///   메인 메뉴
///   → "부모로 시작" → ParentCharPanel (아빠 / 엄마)
///   → "자녀로 시작" → ChildCharPanel  (아들 / 딸)
///   → 캐릭터 클릭  → InGame 진입
///
/// ※ onClick은 Start()에서 런타임 재연결 — 람다/Inspector 직렬화 불필요
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("캐릭터 선택 패널 (비어 있으면 자동 탐색)")]
    public GameObject parentCharPanel;   // 아빠/엄마 선택 오버레이
    public GameObject childCharPanel;    // 아들/딸  선택 오버레이

    [Header("방 참여 패널 (기존 유지)")]
    public GameObject joinPanel;
    public TMP_InputField joinCodeInput;
    public TMP_Text       joinErrorText;

    [Header("구 RoleSelectPanel (있으면 자동 숨김)")]
    public GameObject roleSelectPanel;

    // ══════════════════════════════════════════════════════════════
    //  초기화
    //
    //  ★ 핵심 원칙:
    //    Unity Button.onClick의 Persistent Listener(Inspector 설정)는
    //    RemoveAllListeners()로 제거 불가 → 해당 메서드 자체의 동작을 바꿔야 함.
    //
    //    람다(lambda) onClick은 씬 저장 시 직렬화되지 않으므로
    //    항상 Start()에서 런타임 재연결한다.
    // ══════════════════════════════════════════════════════════════

    void Start()
    {
        // ── 1. 캐릭터 패널 자동 탐색 ──────────────────────────────
        // UICharacterSetup이 먼저 실행되어야 하지만,
        // Inspector 슬롯이 비어 있어도 자식 탐색으로 자동 연결
        if (parentCharPanel == null)
            parentCharPanel = FindChildDeep(transform, "ParentCharPanel");
        if (childCharPanel == null)
            childCharPanel  = FindChildDeep(transform, "ChildCharPanel");

        // ── 2. 메인 버튼 런타임 재연결 ────────────────────────────
        // Btn_CreateRoom: persistent onClick이 없거나 삭제된 메서드 참조
        //   → Start()에서 직접 연결
        // Btn_JoinRoom: persistent onClick이 OnClick_ShowJoin에 연결되어 있음
        //   → OnClick_ShowJoin을 ChildSelect로 리다이렉트 (아래 참조)
        WireButton(transform, "Btn_CreateRoom", OnClick_ShowParentSelect);
        WireButton(transform, "Btn_JoinRoom",   OnClick_ShowChildSelect);

        // ── 3. 캐릭터 패널 내부 버튼 런타임 연결 ──────────────────
        // UICharacterSetup의 람다는 씬 저장 시 사라지므로
        // Start()에서 항상 재연결한다
        if (parentCharPanel != null)
        {
            WireButton(parentCharPanel.transform, "아빠로 시작", OnClick_SelectDad);
            WireButton(parentCharPanel.transform, "엄마로 시작", OnClick_SelectMom);
            WireButton(parentCharPanel.transform, "뒤로",        OnClick_Cancel);
        }
        if (childCharPanel != null)
        {
            WireButton(childCharPanel.transform, "아들로 시작",  OnClick_SelectSon);
            WireButton(childCharPanel.transform, "딸로 시작",    OnClick_SelectDaughter);
            WireButton(childCharPanel.transform, "뒤로",         OnClick_Cancel);
        }

        // ── 4. 모든 서브 패널 숨김 ────────────────────────────────
        HideAllSubPanels();
    }

    // ══════════════════════════════════════════════════════════════
    //  메인 버튼
    // ══════════════════════════════════════════════════════════════

    /// "부모로 시작" → 아빠/엄마 선택 패널
    public void OnClick_ShowParentSelect()
    {
        HideAllSubPanels();
        if (parentCharPanel) parentCharPanel.SetActive(true);
    }

    /// "자녀로 시작" → 아들/딸 선택 패널
    public void OnClick_ShowChildSelect()
    {
        HideAllSubPanels();
        if (childCharPanel) childCharPanel.SetActive(true);
    }

    /// Inspector에서 Btn_JoinRoom에 직접 연결된 경우 대비
    /// → OnClick_ShowChildSelect로 리다이렉트
    public void OnClick_ShowJoin()
    {
        OnClick_ShowChildSelect();
    }

    public void OnClick_Cancel()
    {
        HideAllSubPanels();
    }

    // ══════════════════════════════════════════════════════════════
    //  캐릭터 선택 → 게임 진입
    // ══════════════════════════════════════════════════════════════

    public void OnClick_SelectDad()      => EnterGame(CharacterType.Dad);
    public void OnClick_SelectMom()      => EnterGame(CharacterType.Mom);
    public void OnClick_SelectSon()      => EnterGame(CharacterType.Son);
    public void OnClick_SelectDaughter() => EnterGame(CharacterType.Daughter);

    // 기존 버튼 호환 (Inspector 연결이 그대로인 경우)
    public void OnClick_RoleParent() => EnterGame(CharacterType.Dad);
    public void OnClick_RoleChild()  => EnterGame(CharacterType.Son);

    // ══════════════════════════════════════════════════════════════
    //  방 참여 (기존 유지 — 향후 멀티 플레이용)
    // ══════════════════════════════════════════════════════════════

    public void OnClick_JoinConfirm()
    {
        string code = joinCodeInput != null ? joinCodeInput.text : "";
        if (RoomManager.I.JoinRoom(code))
        {
            HideAllSubPanels();
            GetScreenController()?.ShowInGame();
        }
        else
        {
            if (joinErrorText)
                joinErrorText.text = "방을 찾을 수 없어요.\n코드를 다시 확인해 주세요.";
        }
    }

    public void OnClick_CancelJoin() => HideAllSubPanels();

    // ══════════════════════════════════════════════════════════════
    //  내부
    // ══════════════════════════════════════════════════════════════

    void EnterGame(CharacterType charType)
    {
        var role = CharacterData.GetRole(charType);
        RoomManager.I.CreateRoom(role, charType);

        // ── PlayerVisual 즉시 업데이트 ──────────────────────────
        //
        //  PlayerVisual.Start()는 씬 시작 시 딱 한 번만 실행됨.
        //  이후에 다른 캐릭터를 선택해도 Start()는 재실행되지 않아
        //  기존 비주얼이 그대로 남아 있음.
        //
        //  → EnterGame() 시마다 BuildVisual()을 직접 호출해
        //    RoomManager에 방금 저장된 charType으로 즉시 재생성.
        //
        var pv = FindFirstObjectByType<PlayerVisual>();
        if (pv != null)
            pv.BuildVisual(charType);

        HideAllSubPanels();
        GameManager.I?.state.Reset();
        GetScreenController()?.ShowInGame();
    }

    void HideAllSubPanels()
    {
        if (parentCharPanel) parentCharPanel.SetActive(false);
        if (childCharPanel)  childCharPanel.SetActive(false);
        if (joinPanel)       joinPanel.SetActive(false);
        if (roleSelectPanel) roleSelectPanel.SetActive(false);
    }

    UIScreenController GetScreenController() =>
        FindFirstObjectByType<UIScreenController>();

    // ── 버튼 런타임 재연결 헬퍼 ───────────────────────────────────
    //
    //  panelTf 직접 자식 중 name을 포함하는 Button을 찾아
    //  RemoveAllListeners() 후 action 연결.
    //  (RemoveAllListeners는 런타임 리스너만 제거 — persistent는 유지)
    //
    static void WireButton(Transform panelTf, string label,
                            UnityEngine.Events.UnityAction action)
    {
        // 이름에 label이 포함된 직접 자식 탐색
        foreach (Transform child in panelTf)
        {
            if (!child.name.Contains(label)) continue;
            var btn = child.GetComponent<Button>();
            if (btn == null) continue;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            return;
        }
        // 없으면 경고 (에러 아님 — 패널 미생성 시 정상)
        // Debug.LogWarning($"[MainMenuController] 버튼 '{label}' 없음 ({panelTf.name})");
    }

    // ── inactive 포함 자식 오브젝트 탐색 ──────────────────────────
    static GameObject FindChildDeep(Transform root, string goName)
    {
        foreach (Transform child in root)
        {
            if (child.name == goName) return child.gameObject;
            var found = FindChildDeep(child, goName);
            if (found != null) return found;
        }
        return null;
    }
}
