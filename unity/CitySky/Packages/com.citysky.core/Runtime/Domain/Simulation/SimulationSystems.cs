using System;

namespace CitySky.Domain
{
    /// <summary>
    /// Período (em ticks) + fase (deslocamento) de um sistema. Período 1 / fase 0 = todo tick. F1-33.
    /// </summary>
    public readonly struct TickSchedule : IEquatable<TickSchedule>
    {
        public readonly int Period;
        public readonly int Phase;

        public TickSchedule(int period, int phase = 0)
        {
            if (period <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(period), "O período deve ser ≥ 1.");
            }

            if (phase < 0 || phase >= period)
            {
                throw new ArgumentOutOfRangeException(nameof(phase), "A fase deve estar em [0, período).");
            }

            Period = period;
            Phase = phase;
        }

        public static readonly TickSchedule EveryTick = new TickSchedule(1, 0);

        /// <summary>O sistema roda no tick <paramref name="tick"/>?</summary>
        public bool IsDue(long tick) => tick >= Phase && (tick - Phase) % Period == 0;

        public bool Equals(TickSchedule other) => Period == other.Period && Phase == other.Phase;
        public override bool Equals(object? obj) => obj is TickSchedule o && Equals(o);
        public override int GetHashCode() => (Period * 397) ^ Phase;
        public static bool operator ==(TickSchedule a, TickSchedule b) => a.Equals(b);
        public static bool operator !=(TickSchedule a, TickSchedule b) => !a.Equals(b);
    }

    /// <summary>
    /// Contexto passado a cada sistema no tick. Só tempo de simulação (override O3) — nunca tempo real.
    /// </summary>
    public readonly struct SimulationContext
    {
        public readonly long Tick;
        public readonly SimulationClock Clock;
        public readonly ulong WorldSeed;
        public readonly DomainEventBuffer Events;

        public SimulationContext(long tick, SimulationClock clock, ulong worldSeed, DomainEventBuffer events)
        {
            Tick = tick;
            Clock = clock;
            WorldSeed = worldSeed;
            Events = events;
        }

        /// <summary>Fluxo de RNG independente para este sistema neste tick.</summary>
        public Pcg32 Rng(int systemId) => Pcg32.Stream(WorldSeed, systemId, Tick);
    }

    /// <summary>
    /// Um sistema de simulação. Não lê tempo real, não cria threads, não altera o estado de outro contexto.
    /// </summary>
    public interface ISimulationSystem
    {
        /// <summary>Identificador estável do sistema (usado no fluxo de RNG e no hash).</summary>
        int SystemId { get; }

        TickSchedule Schedule { get; }

        void Execute(CityState state, in SimulationContext context);
    }
}
