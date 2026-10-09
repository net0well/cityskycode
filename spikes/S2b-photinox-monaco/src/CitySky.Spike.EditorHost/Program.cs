using System.Diagnostics;
using CitySky.Spike.EditorHost;

// Uso:
//   dotnet run -c Release                          → abre o editor para teste manual (DevTools ligado)
//   dotnet run -c Release -- --selftest --out x.json → autoteste do spike S2b e sai
var sinceStart = Stopwatch.StartNew();
bool selfTest = args.Contains("--selftest");
int outIndex = Array.IndexOf(args, "--out");
string output = outIndex >= 0 && outIndex + 1 < args.Length
    ? Path.GetFullPath(args[outIndex + 1])
    : Path.Combine(AppContext.BaseDirectory, "selftest.json");

return new EditorSession(selfTest, output, sinceStart).Run();
