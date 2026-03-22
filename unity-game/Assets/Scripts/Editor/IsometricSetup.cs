using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 아이소메트릭(등각) 뷰 세팅 + 씬 오브젝트 정리
/// Menu: Tools/Setup Isometric View
/// </summary>
public static class IsometricSetup
{
    [MenuItem("Tools/Isometric Setup")]
    public static void Run()
    {
        DeleteLeftoverBodies();
        SetupIsometricCamera();
        FixRoomSceneLayout();
        FixCharacterScale();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[IsometricSetup] 완료!");
    }

    // ──────────────────────────────────────────────────────────
    // 1. ChildBody / ChildHead / ChildHat 등 잔여 오브젝트 삭제
    // ──────────────────────────────────────────────────────────
    static void DeleteLeftoverBodies()
    {
        string[] killList =
        {
            "ChildBody", "ChildHead", "ChildHat",
            "MomBody",   "MomHead",
            "character-a", "PlayerCharacter",
            "New Game Object"
        };

        // 모든 루트 오브젝트 + 자식 포함 재귀 탐색
        var roots = UnityEngine.SceneManagement.SceneManager
                        .GetActiveScene().GetRootGameObjects();

        var toKill = new List<GameObject>();
        foreach (var root in roots)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (var name in killList)
                    if (t.name == name && !toKill.Contains(t.gameObject))
                        toKill.Add(t.gameObject);
            }
        }

        foreach (var go in toKill)
        {
            Debug.Log($"[IsometricSetup] 삭제: {go.name}");
            Object.DestroyImmediate(go);
        }

        Debug.Log($"[IsometricSetup] 잔여 오브젝트 {toKill.Count}개 삭제 완료");
    }

    // ──────────────────────────────────────────────────────────
    // 2. 아이소메트릭 카메라 설정
    //    - 직교(Orthographic) 투영
    //    - Euler(30, 45, 0) → 고전 게임 아이소메트릭
    //    - OrthographicSize 6 (씬 전체가 들어오는 크기)
    // ──────────────────────────────────────────────────────────
    static void SetupIsometricCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[IsometricSetup] Main Camera 없음");
            return;
        }

        cam.orthographic      = true;
        cam.orthographicSize  = 6f;
        cam.nearClipPlane     = 0.1f;
        cam.farClipPlane      = 100f;
        cam.clearFlags        = CameraClearFlags.SolidColor;
        cam.backgroundColor   = new Color(0.87f, 0.93f, 0.80f); // 연두색 배경 (귀여운 분위기)

        // 아이소메트릭 각도: Y=45°, X=30° (흔히 쓰는 2:1 비율 게임 뷰)
        cam.transform.rotation = Quaternion.Euler(30f, 45f, 0f);

        // 씬 중앙 위에서 바라보도록 위치 조정
        // forward 반대 방향으로 충분히 물러남
        Vector3 center = new Vector3(0f, 0f, 0f);
        cam.transform.position = center - cam.transform.forward * 20f;

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log("[IsometricSetup] 아이소메트릭 카메라 설정 완료 (Orthographic, 30/45°)");
    }

    // ──────────────────────────────────────────────────────────
    // 3. RoomScene 스케일·위치 정렬
    //    아이소메트릭에서 잘 보이도록 RoomScene 자체는 기본 크기(1,1,1)로 유지
    //    각 스테이션을 아이소메트릭 격자 위치에 배치
    // ──────────────────────────────────────────────────────────
    static void FixRoomSceneLayout()
    {
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene != null)
        {
            // 아이소메트릭 뷰에서는 원본 크기(0.55 말고 실제 크기)가 더 잘 보임
            roomScene.transform.localScale    = Vector3.one;
            roomScene.transform.localPosition = Vector3.zero;
            roomScene.transform.localRotation = Quaternion.identity;
            EditorUtility.SetDirty(roomScene);
            Debug.Log("[IsometricSetup] RoomScene 스케일 → (1,1,1)");
        }

        // 스테이션 아이소메트릭 격자 배치
        // 격자 간격 = 4 units. 아이소메트릭에서 자연스러운 배치:
        //   FarmBox(농장)  ← 왼쪽 뒤
        //   Grill(조리대) ← 중앙 왼쪽
        //   KitchenCounter(서빙) ← 오른쪽 앞
        SetTransform("FarmBox1",        new Vector3(-4f, 0f,  4f), new Vector3(1.5f, 1.2f, 1.5f));
        SetTransform("FarmBox2",        new Vector3(-2f, 0f,  4f), new Vector3(1.5f, 1.2f, 1.5f));
        SetTransform("Grill",           new Vector3( 0f, 0f,  2f), new Vector3(1.5f, 1.4f, 1.5f));
        SetTransform("KitchenIsland",   new Vector3( 0f, 0f,  2f), null);   // Grill과 같은 위치(자식일 경우)
        SetTransform("KitchenCounter",  new Vector3( 3f, 0f, -2f), new Vector3(1.5f, 1.2f, 1.5f));
        SetTransform("Sink",            new Vector3( 3f, 0f,  0f), new Vector3(1.2f, 1.2f, 1.2f));
        SetTransform("Fridge",          new Vector3(-4f, 0f, -2f), new Vector3(1.2f, 1.8f, 1.2f));

        Debug.Log("[IsometricSetup] 스테이션 배치 완료");
    }

    // ──────────────────────────────────────────────────────────
    // 4. character-b 크기: RoomScene이 scale(1,1,1)이 됐으므로
    //    character-b localScale 1.0 → 적절한 캐릭터 크기
    // ──────────────────────────────────────────────────────────
    static void FixCharacterScale()
    {
        var charB = FindDeep("character-b");
        if (charB == null)
        {
            Debug.LogWarning("[IsometricSetup] character-b 없음");
            return;
        }

        // 가구 높이 ~1.2~1.4 → 캐릭터는 가구와 비슷하거나 약간 작게
        charB.transform.localScale    = new Vector3(1f, 1f, 1f);
        charB.transform.localPosition = new Vector3(0f, 0f, 0f);

        // Collider 재조정 (localScale 1 기준)
        var col = charB.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            col.height = 1.6f;
            col.radius = 0.25f;
            col.center = new Vector3(0f, 0.8f, 0f);
        }

        EditorUtility.SetDirty(charB);
        Debug.Log($"[IsometricSetup] character-b 스케일 → (1,1,1)");
    }

    // ──────────────────────────────────────────────────────────
    // 헬퍼
    // ──────────────────────────────────────────────────────────
    static void SetTransform(string name, Vector3 pos, Vector3? scale)
    {
        var go = FindDeep(name);
        if (go == null) return;

        go.transform.localPosition = pos;
        if (scale.HasValue)
            go.transform.localScale = scale.Value;

        EditorUtility.SetDirty(go);
        Debug.Log($"[IsometricSetup] {name} → pos={pos}" +
                  (scale.HasValue ? $" scale={scale.Value}" : ""));
    }

    static GameObject FindDeep(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) return go;
        foreach (var root in UnityEngine.SceneManagement.SceneManager
                     .GetActiveScene().GetRootGameObjects())
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }
}
