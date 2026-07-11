using UnityEngine;

/// <summary>
/// 서빙 카운터.
/// 완성 버거(AssembledBurger)를 들고 E를 누르면 서빙 완료 → burgerCount +1
/// </summary>
public class ServeCounter : Interactable
{
    public override string GetPrompt()
    {
        var order = DailyBurgerRunManager.I.CurrentOrder;
        if (order != null && order.status == FixedOrderStatus.Succeeded)
            return "주문 완료! 감사합니다.";
        return "서빙 카운터\n[E] 완성 버거 전달";
    }

    public override void Interact(PlayerHand hand)
    {
        if (hand == null || hand.IsEmpty)
        {
            GameManager.I?.PostFeedback("완성 버거를 들고 전달해 주세요.", false);
            return;
        }

        if (!hand.Has(IngredientType.AssembledBurger))
        {
            GameManager.I?.RegisterServeFailure("주문 실패: 완성 버거가 아니에요.");
            return;
        }

        if (!DailyBurgerRunManager.I.CanServeBurgerToday(out string message))
        {
            Debug.Log($"[Serve] {message}");
            return;
        }

        if (GameManager.I != null && GameManager.I.ServeBurger())
        {
            hand.Drop();
            Debug.Log("[Serve] Success");
        }
    }
}
