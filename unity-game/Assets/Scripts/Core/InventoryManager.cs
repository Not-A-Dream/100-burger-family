using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 재료 인벤토리 싱글톤.
/// 웹앱(100-burger)의 Firestore inventory/current 문서를 Unity 로컬로 포팅.
///
/// 왜 PlayerHand와 분리했나?
///   PlayerHand = 손에 든 1개 슬롯 (물리적으로 들고 다니는 것)
///   InventoryManager = 선반/냉장고에 쌓인 재료 수량 (전체 재고)
///   GrillStation은 "인벤토리에서 꺼내서 굽고, 결과를 인벤토리에 넣는" 방식.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager I
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[InventoryManager]");
            _instance = go.AddComponent<InventoryManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    private static InventoryManager _instance;

    [Header("게임 시작 시 초기 재고")]
    public int startBread    = 5;
    public int startRawPatty = 5;
    public int startRawBacon = 5;
    public int startSauce    = 5;

    private Dictionary<IngredientType, int> _counts = new();

    /// <summary>재고 변경 시 발생. UI에서 구독해 갱신.</summary>
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitInventory();
    }

    void InitInventory()
    {
        _counts.Clear();
        // 모든 타입 0으로 초기화
        foreach (IngredientType t in Enum.GetValues(typeof(IngredientType)))
            _counts[t] = 0;

        // 초기 지급: 빵/생패티/생베이컨/소스
        // 왜 초기 지급? 웹앱의 "발주대 24시간 배송"을 Unity 로컬에선
        // 시작 재고로 단순화. 재고가 0이 되면 ServeCounter 옆 SupplyStation에서 보충.
        _counts[IngredientType.Bread]    = startBread;
        _counts[IngredientType.RawPatty] = startRawPatty;
        _counts[IngredientType.RawBacon] = startRawBacon;
        _counts[IngredientType.Sauce]    = startSauce;
    }

    // ── 조회 ─────────────────────────────────────────────────────

    public int Count(IngredientType type)
        => _counts.TryGetValue(type, out int v) ? v : 0;

    public bool Has(IngredientType type, int amount = 1)
        => Count(type) >= amount;

    // ── 사용/추가 ─────────────────────────────────────────────────

    /// <summary>재료 사용. 수량 부족 시 false 반환.</summary>
    public bool TryUse(IngredientType type, int amount = 1)
    {
        if (!Has(type, amount)) return false;
        _counts[type] -= amount;
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>재료 추가 (수확/완성 시 호출).</summary>
    public void Add(IngredientType type, int amount = 1)
    {
        _counts[type] = Count(type) + amount;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>게임 재시작 시 인벤토리 초기화.</summary>
    public void Reset()
    {
        InitInventory();
        OnInventoryChanged?.Invoke();
    }
}
