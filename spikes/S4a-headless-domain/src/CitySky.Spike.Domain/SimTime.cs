namespace CitySky.Spike.Domain
{
    /// <summary>Conversões de tempo do ADR-0008: 1 tick = 0,25 s TM = 7,5 s TC.</summary>
    public static class SimTime
    {
        public const int TicksPerMinute = 8;
        public const int TicksPerHour = 480;
        public const int TicksPerDay = 11520;

        /// <summary>Período dos baldes da agenda (15 min TC).</summary>
        public const int ScheduleBucket = 120;

        /// <summary>Período dos baldes de necessidades e felicidade (1 h TC).</summary>
        public const int HourBucket = 480;

        public static int MinuteOfDay(long tick)
        {
            return (int)(tick % TicksPerDay) / TicksPerMinute;
        }

        public static int Day(long tick)
        {
            return (int)(tick / TicksPerDay);
        }
    }
}
