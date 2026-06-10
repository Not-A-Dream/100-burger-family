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
    [Header("보충 수량")]
    public int restockAmount = 3;  // 1회 보충량

    [Header("쿨다운 (초)")]
    public float restockCooldown = 20f;

    float _cooldown;

    void Update()
    {
        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    public override string GetPrompt()
    {
        if (_cooldown > 0f)
            return $"발주대\n재충전 중... ({_cooldown:F0}초)";

        return $"발주대\n[E] 재료 보충 (+{restockAmount}씩)";
    }

    public override void Interact(PlayerHand hand)
    {
        if (_cooldown > 0f)
        {
            Debug.Log("[Supply] Cooldown");
            return;
        }

        var inv = InventoryManager.I;
        if (inv == null) return;

        inv.Add(IngredientType.Bread,    restockAmount);
        inv.Add(IngredientType.RawPatty, restockAmount);
        inv.Add(IngredientType.RawBacon, restockAmount);
        inv.Add(IngredientType.Sauce,    restockAmount);

        _cooldown = restockCooldown;
        Debug.Log($"[Supply] +{restockAmount}");
    }
}
