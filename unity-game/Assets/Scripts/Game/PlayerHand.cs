using UnityEngine;

/// <summary>
/// Tracks the item currently held by the player.
/// World objects such as WateringCan attach themselves separately; inventory-only
/// items get a small visual here so the player can see what they are carrying.
/// </summary>
public class PlayerHand : MonoBehaviour
{
    const string HeldVisualName = "HeldItemVisual";

    public IngredientType heldItem = IngredientType.None;

    Transform _heldVisual;

    public bool IsEmpty => heldItem == IngredientType.None;
    public bool Has(IngredientType t) => heldItem == t;

    public void PickUp(IngredientType type)
    {
        heldItem = type;
        RefreshHeldVisual();
        Debug.Log($"[Hand] + {IngredientNames.Korean(type)}");
    }

    public void Drop()
    {
        Debug.Log($"[Hand] - {IngredientNames.Korean(heldItem)}");
        heldItem = IngredientType.None;
        ClearHeldVisual();
    }

    /// <summary>HUD에 표시할 한국어 이름. 빈 손이면 빈 문자열.</summary>
    public string DisplayName()
        => IsEmpty ? "" : IngredientNames.Korean(heldItem);

    void RefreshHeldVisual()
    {
        ClearHeldVisual();

        if (heldItem == IngredientType.None || heldItem == IngredientType.WateringCan)
            return;

        Transform parent = transform.Find(PlayerVisual.RightHandAnchorName) ?? transform;
        var visual = new GameObject(HeldVisualName);
        visual.transform.SetParent(parent, false);

        if (parent == transform)
        {
            visual.transform.localPosition = new Vector3(0.58f, 1.20f, 0.68f);
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            visual.transform.localScale = Vector3.one;
        }
        else
        {
            visual.transform.localPosition = new Vector3(0.06f, 0.04f, 0.06f);
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            visual.transform.localScale = Vector3.one;
        }

        _heldVisual = visual.transform;

        if (heldItem == IngredientType.AssembledBurger)
            BuildBurgerVisual(_heldVisual);
        else
            BuildSimpleItemVisual(_heldVisual, heldItem);
    }

    void ClearHeldVisual()
    {
        if (_heldVisual != null)
        {
            Destroy(_heldVisual.gameObject);
            _heldVisual = null;
        }

        var existing = transform.Find(HeldVisualName);
        if (existing != null)
            Destroy(existing.gameObject);

        var anchor = transform.Find(PlayerVisual.RightHandAnchorName);
        existing = anchor != null ? anchor.Find(HeldVisualName) : null;
        if (existing != null)
            Destroy(existing.gameObject);
    }

    static void BuildBurgerVisual(Transform parent)
    {
        AddSphere(parent, "TopBun", new Vector3(0f, 0.12f, 0f), new Vector3(0.34f, 0.13f, 0.34f), new Color(0.93f, 0.62f, 0.25f));
        AddBox(parent, "Lettuce", new Vector3(0f, 0.04f, 0f), new Vector3(0.38f, 0.05f, 0.32f), new Color(0.22f, 0.68f, 0.24f));
        AddBox(parent, "Patty", new Vector3(0f, -0.03f, 0f), new Vector3(0.34f, 0.08f, 0.30f), new Color(0.28f, 0.12f, 0.06f));
        AddSphere(parent, "BottomBun", new Vector3(0f, -0.11f, 0f), new Vector3(0.34f, 0.11f, 0.34f), new Color(0.90f, 0.54f, 0.20f));
    }

    static void BuildSimpleItemVisual(Transform parent, IngredientType type)
    {
        Color color = type switch
        {
            IngredientType.Tomato => new Color(0.82f, 0.12f, 0.10f),
            IngredientType.Lettuce => new Color(0.22f, 0.68f, 0.24f),
            IngredientType.Bread => new Color(0.90f, 0.54f, 0.20f),
            IngredientType.RawPatty => new Color(0.55f, 0.13f, 0.12f),
            IngredientType.RawBacon => new Color(0.78f, 0.24f, 0.18f),
            IngredientType.Sauce => new Color(0.85f, 0.08f, 0.06f),
            IngredientType.GrilledPatty => new Color(0.28f, 0.12f, 0.06f),
            IngredientType.GrilledBacon => new Color(0.50f, 0.14f, 0.08f),
            _ => new Color(0.82f, 0.82f, 0.78f),
        };

        AddBox(parent, "HeldItem", Vector3.zero, new Vector3(0.24f, 0.18f, 0.24f), color);
    }

    static GameObject AddBox(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        Destroy(go.GetComponent<BoxCollider>());
        ApplyColor(go, color);
        return go;
    }

    static GameObject AddSphere(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        Destroy(go.GetComponent<SphereCollider>());
        ApplyColor(go, color);
        return go;
    }

    static void ApplyColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;

        var shader = Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard");
        if (shader == null) return;

        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        rend.material = mat;
    }
}
