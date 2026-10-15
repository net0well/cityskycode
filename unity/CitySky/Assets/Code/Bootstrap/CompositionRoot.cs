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
        private readonly ISaveStore? _saveStore;

        public CompositionRoot(ulong worldSeed, ILog log, ISaveStore? saveStore = null)
        {
            _log = log;
            _saveStore = saveStore;
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

        /// <summary>Retrato do estado atual para save (somente leitura). F1-101.</summary>
        public CitySnapshot CaptureSnapshot() => SnapshotService.Capture(Simulation.State);

        /// <summary>
        /// Grava o estado atual no slot informado; retorna <c>false</c> se nenhuma store foi configurada.
        /// Carregar um save de volta para a simulação em execução é trabalho de fase futura (precisa de um
        /// caminho de restauração no núcleo da simulação; o round-trip de dados já está provado em testes).
        /// </summary>
        public bool Save(string slot)
        {
            if (_saveStore is null)
            {
                return false;
            }

            _saveStore.Save(slot, CaptureSnapshot());
            return true;
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
