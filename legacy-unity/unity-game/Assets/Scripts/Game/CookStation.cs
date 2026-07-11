using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 조리대 (버거 조립 스테이션).
/// 웹앱(100-burger)의 Kitchen.jsx 3단계 조립 포팅:
///   Stage 0 (Idle)     → 손에 든 재료를 하나씩 올림
///   Stage 1 (Preparing) → 5초 자동 진행 (재료 준비 중)
///   Stage 2 (Assembling)→ 8초 자동 진행 (버거 조립 중)
///   Stage 3 (AddSauce)  → [E] 소스 마무리
///   Stage 4 (Done)      → [E] 완성 버거 손에 들기
///
/// 웹앱과 다른 점:
///   - 웹앱: 빠른 탭 / 재료 순서 선택 / 꾹 누르기
///   - Unity MVP: 손으로 재료 전달 + 자동 진행 + 마지막 [E]
/// </summary>
public class CookStation : Interactable
{
    public enum CookStage { Idle, Preparing, Assembling, AddSauce, Done }

    [Header("타이머 (초)")]
    public float prepareTime  = 5f;
    public float assembleTime = 8f;

    /// <summary>버거 완성에 필요한 전체 재료 목록</summary>
    static readonly IngredientType[] DefaultIngredients =
    {
        IngredientType.Bread,
        IngredientType.GrilledPatty,
        IngredientType.GrilledBacon,
        IngredientType.Lettuce,
        IngredientType.Tomato,
        IngredientType.Sauce,
    };

    CookStage _stage = CookStage.Idle;
    float     _timer;
    readonly HashSet<IngredientType> _loadedIngredients = new();

    void Awake()
    {
        prepareTime = Mathf.Min(prepareTime, 2.5f);
        assembleTime = Mathf.Min(assembleTime, 4f);
    }

    void Update()
    {
        // Preparing: prepareTime 경과 → Assembling으로 전환
        if (_stage == CookStage.Preparing)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _stage = CookStage.Assembling;
                _timer = assembleTime;
            }
        }
        // Assembling: assembleTime 경과 → AddSauce 단계로 전환
        else if (_stage == CookStage.Assembling)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _stage = CookStage.AddSauce;
        }
    }

    public override string GetPrompt()
    {
        var order = DailyBurgerRunManager.I.CurrentOrder;
        if (_stage == CookStage.Idle && order != null && order.status == FixedOrderStatus.Succeeded)
            return "오늘 주문 완료!";
        if (_stage == CookStage.Idle && order != null && order.status == FixedOrderStatus.ReadyToServe)
            return "완성 버거를 서빙대로 전달하세요.";

        return _stage switch
        {
            CookStage.Idle
                => $"조리대 [{LoadedCount}/{RequiredCount}]\n{BuildMissingPrompt()}",
            CookStage.Preparing
                => $"재료 준비 중... ({_timer:F0}초)",
            CookStage.Assembling
                => $"버거 조립 중... ({_timer:F0}초)",
            CookStage.AddSauce
                => "마지막 단계!\n[E] 포장 마무리",
            CookStage.Done
                => "버거 완성!\n[E] 들기",
            _ => ""
        };
    }

    public override void Interact(PlayerHand hand)
    {
        switch (_stage)
        {
            case CookStage.Idle:
                var order = DailyBurgerRunManager.I.CurrentOrder;
                if (order != null && order.status is FixedOrderStatus.ReadyToServe or FixedOrderStatus.Succeeded)
                {
                    GameManager.I?.PostFeedback("현재 주문부터 전달해 주세요.", false);
                    return;
                }

                if (hand == null || hand.IsEmpty)
                {
                    GameManager.I?.PostFeedback("주문 재료를 들고 조리대를 사용하세요.", false);
                    return;
                }

                IngredientType item = hand.heldItem;
                if (!IsRequired(item) || _loadedIngredients.Contains(item))
                {
                    GameManager.I?.PostFeedback("이 재료는 지금 필요하지 않아요.", false);
                    return;
                }

                _loadedIngredients.Add(item);
                hand.Drop();
                GameManager.I?.PostFeedback($"조리대에 {IngredientNames.Korean(item)} 추가", true);

                if (HasAllIngredients())
                {
                    _stage = CookStage.Preparing;
                    _timer = prepareTime;
                    DailyBurgerRunManager.I.SetOrderStatus(FixedOrderStatus.Cooking);
                    Debug.Log("[Cook] Start");
                }
                break;

            case CookStage.AddSauce:
                // 소스 뿌리기 완료 → 완성 대기
                _stage = CookStage.Done;
                DailyBurgerRunManager.I.SetOrderStatus(FixedOrderStatus.ReadyToServe);
                GameManager.I?.PostFeedback("버거 완성! 손에 들고 서빙대로 이동하세요.", true);
                Debug.Log("[Cook] Ready");
                break;

            case CookStage.Done:
                // 손에 들기
                if (!hand.IsEmpty)
                {
                    Debug.Log("[Cook] 손이 찼어요");
                    return;
                }
                hand.TryPickUp(IngredientType.AssembledBurger);
                _stage = CookStage.Idle;
                _loadedIngredients.Clear();
                Debug.Log("[Cook] + Burger");
                break;

            default:
                Debug.Log("[Cook] Busy");
                break;
        }
    }

    // ── HUD 연동 ─────────────────────────────────────────────────

    /// <summary>조리 진행률 0~1 (HUD 진행 바용)</summary>
    public float GetProgress() => _stage switch
    {
        CookStage.Preparing  => 1f - _timer / prepareTime,
        CookStage.Assembling => 1f - _timer / assembleTime,
        CookStage.AddSauce   => 0.9f,
        CookStage.Done       => 1f,
        _                    => 0f
    };

    public bool IsCooking => _stage is CookStage.Preparing or CookStage.Assembling;
    public bool IsDone    => _stage == CookStage.Done;
    public CookStage Stage => _stage;
    public int LoadedCount => _loadedIngredients.Count;
    public int RequiredCount => RequiredIngredients.Count;

    // ── 내부 헬퍼 ───────────────────────────────────────────────

    bool HasAllIngredients()
    {
        foreach (var t in RequiredIngredients)
            if (!_loadedIngredients.Contains(t)) return false;
        return true;
    }

    string BuildMissingPrompt()
    {
        string missing = BuildMissingList();
        return $"필요 재료: {missing}\n[E] 재료 놓기";
    }

    string BuildMissingList()
    {
        var list = new List<string>();
        foreach (var t in RequiredIngredients)
            if (!_loadedIngredients.Contains(t))
                list.Add(IngredientNames.Korean(t));
        return string.Join(", ", list);
    }

    IReadOnlyList<IngredientType> RequiredIngredients
    {
        get
        {
            var order = DailyBurgerRunManager.I.CurrentOrder;
            return order != null && order.requiredIngredients != null
                ? order.requiredIngredients
                : DefaultIngredients;
        }
    }

    bool IsRequired(IngredientType type)
    {
        foreach (var required in RequiredIngredients)
            if (required == type)
                return true;
        return false;
    }
}
