using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

// S3 — harness de orçamento de render. Spawna a cena (prédios/árvores/agentes low-poly), roteiriza a
// câmera em 3 zooms × dia/noite, e grava CSV por frame + resumo JSON via FrameTimingManager (funciona
// em build de player, sem o Editor conectado). Conta/densidade vêm por linha de comando.
public sealed class PerfRunner : MonoBehaviour
{
    private int _buildings = 5000;
    private int _trees = 20000;
    private int _agents = 1000;
    private int _framesPerSegment = 300;
    private float _warmupSeconds = 8f;
    private string _outDir = ".";

    private readonly float _areaBuild = 300f;   // meia-largura da malha de prédios (m)
    private readonly float _areaTrees = 520f;
    private Transform[] _agentT = Array.Empty<Transform>();
    private Vector3[] _agentVel = Array.Empty<Vector3>();
    private Light _sun = null!;
    private readonly List<Light> _streetlights = new();
    private Camera _cam = null!;

    private struct Sample { public double fps, gpuMs, cpuMainMs; }

    private void Awake()
    {
        ParseArgs();
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        _cam = Camera.main!;
        BuildScene();
    }

    private void Start() => StartCoroutine(RunAll());

    private IEnumerator RunAll()
    {
        var summary = new StringBuilder();
        var csv = new StringBuilder();
        csv.AppendLine("segment,frame,fps,gpuMs,cpuMainMs");

        summary.AppendLine("{");
        summary.AppendLine($"  \"gpu\": {Q(SystemInfo.graphicsDeviceName)},");
        summary.AppendLine($"  \"vramMB\": {SystemInfo.graphicsMemorySize},");
        summary.AppendLine($"  \"cpu\": {Q(SystemInfo.processorType)},");
        summary.AppendLine($"  \"systemMemoryMB\": {SystemInfo.systemMemorySize},");
        summary.AppendLine($"  \"screen\": \"{Screen.width}x{Screen.height}\",");
        summary.AppendLine($"  \"renderPipeline\": {Q(GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "none")},");
        summary.AppendLine($"  \"gpuResidentDrawer\": {Q(GrdStatus())},");
        summary.AppendLine($"  \"buildings\": {_buildings}, \"trees\": {_trees}, \"agents\": {_agents},");
        summary.AppendLine("  \"segments\": [");

        // Aquecimento.
        SetZoom("mid");
        SetPeriod(day: true);
        float t = 0f;
        while (t < _warmupSeconds) { t += Time.unscaledDeltaTime; yield return null; }

        string[] zooms = { "near", "mid", "far" };
        bool[] periods = { true, false }; // dia, noite
        bool first = true;
        foreach (string zoom in zooms)
        {
            foreach (bool isDay in periods)
            {
                SetZoom(zoom);
                SetPeriod(isDay);
                // pequeno settle para o Forward+ / sombras reagirem
                for (int i = 0; i < 20; i++) { yield return null; }

                var samples = new List<Sample>(_framesPerSegment);
                for (int f = 0; f < _framesPerSegment; f++)
                {
                    FrameTimingManager.CaptureFrameTimings();
                    var timings = new FrameTiming[1];
                    uint got = FrameTimingManager.GetLatestTimings(1, timings);
                    double gpu = got > 0 ? timings[0].gpuFrameTime : 0;
                    double cpuMain = got > 0 ? timings[0].cpuMainThreadFrameTime : 0;
                    double fps = Time.unscaledDeltaTime > 0 ? 1.0 / Time.unscaledDeltaTime : 0;
                    samples.Add(new Sample { fps = fps, gpuMs = gpu, cpuMainMs = cpuMain });
                    string seg = $"{zoom}-{(isDay ? "day" : "night")}";
                    csv.AppendLine($"{seg},{f},{fps.ToString("F2", CultureInfo.InvariantCulture)},{gpu.ToString("F2", CultureInfo.InvariantCulture)},{cpuMain.ToString("F2", CultureInfo.InvariantCulture)}");
                    AnimateAgents();
                    yield return null;
                }

                summary.AppendLine((first ? "    " : "    ,") + SegmentJson($"{zoom}-{(isDay ? "day" : "night")}", samples));
                first = false;
            }
        }

        summary.AppendLine("  ],");
        summary.AppendLine($"  \"reservedMemoryMB\": {Profiler.GetTotalReservedMemoryLong() / (1024 * 1024)},");
        summary.AppendLine($"  \"monoHeapMB\": {Profiler.GetMonoHeapSizeLong() / (1024 * 1024)}");
        summary.AppendLine("}");

        Directory.CreateDirectory(_outDir);
        File.WriteAllText(Path.Combine(_outDir, "s3-summary.json"), summary.ToString());
        File.WriteAllText(Path.Combine(_outDir, "s3-frames.csv"), csv.ToString());
        Debug.Log("S3_DONE\n" + summary);
        yield return null;
        Application.Quit(0);
    }

