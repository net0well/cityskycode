namespace CitySky.Application
{
    /// <summary>
    /// Acumula o "tempo devido" em ticks e decide quantos rodar por frame (SIMULATION_TICK §4).
    /// Puro (sem relógio real) e determinístico quanto ao resultado: nunca pula ticks — se a máquina
    /// não acompanha, o backlog fica para os próximos frames (a velocidade efetiva cai, não o resultado).
    /// </summary>
    public sealed class TickAccumulator
    {
        /// <summary>Teto de ticks por frame (evita travar o frame).</summary>
        public const int MaxTicksPerFrame = 12;

        /// <summary>Teto de backlog, para não acumular após uma pausa do SO (spiral of death).</summary>
        public const double MaxBacklogTicks = 60.0;

        /// <summary>Limite do delta real por frame (SIMULATION_TICK §4).</summary>
        public const double MaxDeltaSeconds = 0.1;

        private double _due;

        /// <summary>Fração do próximo tick, para interpolação visual (0..1).</summary>
        public double Alpha { get; private set; }

        /// <summary>Backlog atual, em ticks.</summary>
        public double Due => _due;

        /// <summary>Adiciona o tempo do frame (clampado) na velocidade dada.</summary>
        public void Accumulate(double deltaSeconds, GameSpeed speed)
        {
            if (deltaSeconds < 0)
            {
                deltaSeconds = 0;
            }
            else if (deltaSeconds > MaxDeltaSeconds)
            {
                deltaSeconds = MaxDeltaSeconds;
            }

            _due += deltaSeconds * speed.TicksPerSecond;
        }

        /// <summary>
        /// Consome um tick se houver backlog e o teto por frame não foi atingido. Devolve <c>true</c>
        /// quando o chamador deve rodar um tick.
        /// </summary>
        public bool TryConsume(int ranThisFrame)
        {
            if (_due >= 1.0 && ranThisFrame < MaxTicksPerFrame)
            {
                _due -= 1.0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fecha o frame: limita o backlog e atualiza o alpha de interpolação. Devolve <c>true</c> se o
        /// backlog foi cortado (a velocidade efetiva está limitada pela máquina).
        /// </summary>
        public bool CloseFrame()
        {
            bool limited = false;
            if (_due > MaxBacklogTicks)
            {
                _due = MaxBacklogTicks;
                limited = true;
            }

            Alpha = _due >= 1.0 ? 1.0 : _due; // 0..1 para interpolação
            return limited;
        }
    }
}
