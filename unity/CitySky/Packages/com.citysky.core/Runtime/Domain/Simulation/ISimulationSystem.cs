namespace CitySky.Domain
{
    /// <summary>
    /// Um sistema de simulação (padrão Strategy: cada sistema é uma estratégia agendada). Não lê tempo
    /// real, não cria threads, não altera o estado de outro contexto.
    /// </summary>
    public interface ISimulationSystem
    {
        /// <summary>Identificador estável do sistema (usado no fluxo de RNG e no hash).</summary>
        int SystemId { get; }

        TickSchedule Schedule { get; }

        void Execute(CityState state, in SimulationContext context);
    }
}