    private string SegmentJson(string name, List<Sample> s)
    {
        var fps = s.ConvertAll(x => x.fps); fps.Sort();
        var gpu = s.ConvertAll(x => x.gpuMs); gpu.Sort();
        var cpu = s.ConvertAll(x => x.cpuMainMs); cpu.Sort();
        double P(List<double> v, double p) => v.Count == 0 ? 0 : v[Mathf.Clamp((int)(p * v.Count), 0, v.Count - 1)];
        double mean(List<double> v) { double a = 0; foreach (var x in v) a += x; return v.Count == 0 ? 0 : a / v.Count; }
        string F(double d) => d.ToString("F2", CultureInfo.InvariantCulture);
        return $"{{ \"segment\": {Q(name)}, \"fpsP5\": {F(P(fps, 0.05))}, \"fpsMedian\": {F(P(fps, 0.5))}, \"gpuMsP95\": {F(P(gpu, 0.95))}, \"gpuMsMean\": {F(mean(gpu))}, \"cpuMainMsP95\": {F(P(cpu, 0.95))}, \"frames\": {s.Count} }}";
    }

    // ------------------------------------------------------------------ cena
    private void BuildScene()
    {
        Mesh cube = PrimitiveMesh(PrimitiveType.Cube);
        Mesh capsule = PrimitiveMesh(PrimitiveType.Capsule);
        Mesh tree = BuildTreeMesh();

        Material ground = Lit(new Color(0.30f, 0.62f, 0.33f));
        var buildingMats = new[]
        {
            Lit(new Color(0.78f, 0.78f, 0.80f)), Lit(new Color(0.70f, 0.64f, 0.56f)),
            Lit(new Color(0.60f, 0.68f, 0.78f)), Lit(new Color(0.82f, 0.74f, 0.62f)),
            Lit(new Color(0.66f, 0.66f, 0.70f)), Lit(new Color(0.74f, 0.60f, 0.58f)),
        };
        Material treeMat = Lit(new Color(0.22f, 0.55f, 0.27f));
        Material agentMat = Lit(new Color(0.85f, 0.45f, 0.30f));

        var groundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Destroy(groundGo.GetComponent<Collider>());
        groundGo.transform.localScale = new Vector3(140f, 1f, 140f);
        groundGo.GetComponent<Renderer>().sharedMaterial = ground;

        var rng = new System.Random(7);
        Transform buildingsRoot = new GameObject("Buildings").transform;
        int side = Mathf.CeilToInt(Mathf.Sqrt(_buildings));
        float step = (_areaBuild * 2f) / side;
        int placed = 0;
        for (int gx = 0; gx < side && placed < _buildings; gx++)
        {
            for (int gz = 0; gz < side && placed < _buildings; gz++)
            {
                float h = 6f + (float)rng.NextDouble() * 34f;
                float w = step * (0.45f + (float)rng.NextDouble() * 0.25f);
                var pos = new Vector3(-_areaBuild + gx * step + step * 0.5f, h * 0.5f, -_areaBuild + gz * step + step * 0.5f);
                SpawnMesh("B", cube, buildingMats[placed % buildingMats.Length], pos, new Vector3(w, h, w), buildingsRoot);
                placed++;
            }
        }

        Transform treesRoot = new GameObject("Trees").transform;
        for (int i = 0; i < _trees; i++)
        {
            float x = (float)(rng.NextDouble() * 2 - 1) * _areaTrees;
            float z = (float)(rng.NextDouble() * 2 - 1) * _areaTrees;
            float s = 0.8f + (float)rng.NextDouble() * 0.8f;
            SpawnMesh("T", tree, treeMat, new Vector3(x, 0f, z), new Vector3(s, s, s), treesRoot);
        }

        Transform agentsRoot = new GameObject("Agents").transform;
        _agentT = new Transform[_agents];
        _agentVel = new Vector3[_agents];
        for (int i = 0; i < _agents; i++)
        {
            float x = (float)(rng.NextDouble() * 2 - 1) * _areaBuild;
            float z = (float)(rng.NextDouble() * 2 - 1) * _areaBuild;
            Transform tr = SpawnMesh("A", capsule, agentMat, new Vector3(x, 1f, z), new Vector3(0.5f, 1f, 0.5f), agentsRoot);
            _agentT[i] = tr;
            float a = (float)(rng.NextDouble() * Math.PI * 2);
            _agentVel[i] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (1.2f + (float)rng.NextDouble() * 1.5f);
        }

        // Luzes de rua (Forward+): apagadas de dia, acesas de noite.
        Transform lightsRoot = new GameObject("Streetlights").transform;
        int lside = 11; // 121 luzes
        float lstep = (_areaBuild * 2f) / lside;
        for (int gx = 0; gx < lside; gx++)
        {
            for (int gz = 0; gz < lside; gz++)
            {
                var go = new GameObject("L");
                go.transform.SetParent(lightsRoot);
                go.transform.position = new Vector3(-_areaBuild + gx * lstep, 8f, -_areaBuild + gz * lstep);
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 28f;
                l.intensity = 2.2f;
                l.color = new Color(1f, 0.92f, 0.75f);
                l.enabled = false;
                _streetlights.Add(l);
            }
        }

        var sunGo = new GameObject("Sun");
        _sun = sunGo.AddComponent<Light>();
        _sun.type = LightType.Directional;
        _sun.shadows = LightShadows.Soft;
    }

