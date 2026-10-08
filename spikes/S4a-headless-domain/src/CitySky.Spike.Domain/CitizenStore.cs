namespace CitySky.Spike.Domain
{
    /// <summary>
    /// Store SoA de cidadãos (DOMAIN_MODEL.md §4.1). Índice = id, começando em 1 (0 = inválido).
    /// Simplificação de spike: arrays públicos em vez de métodos com escrita restrita ao contexto.
    /// </summary>
    public sealed class CitizenStore
    {
        public const int FactorCount = 4;
        public const int Housing = 0;
        public const int Employment = 1;
        public const int Energy = 2;
        public const int Consumption = 3;

        public readonly int Count;

        public readonly int[] Household;
        public readonly int[] Home;
        public readonly int[] Workplace;           // 0 = desempregado
        public readonly byte[] Activity;
        public readonly short[] ShiftStartMinute;
        public readonly short[] ShiftEndMinute;
        public readonly int[] CommuteTicks;        // custo de caminho casa ↔ trabalho (tabela falsa, sem pathfinding)
        public readonly int[] ShopTicks;           // custo de caminho casa ↔ mercado
        public readonly int[] LastWorkDay;
        public readonly long[] ActivityEndTick;
        public readonly int[] TripSlot;            // posição no TripStore; -1 = sem viagem

        /// <summary>Satisfação por necessidade, 0–10000, em [id * FactorCount + fator].</summary>
        public readonly short[] Needs;

        /// <summary>Felicidade suavizada, 0–10000 (centésimos de ponto).</summary>
        public readonly short[] Happiness;

        /// <summary>Última contribuição de cada fator (centésimos, ≤ 0), para manter o agregado por prédio de forma incremental.</summary>
        public readonly short[] Contribution;

        public CitizenStore(int count)
        {
            Count = count;
            int size = count + 1;
            Household = new int[size];
            Home = new int[size];
            Workplace = new int[size];
            Activity = new byte[size];
            ShiftStartMinute = new short[size];
            ShiftEndMinute = new short[size];
            CommuteTicks = new int[size];
            ShopTicks = new int[size];
            LastWorkDay = new int[size];
            ActivityEndTick = new long[size];
            TripSlot = new int[size];
            Needs = new short[size * FactorCount];
            Happiness = new short[size];
            Contribution = new short[size * FactorCount];

            for (int i = 0; i < size; i++)
            {
                TripSlot[i] = -1;
                LastWorkDay[i] = -1;
            }
        }
    }
}
