using BenchmarkDotNet.Attributes;
using CitySky.Spike.Domain;

namespace CitySky.Spike.Bench;

/// <summary>
/// Custo médio por tick e alocação por tick (MemoryDiagnoser). O estado avança a cada invocação,
/// então a média percorre as horas do dia; o pior tick vem do medidor próprio (<see cref="DayProfiler"/>).
/// </summary>
[MemoryDiagnoser]
public class TickBenchmarks
{
    private Simulation _simulation = null!;

    [Params(1_000, 10_000, 100_000)]
    public int Citizens { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _simulation = PopulationGenerator.Create(Citizens, seed: 42);
        for (int i = 0; i < SimTime.TicksPerDay; i++)
        {
            _simulation.RunTick();
        }
    }

    [Benchmark]
    public void Tick() => _simulation.RunTick();
}