    private Transform SpawnMesh(string name, Mesh mesh, Material mat, Vector3 pos, Vector3 scale, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return go.transform;
    }

    private void AnimateAgents()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _agentT.Length; i++)
        {
            Transform tr = _agentT[i];
            Vector3 p = tr.position + _agentVel[i] * dt;
            if (Mathf.Abs(p.x) > _areaBuild) { _agentVel[i].x = -_agentVel[i].x; p.x = Mathf.Clamp(p.x, -_areaBuild, _areaBuild); }
            if (Mathf.Abs(p.z) > _areaBuild) { _agentVel[i].z = -_agentVel[i].z; p.z = Mathf.Clamp(p.z, -_areaBuild, _areaBuild); }
            p.y = 1f + Mathf.Sin(Time.time * 6f + i) * 0.08f; // bob (proxy de passo)
            tr.position = p;
            tr.forward = _agentVel[i].sqrMagnitude > 0.001f ? _agentVel[i].normalized : tr.forward;
        }
    }

    private void SetZoom(string zoom)
    {
        (float height, float dist) = zoom switch
        {
            "near" => (25f, 50f),
            "far" => (450f, 1100f),
            _ => (120f, 300f),
        };
        _cam.transform.position = new Vector3(0f, height, -dist);
        _cam.transform.LookAt(new Vector3(0f, 0f, 0f));
        _cam.farClipPlane = 3000f;
    }

    private void SetPeriod(bool day)
    {
        if (day)
        {
            _sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            _sun.intensity = 1.2f;
            _sun.color = new Color(1f, 0.96f, 0.9f);
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
        }
        else
        {
            _sun.transform.rotation = Quaternion.Euler(8f, -20f, 0f);
            _sun.intensity = 0.06f;
            _sun.color = new Color(0.4f, 0.45f, 0.7f);
            RenderSettings.ambientLight = new Color(0.07f, 0.08f, 0.12f);
        }

        foreach (Light l in _streetlights)
        {
            l.enabled = !day;
        }
    }

    // ------------------------------------------------------------------ utilidades
    private static Mesh PrimitiveMesh(PrimitiveType type)
    {
        var go = GameObject.CreatePrimitive(type);
        Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
        Destroy(go);
        return m;
    }

    private static Mesh BuildTreeMesh()
    {
        var trunkGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var foliageGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var combines = new CombineInstance[2];
        combines[0].mesh = trunkGo.GetComponent<MeshFilter>().sharedMesh;
        combines[0].transform = Matrix4x4.TRS(new Vector3(0, 1f, 0), Quaternion.identity, new Vector3(0.18f, 1f, 0.18f));
        combines[1].mesh = foliageGo.GetComponent<MeshFilter>().sharedMesh;
        combines[1].transform = Matrix4x4.TRS(new Vector3(0, 2.6f, 0), Quaternion.Euler(0, 45, 0), new Vector3(1.6f, 1.8f, 1.6f));
        var mesh = new Mesh();
        mesh.CombineMeshes(combines, true, true);
        mesh.RecalculateNormals();
        Destroy(trunkGo);
        Destroy(foliageGo);
        return mesh;
    }

    private static Material Lit(Color c)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(s) { color = c, enableInstancing = true };
        return m;
    }

    private static string GrdStatus()
    {
        var urp = GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        if (urp == null)
        {
            return "no-urp";
        }

        try { return urp.gpuResidentDrawerMode.ToString(); }
        catch { return "unknown"; }
    }

    private static string Q(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    private void ParseArgs()
    {
        string[] a = Environment.GetCommandLineArgs();
        for (int i = 0; i < a.Length; i++)
        {
            switch (a[i])
            {
                case "-buildings" when i + 1 < a.Length: int.TryParse(a[i + 1], out _buildings); break;
                case "-trees" when i + 1 < a.Length: int.TryParse(a[i + 1], out _trees); break;
                case "-agents" when i + 1 < a.Length: int.TryParse(a[i + 1], out _agents); break;
                case "-frames" when i + 1 < a.Length: int.TryParse(a[i + 1], out _framesPerSegment); break;
                case "-warmup" when i + 1 < a.Length: float.TryParse(a[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out _warmupSeconds); break;
                case "-outdir" when i + 1 < a.Length: _outDir = a[i + 1]; break;
            }
        }
    }
}
