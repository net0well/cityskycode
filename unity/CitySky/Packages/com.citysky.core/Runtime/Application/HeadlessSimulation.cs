using System.Collections.Generic;
using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Simulação sem Unity (Domain + Application): usada em testes rápidos, nos Laboratórios e no
    /// canário de determinismo .NET/Mono. Monta a lista padrão de sistemas do esqueleto. F1-38.
    /// </summary>
    public sealed class HeadlessSimulation
    {
        private readonly Simulation _simulation;

        public HeadlessSimulation(ulong worldSeed)
            : this(worldSeed, DefaultSystems())
        {
        }

        public HeadlessSimulation(ulong worldSeed, IEnumerable<ISimulationSystem> systems)
        {
            _simulation = new Simulation(worldSeed, systems);
        }

        public CityState State => _simulation.State;

        public DomainEventBuffer Events => _simulation.Events;

        public CommandQueue Commands => _simulation.Commands;

        public long CurrentTick => _simulation.CurrentTick;

        public void RunTicks(long count) => _simulation.RunTicks(count);

        public ulong Hash() => _simulation.State.ComputeHash();

        /// <summary>Sistemas do esqueleto (ordem determinística).</summary>
        public static IReadOnlyList<ISimulationSystem> DefaultSystems() => new ISimulationSystem[]
        {
            new HeartbeatSystem(),
            new CalendarSystem(),
        };
    }
}
