using System.Diagnostics;
using CitySky.Spike.Domain;

namespace CitySky.Spike.Bench;

public sealed record DayResult(
    int Citizens,
    double MeanMs,
    double P50Ms,
    double P95Ms,
    double P99Ms,
    double MaxMs,
    double HourBoundaryMaxMs,
    long AllocatedBytes,
    double MeanActiveTrips,
    int MaxActiveTrips,
    ulong Hash);

/// <summary>
/// Mede cada tick de um dia TC completo (11.520 ticks) depois de 1 dia de aquecimento,
/// para capturar percentis e o pior tick (que o BenchmarkDotNet não mostra).
/// </summary>
public static class DayProfiler
{
    public static DayResult MeasureSecondDay(int citizens, ulong seed)
    {
        Simulation simulation = PopulationGenerator.Create(citizens, seed);
        for (int i = 0; i < SimTime.TicksPerDay; i++)
        {
            simulation.RunTick();
        }

        var elapsed = new long[SimTime.TicksPerDay];
        long activeTripsSum = 0;
        int maxActiveTrips = 0;

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < SimTime.TicksPerDay; i++)
        {
            long start = Stopwatch.GetTimestamp();
            simulation.RunTick();
            elapsed[i] = Stopwatch.GetTimestamp() - start;

            int active = simulation.State.Trips.Count;
            activeTripsSum += active;
            if (active > maxActiveTrips)
            {
                maxActiveTrips = active;
            }
        }
        long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        double hourBoundaryMax = 0;
        for (int i = 0; i < elapsed.Length; i += SimTime.TicksPerHour)
        {
            hourBoundaryMax = Math.Max(hourBoundaryMax, ToMs(elapsed[i]));
        }

        long total = elapsed.Sum();
        Array.Sort(elapsed);

        return new DayResult(
            citizens,
            MeanMs: ToMs(total) / elapsed.Length,
            P50Ms: ToMs(Percentile(elapsed, 0.50)),
            P95Ms: ToMs(Percentile(elapsed, 0.95)),
            P99Ms: ToMs(Percentile(elapsed, 0.99)),
            MaxMs: ToMs(elapsed[^1]),
            HourBoundaryMaxMs: hourBoundaryMax,
            AllocatedBytes: allocatedBytes,
            MeanActiveTrips: (double)activeTripsSum / SimTime.TicksPerDay,
            MaxActiveTrips: maxActiveTrips,
            Hash: StateHasher.Compute(simulation.State));
    }

    public static ulong HashAfterDays(int citizens, ulong seed, int days)
    {
        Simulation simulation = PopulationGenerator.Create(citizens, seed);
        for (long i = 0; i < (long)days * SimTime.TicksPerDay; i++)
        {
            simulation.RunTick();
        }

        return StateHasher.Compute(simulation.State);
    }

    /// <summary>Memória gerenciada retida pelos stores de uma cidade recém-criada.</summary>
    public static long StoreBytes(int citizens, ulong seed)
    {
        long before = GC.GetTotalMemory(forceFullCollection: true);
        Simulation simulation = PopulationGenerator.Create(citizens, seed);
        long after = GC.GetTotalMemory(forceFullCollection: true);
        GC.KeepAlive(simulation);
        return after - before;
    }

    private static long Percentile(long[] sorted, double p) => sorted[(int)Math.Min(sorted.Length - 1, p * sorted.Length)];

    private static double ToMs(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;
}
