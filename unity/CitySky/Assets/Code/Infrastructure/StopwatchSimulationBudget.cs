using System.Diagnostics;
using CitySky.Application;

namespace CitySky.Infrastructure
{
    /// <summary>
    /// Orçamento de tempo real por frame (SIMULATION_TICK §4): roda ticks até estourar ~6 ms. Usa
    /// <c>Stopwatch</c> — permitido fora do domínio (override O3). F1-40.
    /// </summary>
    public sealed class StopwatchSimulationBudget : ISimulationBudget
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly long _budgetMs;

        public StopwatchSimulationBudget(long budgetMs = 6)
        {
            _budgetMs = budgetMs;
        }

        public void BeginFrame() => _stopwatch.Restart();

        public bool HasTimeLeft() => _stopwatch.ElapsedMilliseconds < _budgetMs;
    }
}
