using UnityEngine;

/// <summary>
/// Q로 내려놓은 재료. E로 다시 집을 수 있는 간단한 월드 오브젝트다.
/// </summary>
public class DroppedIngredient : Interactable
{
    IngredientType _itemType;
    Vector3 _basePosition;
    float _phase;

    public static DroppedIngredient Spawn(IngredientType itemType, Transform owner)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Dropped_{itemType}";
        go.transform.localScale = new Vector3(0.32f, 0.22f, 0.32f);

        Vector3 forward = owner.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 position = owner.position + forward * 0.85f;
        position.y = owner.position.y + 0.18f;
        go.transform.position = position;

        var renderer = go.GetComponent<Renderer>();
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (renderer != null && shader != null)
        {
            var material = new Material(shader);
            Color color = PlayerHand.GetColor(itemType);
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            renderer.material = material;
        }

        var dropped = go.AddComponent<DroppedIngredient>();
        dropped._itemType = itemType;
        dropped._basePosition = position;
        dropped._phase = Random.value * Mathf.PI * 2f;
        return dropped;
    }

    public override string GetPrompt()
        => $"{IngredientNames.Korean(_itemType)}\n[E] 다시 들기";

    public override void Interact(PlayerHand hand)
    {
        if (hand == null || !hand.TryPickUp(_itemType))
        {
            GameManager.I?.PostFeedback("손이 비어 있어야 집을 수 있어요.", false);
            return;
        }

        Debug.Log($"[Drop] Picked {IngredientNames.Korean(_itemType)}");
        Destroy(gameObject);
    }

    void Update()
    {
        float bob = Mathf.Sin(Time.time * 3f + _phase) * 0.025f;
        transform.position = _basePosition + Vector3.up * bob;
        transform.Rotate(0f, 35f * Time.deltaTime, 0f, Space.World);
    }
}
