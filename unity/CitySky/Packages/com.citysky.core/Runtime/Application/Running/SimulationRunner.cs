using System;
using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Avança a simulação determinística a partir do tempo real de cada frame, respeitando a velocidade,
    /// o teto de ticks por frame e o orçamento de tempo (SIMULATION_TICK §4). Nunca pula ticks: o backlog
    /// que não coube fica para os próximos frames; a velocidade efetiva é que cai.
    /// </summary>
    public sealed class SimulationRunner
    {
        private readonly Simulation _simulation;
        private readonly TickAccumulator _accumulator;

        public SimulationRunner(Simulation simulation, TickAccumulator? accumulator = null)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _accumulator = accumulator ?? new TickAccumulator();
        }

        /// <summary>Fração do próximo tick (0..1) para interpolação visual.</summary>
        public double Alpha => _accumulator.Alpha;

        /// <summary>A máquina não acompanhou a velocidade pedida neste frame?</summary>
        public bool EffectiveSpeedLimited { get; private set; }

        /// <summary>Ticks rodados no último <see cref="Advance"/>.</summary>
        public int LastTicksRun { get; private set; }

        /// <summary>Tick atual da simulação.</summary>
        public long CurrentTick => _simulation.CurrentTick;

        /// <summary>Avança a simulação pelo tempo do frame, na velocidade dada e dentro do orçamento.</summary>
        public void Advance(double deltaSeconds, GameSpeed speed, ISimulationBudget budget)
        {
            if (budget == null)
            {
                throw new ArgumentNullException(nameof(budget));
            }

            _accumulator.Accumulate(deltaSeconds, speed);
            budget.BeginFrame();

            int ran = 0;
            while (budget.HasTimeLeft())
            {
                if (!_accumulator.TryConsume(ran))
                {
                    break; // sem backlog ou teto de ticks atingido
                }

                _simulation.RunTick();
                ran++;
            }

            EffectiveSpeedLimited = _accumulator.CloseFrame();
            LastTicksRun = ran;
        }
    }
}
