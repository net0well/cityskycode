namespace CitySky.Spike.Domain
{
    /// <summary>
    /// Necessidades do MVP (HAPPINESS_SYSTEM.md §2): cada cidadão 1x por hora TC (balde de 480), fase 0.
    /// </summary>
    public static class NeedsSystem
    {
        private const int UnemployedFloor = 2000;
        private const int UnemployedDecayPerHour = 33;   // ~6000 → 2000 em 5 dias
        private const int EnergyStepPerHour = 417;       // ~fração das últimas 24 h abastecida
        private const int ConsumptionDecayPerHour = 208; // estoque do domicílio acaba em ~2 dias

        public static void Execute(CityState state, long tick)
        {
            CitizenStore c = state.Citizens;
            bool[] powered = state.Buildings.Powered;
            short[] needs = c.Needs;
            int first = (int)(tick % SimTime.HourBucket) + 1;

            for (int id = first; id <= c.Count; id += SimTime.HourBucket)
            {
                int b = id * CitizenStore.FactorCount;
                int home = c.Home[id];

                needs[b + CitizenStore.Housing] = (short)(home != 0 ? 10000 : 0);

                if (c.Workplace[id] != 0)
                {
                    needs[b + CitizenStore.Employment] = 10000;
                }
                else
                {
                    int employment = needs[b + CitizenStore.Employment] - UnemployedDecayPerHour;
                    needs[b + CitizenStore.Employment] = (short)(employment < UnemployedFloor ? UnemployedFloor : employment);
                }

                int energy = needs[b + CitizenStore.Energy];
                energy = powered[home] ? energy + EnergyStepPerHour : energy - EnergyStepPerHour;
                needs[b + CitizenStore.Energy] = (short)(energy > 10000 ? 10000 : energy < 0 ? 0 : energy);

                int consumption = needs[b + CitizenStore.Consumption] - ConsumptionDecayPerHour;
                needs[b + CitizenStore.Consumption] = (short)(consumption < 0 ? 0 : consumption);
            }
        }
    }
}
