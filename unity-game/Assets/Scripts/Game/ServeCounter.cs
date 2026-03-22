using UnityEngine;

/// <summary>
/// 서빙 카운터.
/// 버거(Burger)를 들고 E를 누르면 서빙 완료 → burgerCount +1
/// </summary>
public class ServeCounter : Interactable
{
    public override string GetPrompt()
    {
        return "[E] 버거 서빙";
    }

    public override void Interact(PlayerHand hand)
    {
        if (!hand.Has("Burger"))
        {
            Debug.Log("[Serve] 버거가 없어요! 먼저 조리하세요.");
            return;
        }

        hand.Drop();
        GameManager.I?.ServeBurger();
        Debug.Log("[Serve] 버거 서빙 완료!");
    }
}
