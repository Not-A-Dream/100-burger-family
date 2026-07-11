using UnityEngine;

/// <summary>
/// 아이소메트릭 카메라를 런타임에 강제 적용하고
/// 씬의 불필요한 오브젝트(ChildBody 등)를 제거합니다.
/// Main Camera에 부착합니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class IsometricCameraController : MonoBehaviour
{
    [Header("아이소메트릭 설정")]
    [Tooltip("직교 투영 반높이 — 클수록 더 넓게 보임 (10×10m 방 기준: 7)")]
    public float orthographicSize = 7f;

    [Tooltip("카메라가 바라볼 씬 중심점. y를 올리면 방이 뷰포트 아래로 내려가 TopPanel에 안 가림")]
    public Vector3 lookTarget = new Vector3(0f, 2f, 0f);

    // 아이소메트릭 각도: X=30°, Y=-45° (SE 모서리에서 NW를 바라봄)
    static readonly Quaternion ISO_ROT  = Quaternion.Euler(30f, -45f, 0f);
    const float CAM_DIST = 60f;

    // Inspector 저장값을 무시하고 항상 이 값을 강제 적용
    // (씬 파일의 직렬화된 구 값이 남아있어도 덮어씀)
    const float FORCE_SIZE   = 7f;
    static readonly Vector3 FORCE_TARGET = new Vector3(0f, 2f, 0f);

    // 런타임에 삭제할 Kenney 에셋 더미 오브젝트 이름
    // ※ 실제 플레이어("Player" 포함 이름)는 절대 포함하지 않을 것
    static readonly string[] KILL_LIST =
    {
        "ChildBody",  "ChildHead",  "ChildHat",
        "Child1Body", "Child1Head", "Child1Hat",
        "Child2Body", "Child2Head", "Child2Hat",
        "MomBody",    "MomHead",
        "character-a"
        // "PlayerCharacter" ← 제거: 실제 플레이어일 수 있음
    };

    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();

        // ── 스케일 버그 수정: RoomScene 스케일을 (1,1,1)로 강제 정규화 ──
        // 에디터 도구 실행 후 RoomScene.scale이 변경된 경우를 방지합니다.
        var roomSceneGO = GameObject.Find("RoomScene");
        if (roomSceneGO != null && roomSceneGO.transform.localScale != Vector3.one)
        {
            Debug.LogWarning($"[IsoCamera] RoomScene 스케일 이상 감지: {roomSceneGO.transform.localScale} → (1,1,1) 로 보정");
            roomSceneGO.transform.localScale = Vector3.one;
        }

        // ── Kenney 에셋 더미 오브젝트 삭제 (비활성 포함 전체 탐색) ──
        var allTransforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in allTransforms)
        {
            foreach (var killName in KILL_LIST)
            {
                if (t != null && t.name == killName)
                {
                    Debug.Log($"[IsoCamera] 런타임 삭제: {t.name}");
                    Destroy(t.gameObject);
                    break;
                }
            }
        }

        ApplyIsometric();
    }

    void ApplyIsometric()
    {
        // Inspector 저장값 무시 — const 값으로 항상 강제 적용
        orthographicSize = FORCE_SIZE;
        lookTarget       = FORCE_TARGET;

        // 직교 투영
        _cam.orthographic     = true;
        _cam.orthographicSize = FORCE_SIZE;
        _cam.nearClipPlane    = 0.1f;
        _cam.farClipPlane     = 200f;
        _cam.clearFlags       = CameraClearFlags.SolidColor;
        _cam.backgroundColor  = new Color(0.85f, 0.92f, 0.78f);

        // 아이소메트릭 위치·회전
        transform.rotation = ISO_ROT;
        transform.position = FORCE_TARGET - transform.forward * CAM_DIST;

        Debug.Log($"[IsoCamera] 카메라 적용: size={orthographicSize}, pos={transform.position:F1}");
    }

#if UNITY_EDITOR
    // 에디터에서 lookTarget 변경 시 즉시 반영
    void OnValidate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam != null) ApplyIsometric();
    }
#endif
}
