namespace CitySky.Domain
{
    /// <summary>
    /// Estado autoritativo da cidade. Esqueleto da Fase 1: relógio + contadores (F1-36).
    /// Todos os campos são inteiros/ponto fixo (determinismo). Só sistemas e comandos o mutam.
    /// </summary>
    public sealed class CityState
    {
        public CityState(ulong worldSeed)
        {
            WorldSeed = worldSeed;
            Clock = SimulationClock.Start;
        }

        /// <summary>Semente do mundo (fixa na criação).</summary>
        public ulong WorldSeed { get; }

        /// <summary>Relógio atual (avança a cada tick).</summary>
        public SimulationClock Clock { get; set; }

        /// <summary>Contador de pulsos do <see cref="HeartbeatSystem"/> (para testes de determinismo).</summary>
        public long Heartbeat { get; set; }

        /// <summary>Dias completos contados pelo <see cref="CalendarSystem"/>.</summary>
        public long DayCounter { get; set; }

        /// <summary>Hash determinístico do estado atual.</summary>
        public ulong ComputeHash() => StateHasher.Hash(this);
    }
}
