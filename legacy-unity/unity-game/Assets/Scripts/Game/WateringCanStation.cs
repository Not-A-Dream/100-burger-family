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
    public static WateringCanStation HeldInstance { get; private set; }

    Renderer[] _renderers;
    Collider[] _colliders;
    InteractionBubble[] _bubbles;
    Transform _homeParent;

    void Awake()
    {
        _homeParent = transform.parent;
        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
        _bubbles = GetComponentsInChildren<InteractionBubble>(true);
    }

    public override string GetPrompt()
        => HeldInstance == this ? "" : "[E] 양수기 들기";

    public override void Interact(PlayerHand hand)
    {
        if (hand == null) return;

        if (!hand.IsEmpty)
        {
            Debug.Log("[Can] 손이 찼어요.");
            return;
        }

        hand.PickUp(IngredientType.WateringCan);
        HeldInstance = this;
        AttachToPlayerHand(hand.transform);
        Debug.Log("[Can] +");
    }

    public static bool TryDropHeld(PlayerHand hand, Transform player)
    {
        if (hand == null || player == null || !hand.Has(IngredientType.WateringCan))
            return false;

        var station = HeldInstance ?? FindFirstObjectByType<WateringCanStation>(FindObjectsInactive.Include);
        if (station == null)
        {
            hand.Drop();
            Debug.LogWarning("[Can] 내려놓을 수 없어요.");
            return false;
        }

        Vector3 dropPos = station.FindDropPosition(player);
        station.transform.SetParent(station._homeParent, true);
        station.transform.position = dropPos;
        station.transform.rotation = Quaternion.Euler(0f, player.eulerAngles.y, 0f);
        station.transform.localScale = Vector3.one;
        station.SetInteractableVisible(true);
        HeldInstance = null;

        hand.Drop();
        Debug.Log("[Can] -");
        return true;
    }

    Vector3 FindDropPosition(Transform player)
    {
        Vector3 forward = player.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 candidate = player.position + forward * 0.75f;
        candidate.y = player.position.y;

        // 어느 바닥이든 놓을 수 있게, 플레이어 앞 지점에서 아래로 쏴서 실제 Floor 높이를 찾는다.
        // Raycast는 조리대/재배기 같은 콜라이더도 먼저 맞을 수 있으므로 바닥 이름을 우선한다.
        var hits = Physics.RaycastAll(candidate + Vector3.up * 3f, Vector3.down, 8f);
        bool foundFloor = false;
        float floorY = 0f;

        foreach (var hit in hits)
        {
            if (hit.collider == null || !hit.collider.name.Contains("Floor"))
                continue;

            if (!foundFloor || hit.point.y > floorY)
            {
                floorY = hit.point.y;
                foundFloor = true;
            }
        }

        candidate.y = foundFloor ? floorY : 0f;

        return candidate;
    }

    void AttachToPlayerHand(Transform player)
    {
        var anchor = player.Find(PlayerVisual.RightHandAnchorName);
        if (anchor != null)
        {
            // PlayerVisual이 만든 캐릭터: 오른손 앵커 기준으로 미세 조정한다.
            transform.SetParent(anchor, false);
            transform.localPosition = new Vector3(0.03f, 0.02f, 0.02f);
            transform.localRotation = Quaternion.Euler(0f, 0f, -14f);
            transform.localScale = Vector3.one * 1.25f;
        }
        else
        {
            // 현재 씬의 character-b 프리팹은 PlayerVisual 앵커가 없다.
            // 그래서 Player 루트(발/pivot 기준)에 직접 붙이되, y를 손 높이까지 크게 올린다.
            transform.SetParent(player, false);
            transform.localPosition = new Vector3(0.58f, 1.18f, 0.60f);
            transform.localRotation = Quaternion.Euler(0f, 0f, -14f);
            transform.localScale = Vector3.one * 1.50f;
        }

        SetInteractableVisible(false);
    }

    void SetInteractableVisible(bool interactable)
    {
        gameObject.SetActive(true);

        foreach (var r in _renderers)
            if (r != null) r.enabled = true;

        foreach (var c in _colliders)
            if (c != null) c.enabled = interactable;

        foreach (var bubble in _bubbles)
            if (bubble != null) bubble.enabled = interactable;

        var generatedBubble = transform.Find("InteractionBubble");
        if (generatedBubble != null)
            generatedBubble.gameObject.SetActive(interactable);

        enabled = interactable;
    }
}
