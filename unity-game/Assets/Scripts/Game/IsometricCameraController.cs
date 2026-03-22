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
    [Tooltip("직교 투영 반높이 — 클수록 더 넓게 보임")]
    public float orthographicSize = 13f;

    [Tooltip("카메라가 바라볼 씬 중심점 (RoomScene 중앙)")]
    public Vector3 lookTarget = new Vector3(0f, 0f, 0f);

    // 아이소메트릭 각도: X=30°, Y=-45° (SE 모서리에서 NW를 바라봄 → 정면+왼쪽 벽 안 보임)
    static readonly Quaternion ISO_ROT  = Quaternion.Euler(30f, -45f, 0f);
    const float CAM_DIST = 60f;

    // 런타임에 삭제할 오브젝트 이름 (이름에 숫자 붙은 변형 포함)
    static readonly string[] KILL_LIST =
    {
        "ChildBody",  "ChildHead",  "ChildHat",
        "Child1Body", "Child1Head", "Child1Hat",
        "Child2Body", "Child2Head", "Child2Hat",
        "MomBody",    "MomHead",
        "character-a", "PlayerCharacter"
    };

    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();

        // 불필요 오브젝트 삭제 (비활성 포함 전체 탐색)
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
        // 직교 투영
        _cam.orthographic     = true;
        _cam.orthographicSize = orthographicSize;
        _cam.nearClipPlane    = 0.1f;
        _cam.farClipPlane     = 200f;
        _cam.clearFlags       = CameraClearFlags.SolidColor;
        _cam.backgroundColor  = new Color(0.85f, 0.92f, 0.78f);

        // 아이소메트릭 위치·회전
        transform.rotation = ISO_ROT;
        transform.position = lookTarget - transform.forward * CAM_DIST;

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
