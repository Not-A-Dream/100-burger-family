using UnityEngine;

/// <summary>
/// 플레이어가 손에 들고 있는 단일 슬롯 아이템 관리.
/// string에서 IngredientType enum으로 변경해 타입 안전성을 확보.
/// </summary>
public class PlayerHand : MonoBehaviour
{
    public IngredientType heldItem = IngredientType.None;

    public bool IsEmpty              => heldItem == IngredientType.None;
    public bool Has(IngredientType t) => heldItem == t;

    public void PickUp(IngredientType type)
    {
        heldItem = type;
        Debug.Log($"[Hand] 집음: {IngredientNames.Korean(type)}");
    }

    public void Drop()
    {
        Debug.Log($"[Hand] 내려놓음: {IngredientNames.Korean(heldItem)}");
        heldItem = IngredientType.None;
    }

    /// <summary>HUD에 표시할 한국어 이름. 빈 손이면 빈 문자열 반환.</summary>
    public string DisplayName()
        => IsEmpty ? "" : IngredientNames.Korean(heldItem);
}
