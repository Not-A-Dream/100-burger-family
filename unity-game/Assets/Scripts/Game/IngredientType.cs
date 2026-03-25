/// <summary>
/// 게임 내 모든 재료/아이템 타입 정의.
/// 웹앱(100-burger)의 inventory 데이터 구조를 Unity enum으로 포팅.
/// </summary>
public enum IngredientType
{
    None,

    // ── 농장 수확 ─────────────────────────────────
    Tomato,       // 토마토 (FarmStation)
    Lettuce,      // 양상추 (FarmStation)

    // ── 초기 지급 / 발주 재료 ─────────────────────
    Bread,        // 빵 (InventoryManager 초기 지급)
    RawPatty,     // 생 패티
    RawBacon,     // 생 베이컨
    Sauce,        // 소스

    // ── 불판 완성 재료 ────────────────────────────
    GrilledPatty, // 구운 패티 (GrillStation)
    GrilledBacon, // 구운 베이컨 (GrillStation)

    // ── 최종 완성품 ───────────────────────────────
    AssembledBurger, // 완성 버거 (CookStation)
}

/// <summary>IngredientType → 한국어 이름 변환</summary>
public static class IngredientNames
{
    public static string Korean(IngredientType type) => type switch
    {
        IngredientType.Tomato          => "토마토",
        IngredientType.Lettuce         => "양상추",
        IngredientType.Bread           => "빵",
        IngredientType.RawPatty        => "생 패티",
        IngredientType.RawBacon        => "생 베이컨",
        IngredientType.Sauce           => "소스",
        IngredientType.GrilledPatty    => "구운 패티",
        IngredientType.GrilledBacon    => "구운 베이컨",
        IngredientType.AssembledBurger => "완성 버거",
        _                              => "없음"
    };
}
