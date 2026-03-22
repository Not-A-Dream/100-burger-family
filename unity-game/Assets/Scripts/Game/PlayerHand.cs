using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어가 손에 들고 있는 아이템을 관리.
/// </summary>
public class PlayerHand : MonoBehaviour
{
    public string heldItem = "";

    public bool IsEmpty   => string.IsNullOrEmpty(heldItem);
    public bool Has(string item) => heldItem == item;

    public void PickUp(string itemName)
    {
        heldItem = itemName;
        Debug.Log($"[Hand] 집음: {itemName}");
    }

    public void Drop()
    {
        Debug.Log($"[Hand] 내려놓음: {heldItem}");
        heldItem = "";
    }

    // 아이템 한국어 이름
    public string DisplayName()
    {
        return heldItem switch
        {
            "Tomato"     => "토마토",
            "Ingredient" => "재료",
            "Burger"     => "버거",
            _            => heldItem
        };
    }
}
