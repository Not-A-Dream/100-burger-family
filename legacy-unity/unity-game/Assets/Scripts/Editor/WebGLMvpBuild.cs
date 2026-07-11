using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WebGLMvpBuild
{
    const string OutputPath = "Builds/WebGL";

    [MenuItem("Tools/100 Burger Family/Build WebGL MVP")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Build] Play Mode를 종료한 뒤 다시 실행하세요.");
            return;
        }

        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            throw new System.Exception("WebGL build target 전환에 실패했습니다.");

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.runInBackground = true;

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new System.Exception("Build Settings에 활성 Scene이 없습니다.");

        Directory.CreateDirectory(OutputPath);
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception($"WebGL build 실패: {report.summary.result}");

        Debug.Log($"[Build] WebGL success: {OutputPath} ({report.summary.totalSize} bytes)");
    }
}
