using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MvpSceneSetup
{
    [MenuItem("Tools/100 Burger Family/Stop Play Mode")]
    public static void StopPlayMode()
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;
        Debug.Log("[MVP Setup] Edit Mode ready");
    }

    [MenuItem("Tools/100 Burger Family/Apply MVP Scene")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[MVP Setup] Play Mode를 먼저 종료하세요.");
            return;
        }

        KoreanFontSetup.Setup();
        AssetPackUISetup.Apply();

        foreach (var cook in Object.FindObjectsByType<CookStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            cook.prepareTime = 2.5f;
            cook.assembleTime = 4f;
            EditorUtility.SetDirty(cook);
        }

        foreach (var grill in Object.FindObjectsByType<GrillStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            grill.grillTime = 6f;
            grill.collectWindow = 6f;
            EditorUtility.SetDirty(grill);
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[MVP Setup] Scene, HUD, font, gameplay timing applied");
    }
}
