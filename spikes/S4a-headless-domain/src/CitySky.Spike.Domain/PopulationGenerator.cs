namespace CitySky.Spike.Domain
{
    /// <summary>
    /// Gera uma cidade sintética determinística a partir de uma seed.
    /// Domicílios de 1–4 pessoas, 1 casa por domicílio, 1 local de trabalho para cada 10 cidadãos,
    /// 90% empregados, turno de 8 h começando entre 07:00 e 09:00, 3% das casas sem energia.
    /// Tempos de caminho vêm de uma tabela falsa (30–150 min TC até o trabalho; 10–60 min até o mercado):
    /// com a escala temporal do ADR-0008, 1 km a pé leva ~2 h TC, então viagens longas são realistas.
    /// </summary>
    public static class PopulationGenerator
    {
        public static Simulation Create(int citizens, ulong seed)
        {
            var rng = new Pcg32(seed, 0x5EED);

            // 1ª passada: tamanhos dos domicílios (determina o número de casas).
            var householdSizes = new int[citizens];
            int households = 0;
            int assigned = 0;
            while (assigned < citizens)
            {
                int size = rng.NextInt(1, 5);
                if (assigned + size > citizens)
                {
                    size = citizens - assigned;
                }

                householdSizes[households++] = size;
                assigned += size;
            }

            int homes = households;
            int workplaces = citizens / 10 > 0 ? citizens / 10 : 1;
            var state = new CityState(citizens, homes + workplaces);
            CitizenStore c = state.Citizens;
            BuildingStore buildings = state.Buildings;

            for (int home = 1; home <= homes; home++)
            {
                buildings.Powered[home] = rng.NextInt(0, 100) >= 3;
            }

            for (int work = homes + 1; work <= homes + workplaces; work++)
            {
                buildings.Powered[work] = true;
            }

            // 2ª passada: cidadãos.
            int id = 0;
            for (int household = 1; household <= households; household++)
            {
                for (int member = 0; member < householdSizes[household - 1]; member++)
                {
                    id++;
                    c.Household[id] = household;
                    c.Home[id] = household;

                    bool employed = rng.NextInt(0, 100) < 90;
                    c.Workplace[id] = employed ? homes + 1 + rng.NextInt(0, workplaces) : 0;

                    int shiftStart = 8 * 60 + rng.NextInt(-60, 61);
                    c.ShiftStartMinute[id] = (short)shiftStart;
                    c.ShiftEndMinute[id] = (short)(shiftStart + 8 * 60);
                    c.CommuteTicks[id] = rng.NextInt(30, 151) * SimTime.TicksPerMinute;
                    c.ShopTicks[id] = rng.NextInt(10, 61) * SimTime.TicksPerMinute;
                    c.Activity[id] = (byte)Activity.Sleeping;

                    int b = id * CitizenStore.FactorCount;
                    c.Needs[b + CitizenStore.Housing] = 10000;
                    c.Needs[b + CitizenStore.Employment] = (short)(employed ? 10000 : 6000);
                    c.Needs[b + CitizenStore.Energy] = 10000;
                    c.Needs[b + CitizenStore.Consumption] = (short)rng.NextInt(3000, 10001);
                    c.Happiness[id] = 7000;
                }
            }

            return new Simulation(state);
        }
    }
}
