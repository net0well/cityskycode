using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CitySky.Spike.Domain;

namespace CitySky.Spike.Bench
{
    /// <summary>
    /// Mesmo benchmark do S4a, mas rodável dentro da Unity (Mono no Editor, Mono/IL2CPP no player).
    /// Mede o custo por tick de um dia TC com 100k cidadãos e o hash de estado, para comparar
    /// o fator de runtime com o .NET 10 e confirmar o determinismo entre runtimes (AC-13).
    /// </summary>
    public static class BenchCore
    {
        public static string Run(int citizens, ulong seed)
        {
            Simulation sim = PopulationGenerator.Create(citizens, seed);
            for (int i = 0; i < SimTime.TicksPerDay; i++)
            {
                sim.RunTick();
            }

            var elapsed = new long[SimTime.TicksPerDay];
            long allocBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < SimTime.TicksPerDay; i++)
            {
                long start = Stopwatch.GetTimestamp();
                sim.RunTick();
                elapsed[i] = Stopwatch.GetTimestamp() - start;
            }

            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocBefore;
            ulong hash = StateHasher.Compute(sim.State);

            long total = 0;
            foreach (long e in elapsed)
            {
                total += e;
            }

            Array.Sort(elapsed);
            double toMs = 1000.0 / Stopwatch.Frequency;
            double mean = total * toMs / elapsed.Length;
            double p50 = elapsed[elapsed.Length / 2] * toMs;
            double p95 = elapsed[(int)(elapsed.Length * 0.95)] * toMs;
            double max = elapsed[elapsed.Length - 1] * toMs;

            // Hash também após 2 dias, para comparar com o .NET (determinismo).
            Simulation sim2 = PopulationGenerator.Create(citizens, seed);
            for (long i = 0; i < 2L * SimTime.TicksPerDay; i++)
            {
                sim2.RunTick();
            }

            ulong hash2days = StateHasher.Compute(sim2.State);

            var sb = new StringBuilder();
            sb.Append("citizens=").Append(citizens.ToString(CultureInfo.InvariantCulture));
            sb.Append(" seed=").Append(seed.ToString(CultureInfo.InvariantCulture));
            sb.Append(" meanMs=").Append(mean.ToString("F4", CultureInfo.InvariantCulture));
            sb.Append(" p50Ms=").Append(p50.ToString("F4", CultureInfo.InvariantCulture));
            sb.Append(" p95Ms=").Append(p95.ToString("F4", CultureInfo.InvariantCulture));
            sb.Append(" maxMs=").Append(max.ToString("F4", CultureInfo.InvariantCulture));
            sb.Append(" allocBytesDay=").Append(allocated.ToString(CultureInfo.InvariantCulture));
            sb.Append(" hash1day=").Append(hash.ToString("X16"));
            sb.Append(" hash2days=").Append(hash2days.ToString("X16"));
            return sb.ToString();
        }
    }
}
