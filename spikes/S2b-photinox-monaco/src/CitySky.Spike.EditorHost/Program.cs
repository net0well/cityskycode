using System.Diagnostics;
using CitySky.Spike.EditorHost;

// Uso:
//   dotnet run -c Release                            → abre o editor para teste manual (DevTools ligado)
//   dotnet run -c Release -- --selftest --out x.json → autoteste do spike S2b e sai
//   dotnet run -c Release -- --screenshots <pasta>   → capturas do painel embutido (normal, expandido, minimizado)
var sinceStart = Stopwatch.StartNew();
bool selfTest = args.Contains("--selftest");
string? screenshots = ArgValue("--screenshots");
string output = ArgValue("--out") ?? Path.Combine(AppContext.BaseDirectory, "selftest.json");

return new EditorSession(
    selfTest,
    screenshots is null ? null : Path.GetFullPath(screenshots),
    Path.GetFullPath(output),
    sinceStart).Run();

string? ArgValue(string name)
{
    int index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}
