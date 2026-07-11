using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// MalgunGothic.ttf 로 Dynamic TMP 폰트 에셋을 생성하고
/// 씬 내 모든 TMP_Text 컴포넌트에 할당합니다.
/// Menu: Tools → Setup Korean Font
/// </summary>
public static class KoreanFontSetup
{
    const string TTF_PATH    = "Assets/Fonts/MalgunGothic.ttf";
    const string ASSET_PATH  = "Assets/Fonts/MalgunGothic SDF.asset";

    [MenuItem("Tools/Setup Korean Font")]
    public static void Setup()
    {
        // 1. TTF 임포트 확인
        var font = AssetDatabase.LoadAssetAtPath<Font>(TTF_PATH);
        if (font == null)
        {
            Debug.LogError($"[KoreanFontSetup] Font not found: {TTF_PATH}");
            return;
        }

        // 2. 기존 에셋 삭제 후 새로 생성 (atlas texture 누락 방지)
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ASSET_PATH) != null)
        {
            AssetDatabase.DeleteAsset(ASSET_PATH);
            AssetDatabase.Refresh();
            Debug.Log($"[KoreanFontSetup] Deleted old asset: {ASSET_PATH}");
        }

        // Dynamic 모드: 사용되는 글리프를 런타임에 자동 생성
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic
        );

        if (fontAsset == null)
        {
            Debug.LogError("[KoreanFontSetup] Failed to create TMP_FontAsset.");
            return;
        }

        fontAsset.name = "MalgunGothic SDF";

        // 메인 에셋 저장
        AssetDatabase.CreateAsset(fontAsset, ASSET_PATH);

        // TMP material도 font asset의 sub-asset으로 저장해야 Player/build에서 참조가 유지된다.
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "MalgunGothic SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        // atlas texture를 서브 에셋으로 추가 (없으면 런타임에 텍스트 안 보임)
        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = "MalgunGothic SDF Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        // 추가 atlas textures도 서브 에셋으로 추가
        if (fontAsset.atlasTextures != null)
        {
            for (int i = 1; i < fontAsset.atlasTextures.Length; i++)
            {
                if (fontAsset.atlasTextures[i] != null)
                {
                    fontAsset.atlasTextures[i].name = $"MalgunGothic SDF Atlas {i}";
                    AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[i], fontAsset);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[KoreanFontSetup] Created: {ASSET_PATH}");

        // 3. 씬 내 모든 TMP_Text 에 폰트 할당
        var allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (var t in allTexts)
        {
            t.font = fontAsset;
            EditorUtility.SetDirty(t);
            count++;
        }

        // 4. TMP Settings 기본 폰트 교체 (LiberationSans → MalgunGothic)
        var tmpSettings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(
            "Assets/TextMesh Pro/Resources/TMP Settings.asset");
        if (tmpSettings != null)
        {
            var so = new SerializedObject(tmpSettings);
            var prop = so.FindProperty("m_defaultFontAsset");
            if (prop != null)
            {
                prop.objectReferenceValue = fontAsset;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(tmpSettings);
                AssetDatabase.SaveAssets();
                Debug.Log("[KoreanFontSetup] TMP Settings 기본 폰트 → MalgunGothic SDF");
            }
        }

        // 5. 씬 저장
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[KoreanFontSetup] 한국어 폰트 적용 완료: {count}개 TMP_Text 오브젝트");
    }
}
