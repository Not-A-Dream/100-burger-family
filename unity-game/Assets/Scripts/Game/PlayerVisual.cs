using UnityEngine;

/// <summary>
/// 플레이어 게임오브젝트에 부착.
/// Start()에 RoomManager.I.MyCharacterType을 읽어
/// 몸통(Capsule) + 머리(Sphere) + 악세서리를 프리미티브로 생성합니다.
///
/// ※ 이 컴포넌트가 붙은 오브젝트 자체에는 Renderer가 없어야 합니다.
///    (PlayerController, Rigidbody, CapsuleCollider 만)
/// </summary>
public class PlayerVisual : MonoBehaviour
{
    [Header("현재 캐릭터 (런타임에 RoomManager에서 읽어옴)")]
    [SerializeField] CharacterType _currentType = CharacterType.None;

    void Start()
    {
        var type = (RoomManager.I?.MyCharacterType ?? CharacterType.Dad);
        BuildVisual(type);
    }

    /// <summary>에디터 미리보기용 — Inspector에서 값 바꾸면 즉시 반영</summary>
    public void BuildVisual(CharacterType type)
    {
        _currentType = type;

        // 기존 비주얼 자식 삭제
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        float s = CharacterData.GetBodyScale(type);

        // ── 1. 몸통 (Capsule) ─────────────────────────────
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform, false);
        body.transform.localPosition = new Vector3(0f, 0f, 0f);
        body.transform.localScale    = new Vector3(s, s * 0.55f, s); // 약간 납작하게
        Destroy(body.GetComponent<CapsuleCollider>());
        ApplyColor(body, CharacterData.GetBodyColor(type));

        // ── 2. 머리 (Sphere) ──────────────────────────────
        float headY = s * 0.55f + s * 0.45f; // 몸통 top + 머리 반지름
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform, false);
        head.transform.localPosition = new Vector3(0f, headY, 0f);
        head.transform.localScale    = new Vector3(s * 0.45f, s * 0.45f, s * 0.45f);
        Destroy(head.GetComponent<SphereCollider>());
        ApplyColor(head, CharacterData.GetSkinColor(type));

        // ── 3. 악세서리 ──────────────────────────────────
        BuildAccessory(type, head.transform, s);

        // ── 4. 콜라이더 크기 재설정 ──────────────────────
        var col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            float totalH = headY + s * 0.45f;
            col.height = totalH / s * s;       // 전체 높이
            col.radius = s * 0.48f;
            col.center = new Vector3(0f, totalH / 2f, 0f);
        }

        Debug.Log($"[PlayerVisual] 캐릭터 생성: {CharacterData.GetDisplayName(type)}");
    }

    void BuildAccessory(CharacterType type, Transform headTf, float s)
    {
        switch (type)
        {
            case CharacterType.Dad:
                // 아빠: 굵은 눈썹 느낌의 작은 갈색 박스
                AddBox(headTf, "Brow",
                    new Vector3(0f, 0.3f, 0.45f),
                    new Vector3(0.6f, 0.1f, 0.1f),
                    s * 0.45f, new Color(0.35f, 0.22f, 0.10f));
                break;

            case CharacterType.Mom:
                // 엄마: 옆에 핑크 꽃 헤어핀
                AddSphere(headTf, "HairPin",
                    new Vector3(0.55f, 0.3f, 0f),
                    0.28f, s * 0.45f, CharacterData.GetAccColor(type));
                break;

            case CharacterType.Son:
                // 아들: 노란 야구 모자 (납작한 원통)
                AddBox(headTf, "Hat",
                    new Vector3(0f, 0.5f, 0f),
                    new Vector3(1.1f, 0.22f, 1.1f),
                    s * 0.45f, CharacterData.GetAccColor(type));
                // 모자 챙
                AddBox(headTf, "HatBrim",
                    new Vector3(0f, 0.3f, 0.55f),
                    new Vector3(1.0f, 0.10f, 0.45f),
                    s * 0.45f, CharacterData.GetAccColor(type));
                break;

            case CharacterType.Daughter:
                // 딸: 양쪽에 빨간 리본
                AddSphere(headTf, "Ribbon_L",
                    new Vector3(-0.55f, 0.45f, 0f),
                    0.25f, s * 0.45f, CharacterData.GetAccColor(type));
                AddSphere(headTf, "Ribbon_R",
                    new Vector3( 0.55f, 0.45f, 0f),
                    0.25f, s * 0.45f, CharacterData.GetAccColor(type));
                break;
        }
    }

    // ── 헬퍼 ────────────────────────────────────────────────────

    /// 로컬 공간에 Box Primitive 추가 (scale = parentScale * size)
    void AddBox(Transform parent, string name,
                Vector3 localPos, Vector3 localSize,
                float parentScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localSize; // 부모 스케일에 곱해짐
        Destroy(go.GetComponent<BoxCollider>());
        ApplyColor(go, color);
    }

    /// 로컬 공간에 Sphere Primitive 추가
    void AddSphere(Transform parent, string name,
                   Vector3 localPos, float radius,
                   float parentScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = Vector3.one * radius;
        Destroy(go.GetComponent<SphereCollider>());
        ApplyColor(go, color);
    }

    void ApplyColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var shader = Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard");
        if (shader == null) return;
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color",     color);
        rend.material = mat;
    }
}
