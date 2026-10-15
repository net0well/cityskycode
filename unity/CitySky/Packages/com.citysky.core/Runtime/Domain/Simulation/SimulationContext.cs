namespace CitySky.Domain
{
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
}
