using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CitySky.Spike.Bench.Editor
{
    /// <summary>Métodos chamados por -executeMethod no modo batch.</summary>
    public static class BenchBuild
    {
        private const int Citizens = 100_000;
        private const ulong Seed = 42;

        /// <summary>Roda o benchmark no Editor (Mono) e escreve o resultado.</summary>
        public static void RunInEditor()
        {
            string outDir = GetOut();
            Directory.CreateDirectory(outDir);
            string line = $"runtime=Editor-Mono unity={Application.unityVersion} {BenchCore.Run(Citizens, Seed)}";
            File.WriteAllText(Path.Combine(outDir, "s4b-editor-Mono.txt"), line);
            Debug.Log("S4B_RESULT " + line);
            EditorApplication.Exit(0);
        }

        /// <summary>Compila um player com o backend pedido (-buildTarget e um define próprio).</summary>
        public static void BuildPlayer()
        {
            string backend = GetArg("-s4bBackend") ?? "Mono";
            try
            {
                string outDir = GetOut();
                string buildDir = Path.Combine(outDir, "player-" + backend);
                Directory.CreateDirectory(buildDir);
                Debug.Log($"S4B_BUILD_START backend={backend} buildDir={buildDir}");

                // Cena vazia (o benchmark roda por RuntimeInitializeOnLoadMethod).
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                string scenePath = "Assets/Empty.unity";
                bool saved = EditorSceneManager.SaveScene(scene, scenePath);
                Debug.Log($"S4B_BUILD_SCENE saved={saved}");

                var named = NamedBuildTarget.Standalone;
                PlayerSettings.SetScriptingBackend(
                    named,
                    backend == "IL2CPP" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
                PlayerSettings.SetIl2CppCompilerConfiguration(named, Il2CppCompilerConfiguration.Release);

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { scenePath },
                    locationPathName = Path.Combine(buildDir, "S4bPlayer.exe"),
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None,
                };

                var report = BuildPipeline.BuildPlayer(options);
                Debug.Log($"S4B_BUILD backend={backend} result={report.summary.result} totalSize={report.summary.totalSize}");
                EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 2);
            }
            catch (Exception ex)
            {
                Debug.LogError($"S4B_BUILD_EXCEPTION backend={backend} {ex}");
                EditorApplication.Exit(3);
            }
        }

        private static string GetOut() => GetArg("-s4bOut") ?? Path.Combine(Directory.GetCurrentDirectory(), "s4b-out");

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
}
