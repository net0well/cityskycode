namespace CitySky.Application
{
    /// <summary>
    /// Orçamento de tempo real por frame para o runner (port — Strategy de plataforma). A implementação
    /// da Unity usa um <c>Stopwatch</c> com alvo de 6 ms (SIMULATION_TICK §4); nos testes/headless, sem limite.
    /// </summary>
    public interface ISimulationBudget
    {
        /// <summary>Marca o início do frame (zera o relógio do orçamento).</summary>
        void BeginFrame();

        /// <summary>Ainda há tempo neste frame para rodar mais um tick?</summary>
        bool HasTimeLeft();
    }
}
