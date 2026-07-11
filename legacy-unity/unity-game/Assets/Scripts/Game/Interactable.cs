using UnityEngine;

/// <summary>
/// 플레이어가 E키로 상호작용할 수 있는 오브젝트의 기반 클래스.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public float interactRadius = 1.8f;

    /// <summary>화면에 표시할 상호작용 힌트 텍스트</summary>
    public abstract string GetPrompt();

    /// <summary>플레이어가 E키를 눌렀을 때 호출</summary>
    public abstract void Interact(PlayerHand hand);

    public bool IsInRange(Vector3 playerPos)
        => Vector3.Distance(playerPos, transform.position) <= interactRadius;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
