using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 조리대 (버거 조립 스테이션).
/// 웹앱(100-burger)의 Kitchen.jsx 3단계 조립 포팅:
///   Stage 0 (Idle)     → 재료 확인 후 [E] 조립 시작
///   Stage 1 (Preparing) → 5초 자동 진행 (재료 준비 중)
///   Stage 2 (Assembling)→ 8초 자동 진행 (버거 조립 중)
///   Stage 3 (AddSauce)  → [E] 소스 마무리
///   Stage 4 (Done)      → [E] 완성 버거 손에 들기
///
/// 웹앱과 다른 점:
///   - 웹앱: 빠른 탭 / 재료 순서 선택 / 꾹 누르기
///   - Unity MVP: 자동 진행 + 마지막 [E] 한 번으로 단순화
///   - 재료는 모두 InventoryManager에서 차감
/// </summary>
public class CookStation : Interactable
{
    public enum CookStage { Idle, Preparing, Assembling, AddSauce, Done }

    [Header("타이머 (초)")]
    public float prepareTime  = 5f;
    public float assembleTime = 8f;

    /// <summary>버거 완성에 필요한 전체 재료 목록</summary>
    static readonly IngredientType[] RequiredIngredients =
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
        return _stage switch
        {
            CookStage.Idle when HasAllIngredients()
                => "조리대\n[E] 버거 조립 시작!",
            CookStage.Idle
                => BuildMissingPrompt(),
            CookStage.Preparing
                => $"재료 준비 중... ({_timer:F0}초)",
            CookStage.Assembling
                => $"버거 조립 중... ({_timer:F0}초)",
            CookStage.AddSauce
                => "마지막 단계!\n[E] 소스 뿌리기",
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
                if (!HasAllIngredients())
                {
                    Debug.Log($"[Cook] 재료: {BuildMissingList()}");
                    return;
                }
                // 필요 재료 전부 소비
                foreach (var t in RequiredIngredients)
                    InventoryManager.I?.TryUse(t);
                _stage = CookStage.Preparing;
                _timer = prepareTime;
                Debug.Log("[Cook] Start");
                break;

            case CookStage.AddSauce:
                // 소스 뿌리기 완료 → 완성 대기
                _stage = CookStage.Done;
                Debug.Log("[Cook] Ready");
                break;

            case CookStage.Done:
                // 손에 들기
                if (!hand.IsEmpty)
                {
                    Debug.Log("[Cook] 손이 찼어요");
                    return;
                }
                hand.PickUp(IngredientType.AssembledBurger);
                _stage = CookStage.Idle;
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

    // ── 내부 헬퍼 ───────────────────────────────────────────────

    bool HasAllIngredients()
    {
        var inv = InventoryManager.I;
        if (inv == null) return false;
        foreach (var t in RequiredIngredients)
            if (!inv.Has(t)) return false;
        return true;
    }

    string BuildMissingPrompt()
    {
        string missing = BuildMissingList();
        return $"재료 부족:\n{missing}";
    }

    string BuildMissingList()
    {
        var inv = InventoryManager.I;
        var list = new List<string>();
        foreach (var t in RequiredIngredients)
            if (inv == null || !inv.Has(t))
                list.Add(IngredientNames.Korean(t));
        return string.Join(", ", list);
    }
}
