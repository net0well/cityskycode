using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IpcBuild
{
    public static void BuildPlayer()
    {
        string backend = GetArg("-backend") ?? "IL2CPP";
        try
        {
            string outDir = GetArg("-out") ?? Path.Combine(Directory.GetCurrentDirectory(), "s1b-out");
            string buildDir = Path.Combine(outDir, "player-" + backend);
            Directory.CreateDirectory(buildDir);
            Debug.Log($"S1B_BUILD_START backend={backend}");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "Assets/Empty.unity");

            var named = NamedBuildTarget.Standalone;
            PlayerSettings.SetScriptingBackend(named,
                backend == "IL2CPP" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            PlayerSettings.SetIl2CppCompilerConfiguration(named, Il2CppCompilerConfiguration.Release);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Empty.unity" },
                locationPathName = Path.Combine(buildDir, "S1bPlayer.exe"),
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"S1B_BUILD backend={backend} result={report.summary.result}");
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 2);
        }
        catch (Exception ex)
        {
            Debug.LogError($"S1B_BUILD_EXCEPTION {ex}");
            EditorApplication.Exit(3);
        }
    }

    private static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
