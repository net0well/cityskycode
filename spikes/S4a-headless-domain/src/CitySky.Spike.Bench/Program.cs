using System.Runtime;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;
using CitySky.Spike.Bench;

// Uso:
//   dotnet run -c Release                 → medidor por tick (percentis, pior tick, alocação, hash)
//   dotnet run -c Release -- bdn [filtros] → BenchmarkDotNet (média e alocação por tick)
if (args.Length > 0 && args[0] == "bdn")
{
    BenchmarkSwitcher.FromAssembly(typeof(TickBenchmarks).Assembly).Run(args[1..]);
    return;
}

const ulong Seed = 42;
const int Repetitions = 3;
int[] sizes = [1_000, 10_000, 100_000];

Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription} · {RuntimeInformation.OSDescription} · {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Processadores lógicos: {Environment.ProcessorCount} · GC: {(GCSettings.IsServerGC ? "Server" : "Workstation")}");
Console.WriteLine();

Console.WriteLine("| Cidadãos | Rodada | Média (ms) | p50 | p95 | p99 | Máx | Máx na virada de hora | Alocado no dia (B) | Viagens ativas (média / máx) |");
Console.WriteLine("|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|");
foreach (int size in sizes)
{
    for (int round = 1; round <= Repetitions; round++)
    {
        DayResult r = DayProfiler.MeasureSecondDay(size, Seed);
        Console.WriteLine(
            $"| {size:N0} | {round} | {r.MeanMs:F4} | {r.P50Ms:F4} | {r.P95Ms:F4} | {r.P99Ms:F4} | {r.MaxMs:F4} | {r.HourBoundaryMaxMs:F4} | {r.AllocatedBytes:N0} | {r.MeanActiveTrips:N0} / {r.MaxActiveTrips:N0} |");
    }
}

Console.WriteLine();
Console.WriteLine("| Cidadãos | Memória dos stores (MB) |");
Console.WriteLine("|---:|---:|");
foreach (int size in sizes)
{
    Console.WriteLine($"| {size:N0} | {DayProfiler.StoreBytes(size, Seed) / (1024.0 * 1024.0):F1} |");
}

Console.WriteLine();
ulong first = DayProfiler.HashAfterDays(100_000, Seed, days: 2);
ulong second = DayProfiler.HashAfterDays(100_000, Seed, days: 2);
ulong otherSeed = DayProfiler.HashAfterDays(100_000, Seed + 1, days: 2);
Console.WriteLine($"Hash 100k, seed {Seed}, 2 dias: {first:X16} / {second:X16} → {(first == second ? "IGUAIS" : "DIFERENTES")}");
Console.WriteLine($"Hash 100k, seed {Seed + 1} (controle, deve diferir): {otherSeed:X16} → {(otherSeed != first ? "ok" : "ERRO")}");
