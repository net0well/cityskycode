using System;
using System.Collections.Generic;

namespace CitySky.Domain
{
    /// <summary>
    /// Executa a simulação determinística, um tick por vez (SIMULATION_ARCHITECTURE §3):
    /// aplica comandos → roda sistemas no período → despacha eventos. F1-33.
    /// </summary>
    public sealed class Simulation
    {
        private readonly List<ISimulationSystem> _systems;

        public Simulation(ulong worldSeed, IEnumerable<ISimulationSystem> systems)
        {
            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            WorldSeed = worldSeed;
            State = new CityState(worldSeed);
            Commands = new CommandQueue();
            Events = new DomainEventBuffer();
            _systems = new List<ISimulationSystem>(systems);
        }

        public ulong WorldSeed { get; }

        public CityState State { get; }

        public CommandQueue Commands { get; }

        public DomainEventBuffer Events { get; }

        /// <summary>Próximo tick a ser executado (começa em 0).</summary>
        public long CurrentTick { get; private set; }

        /// <summary>Executa um tick completo.</summary>
        public void RunTick()
        {
            long tick = CurrentTick;
            var clock = new SimulationClock(tick);
            State.Clock = clock;
            var context = new SimulationContext(tick, clock, WorldSeed, Events);

            // 1. Comandos agendados para este tick, em ordem (targetTick, seq).
            List<ICityCommand> due = Commands.DrainDue(tick);
            for (int i = 0; i < due.Count; i++)
            {
                due[i].Apply(State, context);
            }

            // 2. Sistemas no seu período/fase.
            for (int i = 0; i < _systems.Count; i++)
            {
                ISimulationSystem system = _systems[i];
                if (system.Schedule.IsDue(tick))
                {
                    system.Execute(State, context);
                }
            }

            // 3. Eventos de domínio, em ordem, após o tick.
            Events.Flush();

            CurrentTick = tick + 1;
        }

        /// <summary>Executa <paramref name="count"/> ticks.</summary>
        public void RunTicks(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (long i = 0; i < count; i++)
            {
                RunTick();
            }
        }
    }
}
