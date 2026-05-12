using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 이동 + 상호작용 컨트롤러.
/// 새 Input System 사용: WASD / 방향키 이동, E 또는 Space로 상호작용.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 4f;

    [Header("상호작용 탐색 범위")]
    public float interactSearchRadius = 2.5f;

    Rigidbody    _rb;
    PlayerHand   _hand;
    Interactable _nearestInteractable;
    Vector3      _moveDir;

    public System.Action<string> OnPromptChanged;
    public System.Action<string> OnItemChanged;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody>();
        _hand = GetComponent<PlayerHand>();
        if (_rb != null)
        {
            _rb.constraints  = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        EnsureWateringCanInteractable();
        EnsureFarmTimeSigns();
    }

    void EnsureWateringCanInteractable()
    {
        var wateringCan = GameObject.Find("WateringCan")
                       ?? GameObject.Find("WateringJar");
        if (wateringCan == null) return;

        if (wateringCan.GetComponent<WateringCanStation>() == null)
            wateringCan.AddComponent<WateringCanStation>();
        if (wateringCan.GetComponent<InteractionBubble>() == null)
            wateringCan.AddComponent<InteractionBubble>();
    }

    void EnsureFarmTimeSigns()
    {
        foreach (var farm in FindObjectsByType<FarmStation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (farm.GetComponent<FarmTimeSign>() == null)
                farm.gameObject.AddComponent<FarmTimeSign>();
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // ── 방향 입력 ─────────────────────────────────────
        float h = 0f, v = 0f;
        if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) h = -1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) h =  1f;
        if (kb.downArrowKey.isPressed  || kb.sKey.isPressed) v = -1f;
        if (kb.upArrowKey.isPressed    || kb.wKey.isPressed) v =  1f;
        _moveDir = new Vector3(h, 0f, v).normalized;

        // ── E / Space → 상호작용 ──────────────────────────
        if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
        {
            if (ShouldDropWateringCan())
            {
                WateringCanStation.TryDropHeld(_hand, transform);
                OnItemChanged?.Invoke(_hand.DisplayName());
                return;
            }

            _nearestInteractable?.Interact(_hand);
            OnItemChanged?.Invoke(_hand.DisplayName());
        }
    }

    void FixedUpdate()
    {
        if (_moveDir.sqrMagnitude > 0.01f)
        {
            if (_rb != null)
                _rb.MovePosition(_rb.position + _moveDir * moveSpeed * Time.fixedDeltaTime);
            else
                transform.position += _moveDir * moveSpeed * Time.fixedDeltaTime;

            var targetRot = Quaternion.LookRotation(_moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.fixedDeltaTime);
        }

        RefreshNearestInteractable();
    }

    void RefreshNearestInteractable()
    {
        Interactable nearest  = null;
        float        best2    = interactSearchRadius * interactSearchRadius;

        foreach (var inter in FindObjectsByType<Interactable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            float d2 = (inter.transform.position - transform.position).sqrMagnitude;
            if (d2 < best2) { nearest = inter; best2 = d2; }
        }

        if (nearest != _nearestInteractable)
        {
            _nearestInteractable = nearest;
            OnPromptChanged?.Invoke(nearest != null ? nearest.GetPrompt() : "");
        }
        else if (nearest != null)
        {
            OnPromptChanged?.Invoke(nearest.GetPrompt());
        }
    }

    bool ShouldDropWateringCan()
    {
        if (_hand == null || !_hand.Has(IngredientType.WateringCan))
            return false;

        // 물이 필요한 재배기 앞에서는 같은 키가 "내려놓기"가 아니라 "물 주기"로 동작해야 한다.
        if (_nearestInteractable is FarmStation { Stage: FarmStation.FarmStage.NeedsWater })
            return false;

        return true;
    }
}
