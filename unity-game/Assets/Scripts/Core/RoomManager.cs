using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerRole { None, Parent, Child }

public class RoomSession
{
    public string roomCode;
    public PlayerRole hostRole;
    public bool guestJoined;
}

/// <summary>
/// 로컬 MVP용 방 생성/참여 관리 싱글톤.
/// 같은 디바이스에서 두 플레이어가 번갈아 사용하는 구조.
/// </summary>
public class RoomManager : MonoBehaviour
{
    // ── 씬에 없어도 자동 생성되는 Lazy 싱글톤 ──────────────────
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
                Debug.Log("[RoomManager] 자동 생성됨 (씬에 없었음)");
            }
            return _instance;
        }
    }

    public RoomSession  CurrentSession    { get; private set; }
    public PlayerRole   MyRole            { get; private set; }
    public CharacterType MyCharacterType  { get; private set; }
    public bool         IsHost            { get; private set; }

    // 같은 앱 안에서 방 공유 (로컬 MVP)
    static readonly Dictionary<string, RoomSession> _sessions = new();

    public event Action OnRoomChanged;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 방 만들기 ─────────────────────────────────────────────

    /// <summary>방 생성. 캐릭터 타입 포함. 랜덤 4자리 코드 반환.</summary>
    public string CreateRoom(PlayerRole myRole, CharacterType charType = CharacterType.None)
    {
        // charType이 None이면 역할에 맞는 기본값 사용
        if (charType == CharacterType.None)
            charType = myRole == PlayerRole.Parent ? CharacterType.Dad : CharacterType.Son;

        string code  = GenerateCode();
        var session  = new RoomSession { roomCode = code, hostRole = myRole };
        _sessions[code] = session;

        CurrentSession   = session;
        MyRole           = myRole;
        MyCharacterType  = charType;
        IsHost           = true;

        Debug.Log($"[RoomManager] 방 생성: {code} / 역할: {myRole} / 캐릭터: {charType}");
        OnRoomChanged?.Invoke();
        return code;
    }

    // ── 방 참여 ───────────────────────────────────────────────

    /// <summary>코드로 방 참여. 성공 시 true.</summary>
    public bool JoinRoom(string code)
    {
        code = code.Trim();
        if (!_sessions.TryGetValue(code, out var session))
        {
            Debug.Log($"[RoomManager] 방 없음: {code}");
            return false;
        }
        if (session.guestJoined)
        {
            Debug.Log($"[RoomManager] 방이 꽉 참: {code}");
            return false;
        }

        session.guestJoined = true;
        CurrentSession = session;
        MyRole  = session.hostRole == PlayerRole.Parent ? PlayerRole.Child : PlayerRole.Parent;
        IsHost  = false;

        Debug.Log($"[RoomManager] 방 참여: {code} / 역할: {MyRole}");
        OnRoomChanged?.Invoke();
        return true;
    }

    // ── 상태 질의 ─────────────────────────────────────────────

    /// <summary>두 플레이어 모두 입장하면 누구나 시작 가능.</summary>
    public bool CanStart() => CurrentSession != null && CurrentSession.guestJoined;

    public bool IsInRoom() => CurrentSession != null;

    public string GetRoomCode() => CurrentSession?.roomCode ?? "";

    // ── 방 나가기 ─────────────────────────────────────────────

    /// <summary>
    /// 현재 플레이어만 연결 해제. 방은 _sessions에 유지됨.
    /// 로컬 MVP에서 다른 역할로 재입장 가능하게 하기 위해 방 삭제 안 함.
    /// </summary>
    public void LeaveRoom()
    {
        if (CurrentSession != null && !IsHost)
        {
            // 게스트가 나가면 슬롯 다시 비워줌
            CurrentSession.guestJoined = false;
        }
        CurrentSession = null;
        MyRole = PlayerRole.None;
        IsHost = false;
        OnRoomChanged?.Invoke();
        Debug.Log("[RoomManager] 방 나감 (방은 유지)");
    }

    // ── 테스트용 전체 초기화 ─────────────────────────────────

    /// <summary>모든 방 삭제 + 현재 세션 초기화. 테스트용.</summary>
    public void ClearAllRooms()
    {
        _sessions.Clear();
        CurrentSession = null;
        MyRole = PlayerRole.None;
        IsHost = false;
        OnRoomChanged?.Invoke();
        Debug.Log("[RoomManager] 모든 방 초기화 완료");
    }

    // ── 내부 ──────────────────────────────────────────────────

    static string GenerateCode()
    {
        string code;
        do { code = UnityEngine.Random.Range(1000, 9999).ToString(); }
        while (_sessions.ContainsKey(code));
        return code;
    }
}
