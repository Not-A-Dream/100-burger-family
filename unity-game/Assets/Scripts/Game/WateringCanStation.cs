using UnityEngine;

/// <summary>
/// 방 안에 비치된 양수기/물뿌리개.
///
/// 왜 재배기에 바로 물 주기 버튼을 두지 않나?
///   플레이어가 도구를 집고 재배기까지 이동해야 부모/자녀 협동과 동선 최적화가 생긴다.
///   즉 "물 주기"도 랭킹 시간에 들어가는 실제 플레이 행동이 된다.
/// </summary>
public class WateringCanStation : Interactable
{
    public override string GetPrompt()
        => "[E] 양수기 들기 / 내려놓기";

    public override void Interact(PlayerHand hand)
    {
        if (hand == null) return;

        if (hand.Has(IngredientType.WateringCan))
        {
            hand.Drop();
            Debug.Log("[WateringCan] 양수기를 제자리에 내려놓았습니다.");
            return;
        }

        if (!hand.IsEmpty)
        {
            Debug.Log("[WateringCan] 손이 가득 찼어요. 먼저 들고 있는 것을 내려놓으세요.");
            return;
        }

        hand.PickUp(IngredientType.WateringCan);
        Debug.Log("[WateringCan] 양수기를 들었습니다.");
    }
}

