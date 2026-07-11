using System;
using TMPro;
using UnityEngine;

public static class ReadableTMPFont
{
    static TMP_FontAsset _cached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void PrepareDefaultFont()
    {
        _cached = null;
        EnsureMaterial(TMP_Settings.defaultFontAsset);
    }

    public static TMP_FontAsset Resolve()
    {
        if (_cached != null)
            return _cached;

        var sceneTexts = UnityEngine.Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var text in sceneTexts)
        {
            if (text == null || text.font == null)
                continue;

            if (IsUsable(text.font) && LooksLikeKoreanFont(text.font.name))
                return _cached = text.font;
        }

        if (IsUsable(TMP_Settings.defaultFontAsset))
        {
            if (LooksLikeKoreanFont(TMP_Settings.defaultFontAsset.name))
                return _cached = TMP_Settings.defaultFontAsset;

            _cached = TMP_Settings.defaultFontAsset;
        }

        foreach (var text in sceneTexts)
        {
            if (text != null && IsUsable(text.font))
                return _cached = text.font;
        }

        return _cached = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    static bool IsUsable(TMP_FontAsset font)
        => font != null && EnsureMaterial(font);

    static bool EnsureMaterial(TMP_FontAsset font)
    {
        if (font == null) return false;
        if (font.material != null) return true;
        if (font.atlasTexture == null) return false;

        var shader = Shader.Find("TextMeshPro/Distance Field");
        if (shader == null) return false;

        var material = new Material(shader)
        {
            name = $"{font.name} Runtime Material",
            mainTexture = font.atlasTexture,
        };
        material.SetTexture(ShaderUtilities.ID_MainTex, font.atlasTexture);
        material.SetFloat(ShaderUtilities.ID_TextureWidth, font.atlasWidth);
        material.SetFloat(ShaderUtilities.ID_TextureHeight, font.atlasHeight);
        font.material = material;
        return true;
    }

    static bool LooksLikeKoreanFont(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return name.IndexOf("Malgun", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("Korean", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("Noto", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("Nanum", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
