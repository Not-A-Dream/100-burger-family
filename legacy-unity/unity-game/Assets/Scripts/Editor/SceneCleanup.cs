using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 씬 정리 + 말풍선 프롬프트 추가
/// Menu: Tools/Clean Scene + Add Bubbles
/// Menu: Tools/Fix Scale and Delete Yellow Heads
/// </summary>
public static class SceneCleanup
{
    [MenuItem("Tools/Clean Scene + Add Bubbles")]
    public static void Run()
    {
        DeleteExtraCharacters();
        ScaleDownScene();
        AddInteractionBubbles();
        SetupPlayerOnCharacterB();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SceneCleanup] 완료!");
    }

    // ─────────────────────────────────────────────────────────
    // ★ 새 메뉴: 노란머리 삭제 + 캐릭터 스케일 + 오브젝트 정렬
    // ─────────────────────────────────────────────────────────
    [MenuItem("Tools/Fix Scale and Delete Yellow Heads")]
    public static void FixScaleAndHeads()
    {
        DeleteYellowHeads();
        FixCharacterBScale();
        NormalizeStations();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SceneCleanup] Fix 완료!");
    }

    // ─────────────────────────────────────────────────────────────────
    // ★★ 완전 초기화 + 아이소메트릭 설정
    //     Menu: Tools/Full Reset Isometric
    // ─────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Full Reset Isometric")]
    public static void FullResetIsometric()
    {
        // ── 1. 잔여 오브젝트 전부 삭제 ───────────────────────────────
        string[] killList = {
            "ChildBody",  "ChildHead",  "ChildHat",
            "Child1Body", "Child1Head", "Child1Hat",
            "Child2Body", "Child2Head", "Child2Hat",
            "MomBody","MomHead","character-a","PlayerCharacter","New Game Object"
        };
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        var toKill = new List<GameObject>();
        foreach (var root in roots)
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                foreach (var kn in killList)
                    if (t.name == kn && !toKill.Contains(t.gameObject))
                        toKill.Add(t.gameObject);
        foreach (var go in toKill) { Debug.Log($"[FullReset] 삭제: {go.name}"); Object.DestroyImmediate(go); }
        Debug.Log($"[FullReset] {toKill.Count}개 오브젝트 삭제 완료");

        // ── 2. 카메라 설정 (Undo.RecordObject 방식 — 에디터 저장 보증) ──
        var cam = Camera.main;
        if (cam != null)
        {
            // IsometricCameraController 컴포넌트 부착 (리플렉션)
            var ctrlType = System.Type.GetType("IsometricCameraController");
            if (ctrlType != null && cam.GetComponent(ctrlType) == null)
            {
                cam.gameObject.AddComponent(ctrlType);
                Debug.Log("[FullReset] IsometricCameraController 부착 완료");
            }

            // Camera 컴포넌트 수정
            Undo.RecordObject(cam, "Isometric Camera");
            cam.orthographic     = true;
            cam.orthographicSize = 13f;  // RoomScene 2.25배에 맞춰 확장
            cam.nearClipPlane    = 0.1f;
            cam.farClipPlane     = 300f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.85f, 0.92f, 0.78f);
            EditorUtility.SetDirty(cam);

            // Transform 수정 — SE 모서리에서 NW를 바라봄 (Y=-45°)
            var rot = Quaternion.Euler(30f, -45f, 0f);
            var fwd = rot * Vector3.forward;
            var camPos = Vector3.zero - fwd * 60f;   // 방이 커졌으므로 카메라 더 뒤로
            Undo.RecordObject(cam.transform, "Isometric Camera Transform");
            cam.transform.rotation = rot;
            cam.transform.position = camPos;
            EditorUtility.SetDirty(cam.transform);

            Debug.Log($"[FullReset] 카메라 → 아이소메트릭 직교 pos={camPos:F1}");
        }

        // ── 3. RoomScene 스케일 확장 (1.5 × 1.5 = 2.25) ──────────────
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene != null)
        {
            Undo.RecordObject(roomScene.transform, "RoomScene Scale");
            // 2.25: 사용자 요청 1.5배 추가 확장 (이전 1.5 × 1.5 = 2.25)
            roomScene.transform.localScale    = new Vector3(2.25f, 2.25f, 2.25f);
            roomScene.transform.localPosition = Vector3.zero;
            EditorUtility.SetDirty(roomScene);
            Debug.Log("[FullReset] RoomScene → scale(2.25)");
        }

        // ── 4. 스테이션 배치 (RoomScene 자식 → localPos 기준)
        // RoomScene scale=2.25이므로 localPos는 동일하게, 세계 좌표는 2.25배
        // 가구 localScale은 비례해서 줄여 방 안에서 적당한 크기로
        IsoPlace("FarmBox1",       new Vector3(-2.5f, 0f,  1.5f), new Vector3(0.65f,0.55f,0.65f));
        IsoPlace("FarmBox2",       new Vector3(-1f,   0f,  1.5f), new Vector3(0.65f,0.55f,0.65f));
        IsoPlace("Grill",          new Vector3( 0.5f, 0f,  0.5f), new Vector3(0.65f,0.60f,0.65f));
        IsoPlace("KitchenCounter", new Vector3( 2f,   0f, -1f  ), new Vector3(0.65f,0.55f,0.65f));
        IsoPlace("Sink",           new Vector3( 2f,   0f,  1f  ), new Vector3(0.55f,0.55f,0.55f));
        IsoPlace("Fridge",         new Vector3(-2.5f, 0f, -1f  ), new Vector3(0.55f,0.75f,0.55f));

        // ── 5. character-b ─────────────────────────────────────────
        var charB = FindDeep("character-b");
        if (charB != null)
        {
            Undo.RecordObject(charB.transform, "CharB Scale");
            // RoomScene scale=2.25 × localScale=0.1 → 세계 크기 0.225 (현재의 1/10)
            charB.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);
            charB.transform.localPosition = new Vector3(0f, 0f, 0f);
            EditorUtility.SetDirty(charB);

            var col = charB.GetComponent<CapsuleCollider>();
            if (col != null)
            {
                Undo.RecordObject(col, "CharB Collider");
                col.height = 1.6f; col.radius = 0.25f;
                col.center = new Vector3(0f, 0.8f, 0f);
                EditorUtility.SetDirty(col);
            }
            Debug.Log("[FullReset] character-b → scale(0.1, world≈0.225)");
        }

        // ── 6. 말풍선 offset ─────────────────────────────────────────
        var bubbles = Object.FindObjectsByType<InteractionBubble>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in bubbles)
        {
            Undo.RecordObject(b, "Bubble Offset");
            b.offset = new Vector3(0f, 0.5f, 0f);  // 캐릭터 1/10 크기에 맞춤
            EditorUtility.SetDirty(b);
        }
        if (bubbles.Length > 0) Debug.Log($"[FullReset] 말풍선 offset 재조정: {bubbles.Length}개");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FullReset] 완전 초기화 완료! Play 모드로 확인하세요.");
    }

    static void IsoPlace(string name, Vector3 pos, Vector3 scale)
    {
        var go = FindDeep(name);
        if (go == null) return;
        Undo.RecordObject(go.transform, $"IsoPlace {name}");
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        EditorUtility.SetDirty(go.transform);
        Debug.Log($"[FullReset] {name} → pos={pos} scale={scale}");
    }

    // ─────────────────────────────────────────────────────────
    // A. 노란 머리 오브젝트(MomBody/MomHead/ChildBody/ChildHead/ChildHat) 삭제
    // ─────────────────────────────────────────────────────────
    static void DeleteYellowHeads()
    {
        string[] targets =
        {
            "MomBody", "MomHead",
            "ChildBody", "ChildHead", "ChildHat",
            // 혹시 남아있는 구체/캡슐 마커 추가
            "character-a", "PlayerCharacter", "New Game Object"
        };

        foreach (var name in targets)
        {
            // 씬 전체 재귀 탐색 (비활성 포함)
            while (true)
            {
                var go = FindDeep(name);
                if (go == null) break;
                Debug.Log($"[SceneCleanup] 노란머리 삭제: {go.name} (경로: {GetPath(go)})");
                Object.DestroyImmediate(go);
            }
        }

        // 이름과 무관하게 Primitive 구체/캡슐 중 Renderer.material이 노란색인 것 삭제
        var allMF = Object.FindObjectsByType<MeshFilter>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        var toDelete = new List<GameObject>();
        foreach (var mf in allMF)
        {
            if (mf.sharedMesh == null) continue;
            var meshName = mf.sharedMesh.name.ToLower();
            if (meshName != "sphere" && meshName != "capsule") continue;

            var rend = mf.GetComponent<Renderer>();
            if (rend == null || rend.sharedMaterial == null) continue;
            Color c = rend.sharedMaterial.color;
            // 노란/살구색 판별: R>0.8, G>0.6, B<0.6
            if (c.r > 0.8f && c.g > 0.55f && c.b < 0.6f)
                toDelete.Add(mf.gameObject);
        }
        foreach (var go in toDelete)
        {
            Debug.Log($"[SceneCleanup] 노란 Primitive 삭제: {go.name}");
            Object.DestroyImmediate(go);
        }

        Debug.Log("[SceneCleanup] 노란머리 삭제 완료");
    }

    // ─────────────────────────────────────────────────────────
    // B. character-b 스케일을 가구 크기에 맞게 조정
    // ─────────────────────────────────────────────────────────
    static void FixCharacterBScale()
    {
        var charB = FindDeep("character-b");
        if (charB == null)
        {
            Debug.LogWarning("[SceneCleanup] character-b를 찾을 수 없습니다.");
            return;
        }

        // RoomScene 안에 있는 경우 local scale 기준으로 조정
        // 기존 scale이 (1,1,1)이면 가구 대비 너무 작음 → 3배 확대
        // 이미 조정된 경우(x > 1.5) 재조정 안 함
        if (charB.transform.localScale.x < 1.5f)
        {
            charB.transform.localScale = new Vector3(3f, 3f, 3f);
            EditorUtility.SetDirty(charB);
            Debug.Log($"[SceneCleanup] character-b 스케일 → (3, 3, 3)");
        }
        else
        {
            Debug.Log($"[SceneCleanup] character-b 스케일 이미 조정됨: {charB.transform.localScale}");
        }

        // CapsuleCollider 크기도 스케일에 맞게 재조정
        var col = charB.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            // 스케일 3x 적용 시 월드 높이 ≈ 0.55*3*1.6 = 2.64 → col은 local 기준
            // localScale (3,3,3)이므로 col은 1/3 크기로 줄임
            col.height = 0.55f;
            col.radius = 0.12f;
            col.center = new Vector3(0f, 0.27f, 0f);
            EditorUtility.SetDirty(charB);
            Debug.Log("[SceneCleanup] CapsuleCollider 재조정");
        }
    }

    // ─────────────────────────────────────────────────────────
    // C. 스테이션 스케일 및 위치 정규화
    //    RoomScene(0.55) 안의 오브젝트이므로 localPosition 기준
    // ─────────────────────────────────────────────────────────
    static void NormalizeStations()
    {
        // (오브젝트 이름, 게임플레이 localPosition, 적절한 localScale)
        var stationData = new (string[] names, Vector3 pos, Vector3 scale)[]
        {
            // 농장 — 왼쪽 뒤
            (new[]{"FarmBox1","FarmSoil1","tomato","Tomato1","FarmStation_Marker"},
             new Vector3(-4.5f, 0f,  4.5f), new Vector3(2f, 1f, 2f)),
            // 그릴(조리) — 가운데
            (new[]{"Grill","KitchenIsland","Stove","CookStation_Marker"},
             new Vector3( 0f,   0f,  2f  ), new Vector3(2f, 1f, 2f)),
            // 서빙 카운터 — 오른쪽 앞
            (new[]{"KitchenCounter","CounterTop","Sink","ServeCounter_Marker"},
             new Vector3( 4.5f, 0f, -2f  ), new Vector3(2f, 1f, 2f)),
        };

        foreach (var (names, pos, scl) in stationData)
        {
            GameObject go = null;
            foreach (var n in names)
            {
                go = FindDeep(n);
                if (go != null) break;
            }
            if (go == null) continue;

            go.transform.localPosition = pos;
            // 스케일은 이미 큰 경우(x>=2) 강제 변경하지 않음
            if (go.transform.localScale.x < 1.9f)
                go.transform.localScale = scl;

            EditorUtility.SetDirty(go);
            Debug.Log($"[SceneCleanup] 스테이션 정렬: {go.name} pos={pos}");
        }

        // character-b 출발 위치: 가운데 약간 앞
        var cb = FindDeep("character-b");
        if (cb != null)
        {
            cb.transform.localPosition = new Vector3(0f, 0f, -1f);
            EditorUtility.SetDirty(cb);
            Debug.Log("[SceneCleanup] character-b 초기 위치 설정");
        }

        Debug.Log("[SceneCleanup] 스테이션 정규화 완료");
    }

    // ─────────────────────────────────────────────────────
    // 1. 불필요한 캐릭터 / 마커 삭제
    // ─────────────────────────────────────────────────────
    static void DeleteExtraCharacters()
    {
        string[] deleteNames =
        {
            "character-a", "PlayerCharacter",
            "FarmStation_Marker", "CookStation_Marker", "ServeCounter_Marker",
            "New Game Object"
        };

        foreach (var name in deleteNames)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
                Debug.Log($"[SceneCleanup] 삭제: {name}");
            }
        }

        // Primitive(캡슐/구체)로 만들어진 노란 마커 오브젝트 삭제
        // MeshFilter가 있고 이름에 "Marker" 또는 "Player"가 있으면 삭제
        var allObjects = Object.FindObjectsByType<MeshFilter>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        var toDelete = new List<GameObject>();
        foreach (var mf in allObjects)
        {
            if (mf.sharedMesh == null) continue;
            var meshName = mf.sharedMesh.name.ToLower();
            var goName   = mf.gameObject.name.ToLower();
            // Unity 기본 Primitive 메쉬 이름: Capsule, Sphere, Cube, Cylinder
            bool isPrimitive = meshName is "capsule" or "sphere" or "cylinder";
            bool isMarker    = goName.Contains("marker") || goName.Contains("playercharacter")
                            || goName.Contains("character-a");
            if (isPrimitive && isMarker)
                toDelete.Add(mf.gameObject);
        }
        foreach (var go in toDelete)
        {
            Debug.Log($"[SceneCleanup] 마커 삭제: {go.name}");
            Object.DestroyImmediate(go);
        }

        Debug.Log("[SceneCleanup] 불필요 오브젝트 삭제 완료");
    }

    // ─────────────────────────────────────────────────────
    // 2. RoomScene 전체 스케일 축소
    // ─────────────────────────────────────────────────────
    static void ScaleDownScene()
    {
        var roomScene = GameObject.Find("RoomScene");
        if (roomScene != null)
        {
            roomScene.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            EditorUtility.SetDirty(roomScene);
            Debug.Log("[SceneCleanup] RoomScene 스케일 → 0.55");
        }
    }

    // ─────────────────────────────────────────────────────
    // 3. 말풍선 프롬프트 추가
    // ─────────────────────────────────────────────────────
    static void AddInteractionBubbles()
    {
        // 씬의 모든 Interactable에 말풍선 추가
        var interactables = Object.FindObjectsByType<Interactable>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var inter in interactables)
        {
            var bubble = inter.GetComponent<InteractionBubble>();
            if (bubble == null)
                inter.gameObject.AddComponent<InteractionBubble>();
        }
        Debug.Log($"[SceneCleanup] 말풍선 추가: {interactables.Length}개");
    }

    // ─────────────────────────────────────────────────────
    // 4. character-b에 PlayerController 설정
    // ─────────────────────────────────────────────────────
    static void SetupPlayerOnCharacterB()
    {
        var charB = FindDeep("character-b");
        if (charB == null)
        {
            Debug.LogWarning("[SceneCleanup] character-b not found");
            return;
        }

        // MomBody 등 이전 설정 잔재 정리
        foreach (var name in new[] { "MomBody" })
        {
            var old = GameObject.Find(name);
            if (old != null)
            {
                var oldCtrl = old.GetComponent<PlayerController>();
                if (oldCtrl != null) Object.DestroyImmediate(oldCtrl);
                var oldHand = old.GetComponent<PlayerHand>();
                if (oldHand != null) Object.DestroyImmediate(oldHand);
                var oldRb   = old.GetComponent<Rigidbody>();
                if (oldRb   != null) Object.DestroyImmediate(oldRb);
            }
        }

        // 비활성화 후 컴포넌트 추가 (Awake 조기 실행 방지)
        bool wasActive = charB.activeSelf;
        charB.SetActive(false);

        // Rigidbody
        var rb = charB.GetComponent<Rigidbody>();
        if (rb == null) rb = charB.AddComponent<Rigidbody>();
        rb.constraints   = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass          = 1f;

        // Collider
        if (charB.GetComponent<Collider>() == null)
        {
            var col    = charB.AddComponent<CapsuleCollider>();
            col.height = 1.6f;
            col.radius = 0.3f;
            col.center = new Vector3(0f, 0.8f, 0f);
        }

        // PlayerHand → PlayerController 순서 중요
        if (charB.GetComponent<PlayerHand>()       == null) charB.AddComponent<PlayerHand>();
        if (charB.GetComponent<PlayerController>() == null) charB.AddComponent<PlayerController>();

        charB.SetActive(wasActive);

        EditorUtility.SetDirty(charB);
        Debug.Log($"[SceneCleanup] PlayerController → {charB.name}");
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

    static string GetPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }
}
