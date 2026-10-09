using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using CitySky.Spike.CodeHost;
using CitySky.Spike.Contracts;

// Budgets do spike (CODE_EXECUTION.md §7, modo Script).
const long BudgetTicks = 5_000_000;
const int MaxDepth = 256;
const long AllocationLimit = 128L * 1024 * 1024;
const int OutputLimit = 64 * 1024;
const int WallClockMs = 2000;

string outPath = GetArg("--out") ?? Path.Combine(AppContext.BaseDirectory, "s1a-results.json");
string workerDll = FindWorker();

Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
Console.WriteLine("Preparando Roslyn (referências + Player API)...");

var warmTotal = Stopwatch.StartNew();
var references = ReferenceSet.Create();
var pipeline = new Pipeline(references);

// A Player API (stub) precisa estar na pasta do Worker, como seria no produto.
File.Copy(references.PlayerApiPath, Path.Combine(Path.GetDirectoryName(workerDll)!, "CitySky.PlayerApi.dll"), overwrite: true);
using var pool = new WorkerPool(workerDll);

var results = new JsonObject
{
    ["runtime"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    ["refPackVersion"] = references.RefPackVersion,
    ["budgets"] = new JsonObject { ["ticks"] = BudgetTicks, ["maxDepth"] = MaxDepth, ["allocationMB"] = AllocationLimit / (1024 * 1024), ["wallClockMs"] = WallClockMs },
};

// ---------------------------------------------------------------------------------------------
// Corpus. Cada caso tem um "esperado" verificável.
// ---------------------------------------------------------------------------------------------
var cases = new (string Name, string Source, bool EnforceL1, string Expect)[]
{
    ("hello", """
        using CitySky;
        var city = CityGame.Current;
        var house = new House();
        var r = city.Build(house);
        city.Log(r.Message);
        """, true, "ok"),

    ("loop-regressivo", """
        long soma = 0;
        for (int i = 0; i < 10; i--)   // nunca termina
        {
            soma = soma + 1;
        }
        """, true, "budget:Loop@2"),

    ("recursao-infinita", """
        using CitySky;
        int Fatorial(int n) => n * Fatorial(n - 1);   // sem caso base
        var city = CityGame.Current;
        city.Log(Fatorial(5).ToString());
        """, true, "budget:Recursion"),

    ("catch-nao-engole", """
        long soma = 0;
        try
        {
            while (true) { soma = soma + 1; }
        }
        catch { soma = -1; }
        """, true, "budget:Loop"),

    ("memoria", """
        using System.Collections.Generic;
        var lista = new List<byte[]>();
        while (true) { lista.Add(new byte[1024 * 1024]); }
        """, true, "budget:Memory"),

    ("goto-sem-tick", """
        int i = 0;
        inicio:
        i = i + 1;
        if (i > 0) goto inicio;   // laço que o rewriter de laços não vê
        """, true, "budget-or-watchdog"),

    ("api-proibida-L1", """
        var texto = System.IO.File.ReadAllText("c:/segredo.txt");
        """, true, "L1"),

    ("api-proibida-L2-sem-L1", """
        var texto = System.IO.File.ReadAllText("c:/segredo.txt");
        """, false, "L2"),

    ("excecao-linha", """
        using CitySky;
        var city = CityGame.Current;
        string? nome = null;
        var tamanho = nome.Length;   // NullReference na linha 4
        city.Log(tamanho.ToString());
        """, true, "exception@4"),

    ("saida-limitada", """
        using CitySky;
        var city = CityGame.Current;
        for (int i = 0; i < 100000; i++) { city.Log("linha de log bem comprida para encher o buffer"); }
        """, true, "output-truncated-or-budget"),
};

var corpus = new JsonArray();
foreach (var c in cases)
{
    corpus.Add(RunCase(c.Name, c.Source, c.EnforceL1, c.Expect));
}

results["corpus"] = corpus;

// ---------------------------------------------------------------------------------------------
// Tempos frio × quente do "hello".
// ---------------------------------------------------------------------------------------------
results["coldHello"] = MeasureEndToEnd(cases[0].Source);  // primeira execução = fria
var hot = new List<double>();
for (int i = 0; i < 50; i++)
{
    hot.Add(MeasureEndToEndMs(cases[0].Source));
}
hot.Sort();
results["hotHelloMs"] = new JsonObject
{
    ["n"] = hot.Count,
    ["p50"] = Math.Round(hot[hot.Count / 2], 1),
    ["p95"] = Math.Round(hot[(int)(hot.Count * 0.95)], 1),
    ["max"] = Math.Round(hot[^1], 1),
};

// ---------------------------------------------------------------------------------------------
// Estabilidade: 200 execuções do hello, Worker reciclado a cada 200 (sem violação).
// ---------------------------------------------------------------------------------------------
long before = 0;
for (int i = 0; i < 200; i++)
{
    RunResponse r = CompileAndRun(cases[0].Source, true);
    if (i == 0)
    {
        before = r.WorkerWorkingSetBytes;
    }

    if (i == 199)
    {
        results["stability"] = new JsonObject
        {
            ["runs"] = 200,
            ["workerWorkingSetFirstMB"] = Math.Round(before / (1024.0 * 1024.0), 1),
            ["workerWorkingSetLastMB"] = Math.Round(r.WorkerWorkingSetBytes / (1024.0 * 1024.0), 1),
            ["managedHeapLastMB"] = Math.Round(r.ManagedHeapBytes / (1024.0 * 1024.0), 1),
            ["restarts"] = pool.Restarts,
        };
    }
}

results["warmupTotalMs"] = Math.Round(warmTotal.Elapsed.TotalMilliseconds, 0);

string json = results.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outPath, json);
Console.WriteLine(json);
return 0;

