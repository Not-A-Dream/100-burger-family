using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public enum PlayerRole { None, Parent, Child }

public class RoomSession
{
    public string lobbyId;
    public string lobbyCode;
    public string relayJoinCode;
    public PlayerRole hostRole;
    public bool guestJoined;
}

/// <summary>
/// Unity Relay + Lobby 기반 온라인 방 관리 싱글톤.
///
/// 왜 Relay + Lobby를 함께 쓰나?
///   - Lobby : 방 코드 발급 및 플레이어 입장 감지 (6자리 사람 읽기 코드)
///   - Relay : NAT 통과 P2P 릴레이 서버 (실제 데이터 전송)
///   - NGO   : Relay 위에서 게임 오브젝트 네트워크 동기화
///
/// 흐름:
///   Host  → CreateRoomAsync() → Lobby 생성 + Relay 할당 → NGO StartHost
///   Guest → JoinRoomAsync()   → Lobby 참여 + Relay 연결 → NGO StartClient
/// </summary>
public class RoomManager : MonoBehaviour
{
    // ── Lazy 싱글톤 ──────────────────────────────────────────
    static RoomManager _instance;
    public static RoomManager I
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[RoomManager]");
                _instance = go.AddComponent<RoomManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[RoomManager] 자동 생성됨");
            }
            return _instance;
        }
    }

    // ── 공개 상태 ────────────────────────────────────────────
    public RoomSession   CurrentSession   { get; private set; }
    public PlayerRole    MyRole           { get; private set; }
    public CharacterType MyCharacterType  { get; private set; }
    public bool          IsHost           { get; private set; }
    public bool          IsInitialized    { get; private set; }
    public bool          IsBusy           { get; private set; }

    // ── 이벤트 ───────────────────────────────────────────────
    public event Action         OnRoomChanged;
    /// <summary>에러 발생 시 UI에 메시지 전달</summary>
    public event Action<string> OnError;

    // ── 내부 ─────────────────────────────────────────────────
    Lobby      _currentLobby;
    Coroutine  _heartbeatRoutine;
    Coroutine  _pollRoutine;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    // ═══════════════════════════════════════════════════════════
    //  UGS 초기화
    //
    //  UnityServices.InitializeAsync() → 익명 로그인
    //  NetworkManager가 씬에 없으면 자동 생성 (UnityTransport 포함)
    // ═══════════════════════════════════════════════════════════

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;
        IsBusy = true;
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            EnsureNetworkManager();
            IsInitialized = true;
            Debug.Log($"[RoomManager] UGS 초기화 완료. PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[RoomManager] UGS 초기화 실패: {e.Message}");
            OnError?.Invoke($"서버 연결 실패: {e.Message}");
        }
        finally { IsBusy = false; }
    }

    // ═══════════════════════════════════════════════════════════
    //  방 만들기 (Host)
    //
    //  1. Relay 서버에 슬롯 2개 할당 → JoinCode 받기
    //  2. Lobby 생성 시 JoinCode를 Data["relayCode"]에 숨김
    //     → 참여자는 Lobby 코드만 알면 relayCode를 자동으로 얻음
    //  3. NGO StartHost → UnityTransport가 Relay를 통해 패킷 전달
    // ═══════════════════════════════════════════════════════════

    public async Task<string> CreateRoomAsync(PlayerRole myRole, CharacterType charType = CharacterType.None)
    {
        if (charType == CharacterType.None)
            charType = myRole == PlayerRole.Parent ? CharacterType.Dad : CharacterType.Son;

        IsBusy = true;
        try
        {
            await InitializeAsync();

            // 1. Relay 할당
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(2);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log($"[RoomManager] Relay JoinCode: {relayCode}");

            // 2. Lobby 생성 (relayCode + hostRole 숨김)
            var opts = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    { "hostRole",  new DataObject(DataObject.VisibilityOptions.Member, myRole.ToString()) }
                }
            };
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync("100버거 패밀리", 2, opts);
            Debug.Log($"[RoomManager] Lobby 생성: {_currentLobby.LobbyCode}");

            // 3. NGO Host 시작
            //    RelayServerData(alloc, "dtls") : DTLS 암호화 프로토콜 사용
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(alloc, "dtls"));
            NetworkManager.Singleton.StartHost();

            // 4. 세션 저장
            CurrentSession = new RoomSession
            {
                lobbyId       = _currentLobby.Id,
                lobbyCode     = _currentLobby.LobbyCode,
                relayJoinCode = relayCode,
                hostRole      = myRole
            };
            MyRole          = myRole;
            MyCharacterType = charType;
            IsHost          = true;

            // 5. 하트비트(25초) + 폴링(2초) 시작
            _heartbeatRoutine = StartCoroutine(HeartbeatLobby());
            _pollRoutine      = StartCoroutine(PollLobby());

            OnRoomChanged?.Invoke();
            return _currentLobby.LobbyCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RoomManager] 방 생성 실패: {e.Message}");
            OnError?.Invoke($"방 생성 실패: {e.Message}");
            return null;
        }
        finally { IsBusy = false; }
    }

    // ═══════════════════════════════════════════════════════════
    //  방 참여 (Guest/Client)
    //
    //  1. Lobby 코드로 입장 → Data["relayCode"] 읽기
    //  2. relayCode로 Relay 서버에 Join → JoinAllocation 받기
    //  3. NGO StartClient → UnityTransport가 Host에 연결
    // ═══════════════════════════════════════════════════════════

    public async Task<bool> JoinRoomAsync(string lobbyCode)
    {
        lobbyCode = lobbyCode.Trim().ToUpper();
        IsBusy = true;
        try
        {
            await InitializeAsync();

            // 1. Lobby 참여
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            string relayCode    = _currentLobby.Data["relayCode"].Value;
            string hostRoleStr  = _currentLobby.Data["hostRole"].Value;
            PlayerRole hostRole = Enum.Parse<PlayerRole>(hostRoleStr);
            Debug.Log($"[RoomManager] Lobby 참여. RelayCode: {relayCode}");

            // 2. Relay 참여
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayCode);

            // 3. NGO Client 시작
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));
            NetworkManager.Singleton.StartClient();

            // 4. 세션 저장
            CurrentSession = new RoomSession
            {
                lobbyId       = _currentLobby.Id,
                lobbyCode     = _currentLobby.LobbyCode,
                relayJoinCode = relayCode,
                hostRole      = hostRole,
                guestJoined   = true
            };
            MyRole          = hostRole == PlayerRole.Parent ? PlayerRole.Child : PlayerRole.Parent;
            MyCharacterType = MyRole == PlayerRole.Parent ? CharacterType.Dad : CharacterType.Son;
            IsHost          = false;

            // 5. 폴링 시작 (게스트는 하트비트 불필요)
            _pollRoutine = StartCoroutine(PollLobby());

            OnRoomChanged?.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RoomManager] 방 참여 실패: {e.Message}");
            OnError?.Invoke($"방 참여 실패: {e.Message}");
            return false;
        }
        finally { IsBusy = false; }
    }

    // ── 상태 질의 ─────────────────────────────────────────────

    public bool   CanStart()     => CurrentSession != null && CurrentSession.guestJoined;
    public bool   IsInRoom()     => CurrentSession != null;
    public string GetRoomCode()  => CurrentSession?.lobbyCode ?? "";

    // ── 방 나가기 ─────────────────────────────────────────────

    public async void LeaveRoom()
    {
        StopAllCoroutines();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        if (_currentLobby != null)
        {
            try
            {
                if (IsHost)
                    await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                else
                    await LobbyService.Instance.RemovePlayerAsync(
                        _currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RoomManager] LeaveRoom 정리 오류: {e.Message}");
            }
        }

        _currentLobby  = null;
        CurrentSession = null;
        MyRole         = PlayerRole.None;
        IsHost         = false;
        OnRoomChanged?.Invoke();
        Debug.Log("[RoomManager] 방 나감");
    }

    // ── 구버전 호환 API (로컬 MVP 시절 동기 호출 → 비동기 래퍼) ─
    // MainMenuController 등 기존 코드가 수정 없이 컴파일되도록 유지.
    // 실제 결과는 OnRoomChanged 이벤트로 수신.

    public string CreateRoom(PlayerRole myRole, CharacterType charType = CharacterType.None)
    {
        _ = CreateRoomAsync(myRole, charType);
        return "";
    }

    public bool JoinRoom(string code)
    {
        _ = JoinRoomAsync(code);
        return true;
    }

    public void ClearAllRooms()
    {
        LeaveRoom();
    }

    // ── 내부: Lobby 하트비트 ──────────────────────────────────
    // Lobby는 Host가 30초마다 핑을 보내지 않으면 자동 삭제됨.
    // 25초 주기로 SendHeartbeatPingAsync 호출.

    IEnumerator HeartbeatLobby()
    {
        var wait = new WaitForSeconds(25f);
        while (_currentLobby != null)
        {
            yield return wait;
            if (_currentLobby == null) break;
            var task = LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            while (!task.IsCompleted) yield return null;
            if (task.Exception != null)
                Debug.LogWarning($"[RoomManager] 하트비트 오류: {task.Exception.InnerException?.Message}");
        }
    }

    // ── 내부: Lobby 폴링 ─────────────────────────────────────
    // 2초 주기로 GetLobbyAsync → 게스트 입장 여부 감지.
    // Host가 Lobby를 확인해 두 번째 플레이어가 들어왔는지 알 수 있음.

    IEnumerator PollLobby()
    {
        var wait = new WaitForSeconds(2f);
        while (_currentLobby != null)
        {
            yield return wait;
            var task = LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            while (!task.IsCompleted) yield return null;

            if (task.Exception != null)
            {
                Debug.LogWarning($"[RoomManager] Lobby 폴링 오류: {task.Exception.InnerException?.Message}");
                continue;
            }

            _currentLobby = task.Result;
            if (CurrentSession == null) break;

            bool hadGuest = CurrentSession.guestJoined;
            bool hasGuest = _currentLobby.Players.Count >= 2;
            if (hasGuest != hadGuest)
            {
                CurrentSession.guestJoined = hasGuest;
                Debug.Log($"[RoomManager] 게스트 {(hasGuest ? "입장" : "퇴장")}");
                OnRoomChanged?.Invoke();
            }
        }
    }

    // ── 내부: NetworkManager 자동 생성 ───────────────────────
    // 씬에 NetworkManager가 없으면 [NetworkManager] 오브젝트를 생성.
    // NetworkManager + UnityTransport 컴포넌트를 붙이고
    // DontDestroyOnLoad로 씬 전환 시 유지.

    static void EnsureNetworkManager()
    {
        if (NetworkManager.Singleton != null) return;

        var go = new GameObject("[NetworkManager]");
        DontDestroyOnLoad(go);
        var nm = go.AddComponent<NetworkManager>();
        var ut = go.AddComponent<UnityTransport>();
        nm.NetworkConfig.NetworkTransport = ut;
        Debug.Log("[RoomManager] NetworkManager 자동 생성됨");
    }
}
