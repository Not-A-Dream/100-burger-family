using System;
using TMPro;
using UnityEngine;

public static class ReadableTMPFont
{
    static TMP_FontAsset _cached;

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

            if (LooksLikeKoreanFont(text.font.name))
                return _cached = text.font;
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            if (LooksLikeKoreanFont(TMP_Settings.defaultFontAsset.name))
                return _cached = TMP_Settings.defaultFontAsset;

            _cached = TMP_Settings.defaultFontAsset;
        }

        foreach (var text in sceneTexts)
        {
            if (text != null && text.font != null)
                return _cached = text.font;
        }

        return _cached = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
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
