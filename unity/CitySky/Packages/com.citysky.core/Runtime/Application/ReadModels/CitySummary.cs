using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Read model do cabeçalho: data, hora e velocidade efetiva, com *dirty tracking* — só sinaliza
    /// mudança quando algo exibido muda (F1-41). A apresentação republica a view só quando
    /// <see cref="Update"/> devolve <c>true</c>.
    /// </summary>
    public sealed class CitySummary
    {
        public long DayOfGame { get; private set; }
        public int HourOfDay { get; private set; }
        public int MinuteOfHour { get; private set; }
        public GameSpeed Speed { get; private set; }
        public bool SpeedLimited { get; private set; }

        /// <summary>Versão incrementada a cada mudança (para a apresentação detectar republicação).</summary>
        public int Version { get; private set; }

        /// <summary>Recalcula a partir do relógio e da velocidade. Devolve <c>true</c> se algo exibido mudou.</summary>
        public bool Update(SimulationClock clock, GameSpeed speed, bool speedLimited)
        {
            bool changed =
                DayOfGame != clock.DayOfGame
                || HourOfDay != clock.HourOfDay
                || MinuteOfHour != clock.MinuteOfHour
                || Speed != speed
                || SpeedLimited != speedLimited;

            if (!changed)
            {
                return false;
            }

            DayOfGame = clock.DayOfGame;
            HourOfDay = clock.HourOfDay;
            MinuteOfHour = clock.MinuteOfHour;
            Speed = speed;
            SpeedLimited = speedLimited;
            Version++;
            return true;
        }
    }
}
