using System;
using System.IO;
using CitySky.Demo;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CitySky.Build
{
    /// <summary>
    /// Gera o player Windows do jogo para o marco F1-112 (editor embutido). Cria uma cena-backdrop
    /// PROVISÓRIA (pipeline Built-in; a composição do painel-filho independe do pipeline — mesmo
    /// swapchain DXGI) e constrói um standalone 1280x800, janela, runInBackground.
    ///
    /// Uso (batch):
    ///   Unity.exe -batchmode -nographics -projectPath unity/CitySky \
    ///     -executeMethod CitySky.Build.CityPlayerBuild.BuildPlayer -backend Mono -out &lt;dir&gt; -logFile &lt;log&gt;
    /// </summary>
    public static class CityPlayerBuild
    {
        public static void BuildPlayer()
        {
            string backend = GetArg("-backend") ?? "Mono";
            try
            {
                string outDir = GetArg("-out") ?? Path.Combine(Directory.GetCurrentDirectory(), "Build", "player");
                Directory.CreateDirectory(outDir);
                Debug.Log($"CITY_BUILD_START backend={backend} out={outDir}");

                BuildScene();

                var named = NamedBuildTarget.Standalone;
                PlayerSettings.SetScriptingBackend(named,
                    backend == "IL2CPP" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
                PlayerSettings.SetIl2CppCompilerConfiguration(named, Il2CppCompilerConfiguration.Release);
                PlayerSettings.runInBackground = true;
                PlayerSettings.defaultIsNativeResolution = false;
                PlayerSettings.defaultScreenWidth = 1280;
                PlayerSettings.defaultScreenHeight = 800;
                PlayerSettings.resizableWindow = true;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Demo/CityDemo.unity" },
                    locationPathName = Path.Combine(outDir, "CitySkyPlayer.exe"),
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None,
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                Debug.Log($"CITY_BUILD backend={backend} result={report.summary.result}");
                EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 2);
            }
            catch (Exception ex)
            {
                Debug.LogError($"CITY_BUILD_EXCEPTION {ex}");
                EditorApplication.Exit(3);
            }
        }

        private static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.35f, 0.63f, 0.90f); // céu azul claro (contrasta com o editor escuro)
            camGo.transform.position = new Vector3(0f, 9f, -16f);
            camGo.transform.LookAt(Vector3.zero);
            camGo.AddComponent<DemoSceneDriver>();

            var lightGo = new GameObject("Sun");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            ground.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.30f, 0.65f, 0.32f)); // grama

            Color[] palette =
            {
                new Color(0.90f, 0.35f, 0.32f), new Color(0.96f, 0.76f, 0.30f), new Color(0.40f, 0.52f, 0.90f),
                new Color(0.80f, 0.45f, 0.85f), new Color(0.35f, 0.80f, 0.78f), new Color(0.95f, 0.55f, 0.30f),
            };
            var rng = new System.Random(42);
            for (int i = 0; i < 24; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Building" + i;
                float h = 1f + (float)rng.NextDouble() * 4f;
                cube.transform.localScale = new Vector3(1.4f, h, 1.4f);
                cube.transform.position = new Vector3((float)(rng.NextDouble() * 18 - 9), h / 2f, (float)(rng.NextDouble() * 18 - 9));
                cube.GetComponent<Renderer>().sharedMaterial = Mat(palette[i % palette.Length]);
                cube.AddComponent<DemoSpinner>().DegreesPerSecond = new Vector3(0f, 20f + i * 2f, 0f);
            }

            EditorSceneManager.SaveScene(scene, "Assets/Demo/CityDemo.unity");
        }

        private static Material Mat(Color color)
        {
            return new Material(Shader.Find("Standard")) { color = color };
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
}
