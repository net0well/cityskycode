using CitySky.Spike.EditorHost;

// Uso:
//   run -- --unity <caminho do S2aPlayer.exe> [--out <json>] [--shots <pasta>] [--hold]
//   --hold  mantém o editor embutido aberto para o checklist humano (não fecha ao fim).
string? unity = ArgValue("--unity");
if (unity is null)
{
    Console.Error.WriteLine("falta --unity <caminho do player Unity do S2a>");
    return 2;
}

string output = ArgValue("--out") ?? Path.Combine(AppContext.BaseDirectory, "s2a-result.json");
string shots = ArgValue("--shots") ?? Path.Combine(AppContext.BaseDirectory, "shots");
bool hold = args.Contains("--hold");

return new S2aSession(Path.GetFullPath(unity), Path.GetFullPath(output), Path.GetFullPath(shots), hold).Run();

string? ArgValue(string name)
{
    int index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}
