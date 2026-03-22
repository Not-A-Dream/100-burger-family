using UnityEngine;

/// <summary>
/// 농장 수확 스테이션.
/// 플레이어가 E를 누르면 토마토(Ingredient)를 수확합니다.
/// 수확 후 쿨다운이 지나면 다시 수확 가능.
/// </summary>
public class FarmStation : Interactable
{
    [Header("설정")]
    public float harvestCooldown = 6f;  // 수확 후 재충전 시간 (초)

    float _cooldownRemaining = 0f;
    bool  _ready => _cooldownRemaining <= 0f;

    void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;
    }

    public override string GetPrompt()
    {
        if (!_ready)
            return $"🌱 재충전 중... ({_cooldownRemaining:F0}초)";
        return "🌿 실내 스마트 재배기\n[E] 재료 수확";
    }

    public override void Interact(PlayerHand hand)
    {
        if (!_ready)
        {
            Debug.Log("[Farm] 아직 수확할 수 없어요!");
            return;
        }
        if (!hand.IsEmpty)
        {
            Debug.Log("[Farm] 손이 가득 찼어요!");
            return;
        }

        hand.PickUp("Ingredient");
        _cooldownRemaining = harvestCooldown;
        Debug.Log("[Farm] 재료 수확 완료!");
    }
}
