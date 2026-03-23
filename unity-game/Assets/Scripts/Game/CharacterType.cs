using UnityEngine;

/// <summary>
/// 캐릭터 종류 정의.
/// 부모: 아빠(Dad) / 엄마(Mom)
/// 자녀: 아들(Son) / 딸(Daughter)
/// </summary>
public enum CharacterType { None, Dad, Mom, Son, Daughter }

/// <summary>
/// 각 캐릭터 타입의 시각 데이터 (색상, 크기 등)
/// </summary>
public static class CharacterData
{
    // ── 이름 ──────────────────────────────────────────
    public static string GetDisplayName(CharacterType t) => t switch
    {
        CharacterType.Dad      => "아빠",
        CharacterType.Mom      => "엄마",
        CharacterType.Son      => "아들",
        CharacterType.Daughter => "딸",
        _                      => "플레이어"
    };

    // ── 역할 ──────────────────────────────────────────
    public static PlayerRole GetRole(CharacterType t) => t switch
    {
        CharacterType.Dad      => PlayerRole.Parent,
        CharacterType.Mom      => PlayerRole.Parent,
        CharacterType.Son      => PlayerRole.Child,
        CharacterType.Daughter => PlayerRole.Child,
        _                      => PlayerRole.None
    };

    // ── 몸통 색 ───────────────────────────────────────
    public static Color GetBodyColor(CharacterType t) => t switch
    {
        CharacterType.Dad      => new Color(0.22f, 0.40f, 0.78f), // 파란 셔츠
        CharacterType.Mom      => new Color(0.95f, 0.78f, 0.22f), // 노란 앞치마
        CharacterType.Son      => new Color(0.22f, 0.65f, 0.28f), // 초록 티셔츠
        CharacterType.Daughter => new Color(0.92f, 0.35f, 0.55f), // 분홍 원피스
        _                      => new Color(0.85f, 0.85f, 0.85f)
    };

    // ── 피부 색 (공통) ────────────────────────────────
    public static Color GetSkinColor(CharacterType t)
        => new Color(0.96f, 0.82f, 0.67f);

    // ── 바지/하의 색 ──────────────────────────────────
    public static Color GetBottomColor(CharacterType t) => t switch
    {
        CharacterType.Dad      => new Color(0.22f, 0.22f, 0.45f), // 진청 바지
        CharacterType.Mom      => new Color(0.78f, 0.55f, 0.35f), // 갈색 치마
        CharacterType.Son      => new Color(0.18f, 0.35f, 0.65f), // 청바지
        CharacterType.Daughter => new Color(0.88f, 0.25f, 0.45f), // 진분홍 스커트
        _                      => new Color(0.4f, 0.4f, 0.4f)
    };

    // ── 악세서리 색 (엄마:핑크 핀, 아들:노란 모자, 딸:핑크 리본) ──
    public static Color GetAccColor(CharacterType t) => t switch
    {
        CharacterType.Mom      => new Color(0.95f, 0.35f, 0.55f), // 핑크 헤어핀
        CharacterType.Son      => new Color(0.98f, 0.85f, 0.15f), // 노란 모자
        CharacterType.Daughter => new Color(0.92f, 0.20f, 0.45f), // 빨간 리본
        _                      => Color.white
    };

    // ── 전체 캐릭터 스케일 (부모 > 자녀) ─────────────
    public static float GetBodyScale(CharacterType t) => t switch
    {
        CharacterType.Dad      => 0.42f,
        CharacterType.Mom      => 0.38f,
        CharacterType.Son      => 0.30f,
        CharacterType.Daughter => 0.28f,
        _                      => 0.35f
    };

    // ── 초기 스폰 위치 ────────────────────────────────
    public static Vector3 GetSpawnPosition(CharacterType t)
    {
        float s = GetBodyScale(t);
        return new Vector3(0f, s, -2.5f); // y=scale → 바닥에 발이 닿음
    }
}
