using UnityEngine;

/// <summary>
/// 조리대 스테이션.
/// 재료(Ingredient)를 들고 E를 누르면 조리 시작.
/// cookTime 초 후 손에 버거(Burger)를 쥐여줍니다.
/// </summary>
public class CookStation : Interactable
{
    [Header("설정")]
    public float cookTime = 4f;

    bool       _isCooking;
    float      _progress;
    PlayerHand _cookingHand;

    void Update()
    {
        if (!_isCooking) return;

        _progress += Time.deltaTime;
        if (_progress >= cookTime)
        {
            _isCooking = false;
            _cookingHand?.Drop();
            _cookingHand?.PickUp("Burger");
            Debug.Log("[Cook] 버거 조리 완료!");
        }
    }

    public override string GetPrompt()
    {
        if (_isCooking)
        {
            int pct = Mathf.RoundToInt(_progress / cookTime * 100f);
            return $"조리 중... {pct}%";
        }
        return "[E] 재료 조리";
    }

    public override void Interact(PlayerHand hand)
    {
        if (_isCooking)
        {
            Debug.Log("[Cook] 이미 조리 중이에요!");
            return;
        }
        if (!hand.Has("Ingredient"))
        {
            Debug.Log("[Cook] 재료가 없어요! 먼저 농장에서 수확하세요.");
            return;
        }

        _isCooking    = true;
        _progress     = 0f;
        _cookingHand  = hand;
        Debug.Log("[Cook] 조리 시작!");
    }

    /// <summary>외부(HUD 등)에서 진행률 읽기 (0~1)</summary>
    public float GetProgress() => _isCooking ? _progress / cookTime : 0f;
    public bool  IsCooking     => _isCooking;
}
