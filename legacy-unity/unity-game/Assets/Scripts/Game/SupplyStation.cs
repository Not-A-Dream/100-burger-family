using UnityEngine;

/// <summary>
/// 발주대 (Supply/Order Station).
/// 웹앱(100-burger)의 OrderDesk.jsx 포팅:
///   빵, 생패티, 생베이컨, 소스를 보충해주는 스테이션.
///   웹앱은 24시간 배송이지만, Unity 로컬에선 즉시 보충.
///   쿨다운(restockCooldown)으로 남용 방지.
/// </summary>
public class SupplyStation : Interactable
{
    static readonly IngredientType[] SupplyCycle =
    {
        IngredientType.Bread,
        IngredientType.RawPatty,
        IngredientType.RawBacon,
        IngredientType.Sauce,
    };

    [Header("보충 수량")]
    public int restockAmount = 3;  // 1회 보충량

    [Header("쿨다운 (초)")]
    public float restockCooldown = 20f;

    float _cooldown;
    int _nextSupplyIndex;

    void Update()
    {
        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    public override string GetPrompt()
    {
        if (_cooldown > 0f)
            return "발주대\n잠시만요...";

        IngredientType next = SupplyCycle[_nextSupplyIndex];
        return $"발주대\n[E] {IngredientNames.Korean(next)} 들기";
    }

    public override void Interact(PlayerHand hand)
    {
        if (_cooldown > 0f)
        {
            return;
        }

        if (hand == null || !hand.IsEmpty)
        {
            GameManager.I?.PostFeedback("손을 비운 뒤 재료를 집어 주세요.", false);
            return;
        }

        var inv = InventoryManager.I;
        if (inv == null) return;

        IngredientType item = SupplyCycle[_nextSupplyIndex];
        if (!inv.Has(item))
            inv.Add(item, restockAmount);

        if (!inv.TryUse(item) || !hand.TryPickUp(item))
            return;

        _nextSupplyIndex = (_nextSupplyIndex + 1) % SupplyCycle.Length;
        _cooldown = Mathf.Min(0.35f, restockCooldown);
        GameManager.I?.PostFeedback($"{IngredientNames.Korean(item)}을(를) 들었어요.", true);
        Debug.Log($"[Supply] {item}");
    }
}
