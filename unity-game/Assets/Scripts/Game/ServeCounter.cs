using UnityEngine;

/// <summary>
/// 서빙 카운터.
/// 완성 버거(AssembledBurger)를 들고 E를 누르면 서빙 완료 → burgerCount +1
/// </summary>
public class ServeCounter : Interactable
{
    public override string GetPrompt()
        => "[E] 버거 서빙!";

    public override void Interact(PlayerHand hand)
    {
        if (!hand.Has(IngredientType.AssembledBurger))
        {
            Debug.Log("[Serve] 완성 버거가 없어요! 조리대에서 먼저 완성하세요.");
            return;
        }

        if (!DailyBurgerRunManager.I.CanServeBurgerToday(out string message))
        {
            Debug.Log($"[Serve] {message}");
            return;
        }

        hand.Drop();
        GameManager.I?.ServeBurger();
        Debug.Log("[Serve] 버거 서빙 완료!");
    }
}
