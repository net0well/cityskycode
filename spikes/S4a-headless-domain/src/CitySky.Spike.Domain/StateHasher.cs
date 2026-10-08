namespace CitySky.Spike.Domain
{
    /// <summary>
    /// Hash do estado em ordem de id (FNV-1a 64). O produto usará xxHash64 (SIMULATION_ARCHITECTURE.md §8);
    /// para o spike basta um hash determinístico e sem dependências.
    /// </summary>
    public static class StateHasher
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong Compute(CityState state)
        {
            ulong hash = OffsetBasis;
            CitizenStore c = state.Citizens;
            TripStore trips = state.Trips;

            for (int id = 1; id <= c.Count; id++)
            {
                hash = Mix(hash, c.Activity[id]);
                hash = Mix(hash, c.Happiness[id]);
                hash = Mix(hash, c.LastWorkDay[id]);
                int slot = c.TripSlot[id];
                hash = Mix(hash, slot >= 0 ? trips.ProgressAt(slot) : -1);

                int b = id * CitizenStore.FactorCount;
                for (int f = 0; f < CitizenStore.FactorCount; f++)
                {
                    hash = Mix(hash, c.Needs[b + f]);
                }
            }

            int[] contribution = state.Buildings.Contribution;
            for (int i = 0; i < contribution.Length; i++)
            {
                hash = Mix(hash, contribution[i]);
            }

            return Mix(hash, trips.Count);
        }

        private static ulong Mix(ulong hash, int value)
        {
            unchecked
            {
                hash = (hash ^ (byte)value) * Prime;
                hash = (hash ^ (byte)(value >> 8)) * Prime;
                hash = (hash ^ (byte)(value >> 16)) * Prime;
                hash = (hash ^ (byte)(value >> 24)) * Prime;
                return hash;
            }
        }
    }
}