// ---------------------------------------------------------------------------------------------
JsonObject RunCase(string name, string source, bool enforceL1, string expect)
{
    CompileResult compile = pipeline.Compile(source, enforceL1);
    var obj = new JsonObject
    {
        ["name"] = name,
        ["expect"] = expect,
        ["stage"] = compile.Stage,
        ["compileMs"] = Math.Round(compile.Timings.TotalMs, 2),
    };

    if (compile.Findings.Count > 0)
    {
        obj["findings"] = new JsonArray(compile.Findings.Select(f => (JsonNode)$"{f.Code}@{f.Line}: {f.Message}").ToArray());
    }

    if (compile.Diagnostics.Count > 0)
    {
        obj["diagnostics"] = new JsonArray(compile.Diagnostics.Select(d => (JsonNode)d).ToArray());
    }

    if (compile.Ok)
    {
        RunResponse r = pool.Run(
            new RunRequest(compile.Assembly!, compile.Pdb!, BudgetTicks, MaxDepth, AllocationLimit, OutputLimit),
            WallClockMs);
        obj["run"] = new JsonObject
        {
            ["status"] = r.Status,
            ["reason"] = r.Reason,
            ["line"] = r.Line,
            ["exceptionType"] = r.ExceptionType,
            ["message"] = r.Message,
            ["output"] = r.Output.Length > 80 ? r.Output[..80] + "…" : r.Output,
            ["outputTruncated"] = r.OutputTruncated,
            ["loadMs"] = Math.Round(r.LoadMs, 2),
            ["runMs"] = Math.Round(r.RunMs, 2),
        };
        obj["verdict"] = Verdict(expect, r.Status, r.Reason, r.Line, r.OutputTruncated);
    }
    else
    {
        obj["verdict"] = Verdict(expect, compile.Stage, null, compile.Findings.FirstOrDefault()?.Line ?? 0, false);
    }

    return obj;
}

static string Verdict(string expect, string status, string? reason, int line, bool truncated)
{
    bool ok = expect switch
    {
        "ok" => status == "ok",
        "L1" => status == "L1",
        "L2" => status == "L2",
        "budget:Loop" => status == "budget" && reason == "Loop",
        "budget:Recursion" => status == "budget" && reason == "Recursion",
        "budget:Memory" => status == "budget" && reason == "Memory",
        "budget-or-watchdog" => status is "budget" or "watchdog",
        "output-truncated-or-budget" => truncated || status == "budget",
        _ when expect.StartsWith("budget:Loop@") => status == "budget" && reason == "Loop" && line == int.Parse(expect[12..]),
        _ when expect.StartsWith("exception@") => status == "exception" && line == int.Parse(expect[10..]),
        _ => false,
    };
    return ok ? "✓ PASSOU" : $"✗ FALHOU (obtido: {status}/{reason}/linha {line})";
}

JsonObject MeasureEndToEnd(string source)
{
    long t = Stopwatch.GetTimestamp();
    CompileResult c = pipeline.Compile(source, true);
    double compileMs = Stopwatch.GetElapsedTime(t).TotalMilliseconds;
    RunResponse r = pool.Run(new RunRequest(c.Assembly!, c.Pdb!, BudgetTicks, MaxDepth, AllocationLimit, OutputLimit), WallClockMs);
    return new JsonObject
    {
        ["compileMs"] = Math.Round(compileMs, 1),
        ["loadMs"] = Math.Round(r.LoadMs, 1),
        ["runMs"] = Math.Round(r.RunMs, 1),
        ["totalMs"] = Math.Round(compileMs + r.LoadMs + r.RunMs, 1),
    };
}

double MeasureEndToEndMs(string source)
{
    long t = Stopwatch.GetTimestamp();
    CompileResult c = pipeline.Compile(source, true);
    RunResponse r = pool.Run(new RunRequest(c.Assembly!, c.Pdb!, BudgetTicks, MaxDepth, AllocationLimit, OutputLimit), WallClockMs);
    return Stopwatch.GetElapsedTime(t).TotalMilliseconds + r.LoadMs + r.RunMs;
}

RunResponse CompileAndRun(string source, bool enforceL1)
{
    CompileResult c = pipeline.Compile(source, enforceL1);
    return pool.Run(new RunRequest(c.Assembly!, c.Pdb!, BudgetTicks, MaxDepth, AllocationLimit, OutputLimit), WallClockMs);
}

static string? GetArg(string name)
{
    string[] args = Environment.GetCommandLineArgs();
    int i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

static string FindWorker()
{
    string baseDir = AppContext.BaseDirectory;
    string worker = baseDir.Replace("CitySky.Spike.CodeHost", "CitySky.Spike.Worker");
    string dll = Path.Combine(worker, "CitySky.Spike.Worker.dll");
    if (!File.Exists(dll))
    {
        throw new FileNotFoundException($"Worker não encontrado em {dll}. Compile a solução antes.");
    }

    return dll;
}
