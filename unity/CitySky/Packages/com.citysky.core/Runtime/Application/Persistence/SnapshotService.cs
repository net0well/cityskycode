using CitySky.Domain;

namespace CitySky.Application
{
    /// <summary>
    /// Captura e restaura o <see cref="CityState"/> como <see cref="CitySnapshot"/> (F1-101). A captura
    /// grava o hash determinístico; a restauração reconstrói o estado e <b>confere o hash</b> — se não
    /// bater (ou o esquema for incompatível) lança <see cref="SnapshotException"/>. Pura, sem I/O.
    /// </summary>
    public static class SnapshotService
    {
        public static CitySnapshot Capture(CityState state)
        {
            if (state == null)
            {
                throw new System.ArgumentNullException(nameof(state));
            }

            return new CitySnapshot(
                CitySnapshot.CurrentSchemaVersion,
                state.WorldSeed,
                state.Clock.Tick,
                state.Heartbeat,
                state.DayCounter,
                state.ComputeHash());
        }

        public static CityState Restore(CitySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new System.ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.SchemaVersion != CitySnapshot.CurrentSchemaVersion)
            {
                throw new SnapshotException(
                    "Versão de save incompatível: " + snapshot.SchemaVersion + " (esperado " + CitySnapshot.CurrentSchemaVersion + ").");
            }

            var state = new CityState(snapshot.WorldSeed)
            {
                Clock = new SimulationClock(snapshot.Tick),
                Heartbeat = snapshot.Heartbeat,
                DayCounter = snapshot.DayCounter,
            };

            ulong actual = state.ComputeHash();
            if (actual != snapshot.Hash)
            {
                throw new SnapshotException(
                    "Save corrompido: hash " + actual + " não confere com o gravado " + snapshot.Hash + ".");
            }

            return state;
        }
    }
}
