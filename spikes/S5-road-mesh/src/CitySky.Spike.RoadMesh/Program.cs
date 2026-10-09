using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using CitySky.Spike.RoadMesh;

string outPath = Args("--out") ?? Path.Combine(AppContext.BaseDirectory, "s5-results.json");
var builder = new RoadMeshBuilder(widthMeters: 10f);

var results = new JsonObject
{
    ["runtime"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    ["note"] = "Geometria independente da engine (System.Numerics). O upload do Mesh na Unity e o snapshot visual ficam para a Fase 3.",
};

// ---------------------------------------------------------------------------------------------
// 12 configurações de referência (plano do S5). Ângulos em graus a partir do nó central.
// ---------------------------------------------------------------------------------------------
var configs = new (string Name, double[] AnglesDeg, double[] Lengths)[]
{
    ("T 90graus", new[] { 0.0, 90, 180 }, new[] { 60.0, 60, 60 }),
    ("X 90graus (4 ramos)", new[] { 0.0, 90, 180, 270 }, new[] { 60.0, 60, 60, 60 }),
    ("Y 30/150", new[] { 90.0, 210, 330 }, new[] { 60.0, 60, 60 }),
    ("X assimetrico 30/100/200/290", new[] { 30.0, 100, 200, 290 }, new[] { 60.0, 60, 60, 60 }),
    ("ramos de comprimentos diferentes", new[] { 0.0, 90, 180, 270 }, new[] { 20.0, 120, 40, 80 }),
    ("segmento curto 16m entre 2 nos", new[] { 0.0, 180 }, new[] { 16.0, 60 }),
    ("intersecao em rampa (plano no spike)", new[] { 0.0, 90, 180, 270 }, new[] { 60.0, 60, 60, 60 }),
    ("3 ramos quase colineares 170graus", new[] { 5.0, 175, 270 }, new[] { 60.0, 60, 60 }),
    ("4 ramos, um a 30graus do vizinho", new[] { 0.0, 30, 120, 220 }, new[] { 60.0, 60, 60, 60 }),
    ("fim de linha (1 ramo)", new[] { 0.0 }, new[] { 60.0 }),
    ("dois nos proximos (>=16m)", Array.Empty<double>(), Array.Empty<double>()), // tratado à parte
    ("passagem reta (2 ramos colineares)", new[] { 0.0, 180 }, new[] { 60.0, 60 }),
};

var array = new JsonArray();
var meshesForSvg = new List<(string, RoadMesh)>();
bool allOk = true;
foreach (var c in configs)
{
    RoadGraph graph = c.Name.StartsWith("dois nos") ? TwoClose() : Star(c.AnglesDeg, c.Lengths);
    RoadMesh mesh = builder.Build(graph);
    meshesForSvg.Add((c.Name, mesh));
    MeshChecks checks = GeometryChecks.Check(mesh);
    allOk &= checks.Ok;
    array.Add(new JsonObject
    {
        ["config"] = c.Name,
        ["vertices"] = checks.Vertices,
        ["triangles"] = checks.Triangles,
        ["degenerate"] = checks.DegenerateTriangles,
        ["downfacing"] = checks.DownfacingTriangles,
        ["nans"] = checks.NaNs,
        ["minTriAreaM2"] = Math.Round(checks.MinTriangleAreaM2, 5),
        ["ok"] = checks.Ok,
    });
}

results["configs"] = array;
results["allConfigsOk"] = allOk;

string svgPath = Path.Combine(Path.GetDirectoryName(outPath)!, "s5-configs.svg");
Svg.WriteGrid(svgPath, meshesForSvg);
results["svg"] = Path.GetFileName(svgPath);

// ---------------------------------------------------------------------------------------------
// Desempenho: regenerar uma grade com ~100 interseções de 4 ramos.
// ---------------------------------------------------------------------------------------------
RoadGraph grid = Grid(10, 10, spacing: 80f); // 10x10 = 100 nós internos/externos, ~100 interseções
int intersections = CountIntersections(grid);

// Aquecimento.
for (int i = 0; i < 3; i++)
{
    builder.Build(grid);
}

var samples = new List<double>();
long allocBefore = GC.GetAllocatedBytesForCurrentThread();
for (int i = 0; i < 20; i++)
{
    long t = Stopwatch.GetTimestamp();
    RoadMesh m = builder.Build(grid);
    samples.Add(Stopwatch.GetElapsedTime(t).TotalMilliseconds);
    GC.KeepAlive(m);
}

long allocPerBuild = (GC.GetAllocatedBytesForCurrentThread() - allocBefore) / 20;
samples.Sort();
RoadMesh gridMesh = builder.Build(grid);

results["grid"] = new JsonObject
{
    ["nodes"] = grid.Nodes.Count,
    ["segments"] = grid.Segments.Count,
    ["intersections4way"] = intersections,
    ["vertices"] = gridMesh.Vertices.Count,
    ["triangles"] = gridMesh.Triangles.Count / 3,
    ["buildMsP50"] = Math.Round(samples[samples.Count / 2], 3),
    ["buildMsP95"] = Math.Round(samples[(int)(samples.Count * 0.95)], 3),
    ["buildMsMax"] = Math.Round(samples[^1], 3),
    ["allocPerBuildKB"] = Math.Round(allocPerBuild / 1024.0, 1),
    ["targetMs"] = 50,
    ["meetsTarget"] = samples[(int)(samples.Count * 0.95)] <= 50,
};

string json = results.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outPath, json);
Console.WriteLine(json);
return allOk ? 0 : 1;

// ---------------------------------------------------------------------------------------------
static RoadGraph Star(double[] anglesDeg, double[] lengths)
{
    var g = new RoadGraph();
    int center = g.AddNode(0, 0);
    for (int i = 0; i < anglesDeg.Length; i++)
    {
        double rad = anglesDeg[i] * Math.PI / 180.0;
        int outer = g.AddNode((float)(Math.Cos(rad) * lengths[i]), (float)(Math.Sin(rad) * lengths[i]));
        g.Connect(center, outer);
    }

    return g;
}

static RoadGraph TwoClose()
{
    var g = new RoadGraph();
    int a = g.AddNode(0, 0);
    int b = g.AddNode(16, 0); // 16 m entre os nós
    g.Connect(a, b);
    g.Connect(a, g.AddNode(-60, 0));
    g.Connect(a, g.AddNode(0, 60));
    g.Connect(b, g.AddNode(76, 0));
    g.Connect(b, g.AddNode(16, -60));
    return g;
}

static RoadGraph Grid(int cols, int rows, float spacing)
{
    var g = new RoadGraph();
    var id = new int[cols, rows];
    for (int x = 0; x < cols; x++)
    {
        for (int y = 0; y < rows; y++)
        {
            id[x, y] = g.AddNode(x * spacing, y * spacing);
        }
    }

    for (int x = 0; x < cols; x++)
    {
        for (int y = 0; y < rows; y++)
        {
            if (x + 1 < cols)
            {
                g.Connect(id[x, y], id[x + 1, y]);
            }

            if (y + 1 < rows)
            {
                g.Connect(id[x, y], id[x, y + 1]);
            }
        }
    }

    return g;
}

static int CountIntersections(RoadGraph g)
{
    var degree = new int[g.Nodes.Count];
    foreach ((int a, int b) in g.Segments)
    {
        degree[a]++;
        degree[b]++;
    }

    return degree.Count(d => d >= 3);
}

static string? Args(string name)
{
    string[] a = Environment.GetCommandLineArgs();
    int i = Array.IndexOf(a, name);
    return i >= 0 && i + 1 < a.Length ? a[i + 1] : null;
}
