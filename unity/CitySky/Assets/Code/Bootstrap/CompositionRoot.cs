using System.Globalization;
using CitySky.Application;
using CitySky.Domain;

namespace CitySky.Bootstrap
{
    /// <summary>
    /// Raiz de composição (Pure DI): monta o grafo do esqueleto — simulação determinística, runner,
    /// barramento de comandos e read model — e liga os eventos de domínio ao log. Sem singletons nem
    /// <c>static</c> mutável (F1-43 / ARCHITECTURE §7).
    /// </summary>
    public sealed class CompositionRoot
    {
        private readonly ILog _log;

        public CompositionRoot(ulong worldSeed, ILog log)
        {
            _log = log;
            Simulation = new Simulation(worldSeed, HeadlessSimulation.DefaultSystems());
            Runner = new SimulationRunner(Simulation);
            CommandBus = new CommandBus(Simulation);
            Summary = new CitySummary();
            Simulation.Events.Subscribe(OnDomainEvent);
        }

        public Simulation Simulation { get; }

        public SimulationRunner Runner { get; }

        public CommandBus CommandBus { get; }

        public CitySummary Summary { get; }

        /// <summary>Avança a simulação pelo tempo do frame e atualiza o read model.</summary>
        public void Advance(double deltaSeconds, GameSpeed speed, ISimulationBudget budget)
        {
            Runner.Advance(deltaSeconds, speed, budget);
            Summary.Update(Simulation.State.Clock, speed, Runner.EffectiveSpeedLimited);
        }

        private void OnDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent is DayStartedEvent day)
            {
                _log.Info("Novo dia: " + day.DayOfGame.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
