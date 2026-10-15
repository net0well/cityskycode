using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>
    /// Relógio da simulação: tudo é função do número do tick (SIMULATION_TICK §2). F1-31.
    /// 1 tick = 0,25 s TM = 7,5 s TC. 8 ticks = 1 min TC; 480 = 1 h; 11 520 = 1 dia.
    /// Sem <c>DateTime</c>/tempo real (override O3).
    /// </summary>
    public readonly struct SimulationClock : IEquatable<SimulationClock>
    {
        public const int TicksPerMinute = 8;
        public const int TicksPerHour = 480;
        public const int TicksPerDay = 11_520;
        public const int TmMillisecondsPerTick = 250;   // 0,25 s TM
        public const int TcMillisecondsPerTick = 7_500; // 7,5 s TC

        public readonly long Tick;

        public SimulationClock(long tick)
        {
            if (tick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), "O tick não pode ser negativo.");
            }

            Tick = tick;
        }

        public static readonly SimulationClock Start = default;

        public SimulationClock Advance(long ticks) => new SimulationClock(checked(Tick + ticks));

        /// <summary>Dia de jogo começando em 1 (tick 0 = dia 1).</summary>
        public long DayOfGame => Tick / TicksPerDay + 1;

        public int HourOfDay => (int)(Tick / TicksPerHour % 24);

        public int MinuteOfHour => (int)(Tick / TicksPerMinute % 60);

        /// <summary>Tick dentro do dia atual (0..11519).</summary>
        public int TickOfDay => (int)(Tick % TicksPerDay);

        public long TmMilliseconds => checked(Tick * TmMillisecondsPerTick);

        public long TcMilliseconds => checked(Tick * TcMillisecondsPerTick);

        public DayPhase Phase
        {
            get
            {
                int h = HourOfDay;
                if (h >= 5 && h < 8) { return DayPhase.Dawn; }
                if (h >= 8 && h < 12) { return DayPhase.Morning; }
                if (h >= 12 && h < 18) { return DayPhase.Afternoon; }
                if (h >= 18 && h < 21) { return DayPhase.Evening; }
                return DayPhase.Night;
            }
        }

        public bool Equals(SimulationClock other) => Tick == other.Tick;
        public override bool Equals(object? obj) => obj is SimulationClock o && Equals(o);
        public override int GetHashCode() => Tick.GetHashCode();
        public static bool operator ==(SimulationClock a, SimulationClock b) => a.Tick == b.Tick;
        public static bool operator !=(SimulationClock a, SimulationClock b) => a.Tick != b.Tick;

        public override string ToString()
        {
            return "Dia " + DayOfGame.ToString(CultureInfo.InvariantCulture)
                 + " " + HourOfDay.ToString("D2", CultureInfo.InvariantCulture)
                 + ":" + MinuteOfHour.ToString("D2", CultureInfo.InvariantCulture)
                 + " (" + Phase + ")";
        }
    }
}
