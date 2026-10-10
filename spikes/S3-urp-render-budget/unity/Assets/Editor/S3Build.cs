using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Build do player do spike S3: cria um URP asset (Forward+, GRD, preset Baixo — render scale 0.67,
// 1 cascata, sombra curta), monta a cena mínima (câmera + PerfRunner que spawna tudo em runtime) e
// builda IL2CPP. Propriedades incertas entre versões são setadas por SerializedObject, com log do que pegou.
public static class S3Build
{
    public static void BuildPlayer()
    {
        string backend = GetArg("-backend") ?? "IL2CPP";
        try
        {
            string outDir = GetArg("-out") ?? Path.Combine(Directory.GetCurrentDirectory(), "s3-out");
            string buildDir = Path.Combine(outDir, "player-" + backend);
            Directory.CreateDirectory(buildDir);
            Debug.Log($"S3_BUILD_START backend={backend}");

            ConfigureUrp();
            EnsureShaderIncluded("Universal Render Pipeline/Lit");
            EnsureShaderIncluded("Universal Render Pipeline/Unlit");
            BuildScene();

            var named = NamedBuildTarget.Standalone;
            PlayerSettings.SetScriptingBackend(named,
                backend == "IL2CPP" ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            PlayerSettings.SetIl2CppCompilerConfiguration(named, Il2CppCompilerConfiguration.Release);
            PlayerSettings.runInBackground = true;
            PlayerSettings.enableFrameTimingStats = true; // sem isto o FrameTimingManager fica vazio no player
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/S3Scene.unity" },
                locationPathName = Path.Combine(buildDir, "S3Player.exe"),
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"S3_BUILD backend={backend} result={report.summary.result}");
            EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 2);
        }
        catch (Exception ex)
        {
            Debug.LogError($"S3_BUILD_EXCEPTION {ex}");
            EditorApplication.Exit(3);
        }
    }

    private static void ConfigureUrp()
    {
        Directory.CreateDirectory("Assets/URP");
        AssetDatabase.Refresh();

        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(rendererData, "Assets/URP/UrpRenderer.asset");
        TrySet(new SerializedObject(rendererData), "m_RenderingMode", 2, "Forward+"); // Forward=0, Deferred=1, Forward+=2

        UniversalRenderPipelineAsset urp = UniversalRenderPipelineAsset.Create(rendererData);
        AssetDatabase.CreateAsset(urp, "Assets/URP/UrpAsset.asset");

        var so = new SerializedObject(urp);
        TrySetFloat(so, "m_RenderScale", 0.67f, "render scale (STP alvo)");
        TrySet(so, "m_ShadowDistance", 120, "distância de sombra");
        TrySetFloat(so, "m_ShadowDistance", 120f, "distância de sombra (float)");
        TrySet(so, "m_ShadowCascadeCount", 1, "cascatas de sombra");
        TrySet(so, "m_MainLightShadowmapResolution", 1024, "resolução do shadowmap");
        TrySet(so, "m_MSAA", 1, "MSAA off"); // 1 = Disabled
        TrySet(so, "m_GPUResidentDrawerMode", 1, "GPU Resident Drawer (Instanced)"); // 0=Disabled,1=InstancedDrawing
        so.ApplyModifiedPropertiesWithoutUndo();

        GraphicsSettings.defaultRenderPipeline = urp;
        QualitySettings.renderPipeline = urp;
        AssetDatabase.SaveAssets();
        Debug.Log("S3_URP configured: " + urp.name);
    }

    // Sem isto, o URP/Lit é removido no stripping (nada na cena o referencia em build time) e
    // Shader.Find devolve null no player. Garante a inclusão via "Always Included Shaders".
    private static void EnsureShaderIncluded(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.Log($"S3_SHADER not found in editor: {shaderName}");
            return;
        }

        UnityEngine.Object settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
        var so = new SerializedObject(settings);
        SerializedProperty arr = so.FindProperty("m_AlwaysIncludedShaders");
        for (int i = 0; i < arr.arraySize; i++)
        {
            if (arr.GetArrayElementAtIndex(i).objectReferenceValue == shader)
            {
                return; // já incluído
            }
        }

        arr.InsertArrayElementAtIndex(arr.arraySize);
        arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = shader;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"S3_SHADER included: {shaderName}");
    }

    private static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        camGo.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.65f, 0.85f);
        camGo.AddComponent<PerfRunner>();

        EditorSceneManager.SaveScene(scene, "Assets/S3Scene.unity");
    }

    private static void TrySet(SerializedObject so, string prop, int value, string label)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null && (p.propertyType == SerializedPropertyType.Integer || p.propertyType == SerializedPropertyType.Enum))
        {
            p.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"S3_URP set {label} ({prop}={value})");
        }
        else
        {
            Debug.Log($"S3_URP skip {label}: propriedade {prop} ausente/incompatível");
        }
    }

    private static void TrySetFloat(SerializedObject so, string prop, float value, string label)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null && p.propertyType == SerializedPropertyType.Float)
        {
            p.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"S3_URP set {label} ({prop}={value})");
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
