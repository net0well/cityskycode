using System;

namespace CitySky.Application
{
    /// <summary>
    /// Velocidade do jogo (value object). Mapeia para ticks por segundo real: Pausa/1x/2x/3x/5x/10x =
    /// 0/4/8/12/20/40 (SIMULATION_TICK §5). Em 1x, 4 ticks/s (1 hora TC = 2 min reais).
    /// </summary>
    public readonly struct GameSpeed : IEquatable<GameSpeed>
    {
        public readonly int TicksPerSecond;

        private GameSpeed(int ticksPerSecond)
        {
            TicksPerSecond = ticksPerSecond;
        }

        public static readonly GameSpeed Pause = new GameSpeed(0);
        public static readonly GameSpeed Normal = new GameSpeed(4);    // 1x
        public static readonly GameSpeed Fast = new GameSpeed(8);      // 2x
        public static readonly GameSpeed Faster = new GameSpeed(12);   // 3x
        public static readonly GameSpeed VeryFast = new GameSpeed(20); // 5x
        public static readonly GameSpeed Fastest = new GameSpeed(40);  // 10x

        public bool IsPaused => TicksPerSecond == 0;

        /// <summary>Multiplicador relativo ao 1x (ex.: 10x), para a UI.</summary>
        public int Multiplier => TicksPerSecond / 4;

        public bool Equals(GameSpeed other) => TicksPerSecond == other.TicksPerSecond;
        public override bool Equals(object? obj) => obj is GameSpeed o && Equals(o);
        public override int GetHashCode() => TicksPerSecond;
        public static bool operator ==(GameSpeed a, GameSpeed b) => a.TicksPerSecond == b.TicksPerSecond;
        public static bool operator !=(GameSpeed a, GameSpeed b) => a.TicksPerSecond != b.TicksPerSecond;
        public override string ToString() => IsPaused ? "Pausa" : Multiplier + "x";
    }
}
