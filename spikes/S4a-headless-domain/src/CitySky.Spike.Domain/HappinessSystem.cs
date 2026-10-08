namespace CitySky.Spike.Domain
{
    /// <summary>
    /// Felicidade (HAPPINESS_SYSTEM.md §3) em aritmética inteira: média ponderada (3/3/2/2) com 30% do pior fator,
    /// penalidade de imposto, suavização de 0,25 por hora e contribuições agregadas por prédio.
    /// Cada cidadão 1x por hora TC (balde de 480), com fase deslocada de 240 ticks em relação às necessidades.
    /// </summary>
    public static class HappinessSystem
    {
        public const int Phase = 240;

        private static readonly int[] Weights = { 3, 3, 2, 2 };
        private const int WeightSum = 10;

        public static void Execute(CityState state, long tick)
        {
            CitizenStore c = state.Citizens;
            short[] needs = c.Needs;
            short[] contribution = c.Contribution;
            int[] buildingContribution = state.Buildings.Contribution;
            int taxPenalty = TaxPenalty(state.ResidentialTaxPermille);
            int first = (int)((tick + SimTime.HourBucket - Phase) % SimTime.HourBucket) + 1;

            for (int id = first; id <= c.Count; id += SimTime.HourBucket)
            {
                int b = id * CitizenStore.FactorCount;

                int weighted = 0;
                int worst = 10000;
                int worstFactor = 0;
                for (int f = 0; f < CitizenStore.FactorCount; f++)
                {
                    int s = needs[b + f];
                    weighted += Weights[f] * s;
                    if (s < worst)
                    {
                        worst = s;
                        worstFactor = f;
                    }
                }

                int baseValue = weighted / WeightSum;
                int raw = (7 * baseValue + 3 * worst) / 10;
                int target = raw - taxPenalty;
                target = target < 0 ? 0 : target > 10000 ? 10000 : target;

                int current = c.Happiness[id];
                current += (target - current) / 4;
                c.Happiness[id] = (short)current;

                // Explicabilidade: quanto cada fator puxa a felicidade para baixo (soma = raw − 10000, ± arredondamento).
                int home = c.Home[id];
                for (int f = 0; f < CitizenStore.FactorCount; f++)
                {
                    int delta = 7 * Weights[f] * (needs[b + f] - 10000) / 100;
                    if (f == worstFactor)
                    {
                        delta += 3 * (worst - 10000) / 10;
                    }

                    int old = contribution[b + f];
                    if (delta != old)
                    {
                        contribution[b + f] = (short)delta;
                        buildingContribution[home * CitizenStore.FactorCount + f] += delta - old;
                    }
                }
            }
        }

        /// <summary>0 até 8%; linear até 10 pontos em 12%; até 30 pontos em 20% (escala 0–10000).</summary>
        public static int TaxPenalty(int permille)
        {
            if (permille <= 80)
            {
                return 0;
            }

            if (permille <= 120)
            {
                return (permille - 80) * 1000 / 40;
            }

            int capped = permille > 200 ? 200 : permille;
            return 1000 + (capped - 120) * 2000 / 80;
        }
    }
}
