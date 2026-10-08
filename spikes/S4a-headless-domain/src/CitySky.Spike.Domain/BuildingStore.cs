namespace CitySky.Spike.Domain
{
    /// <summary>Prédios: só o que o spike precisa (energia e contribuições agregadas para o "Por quê?").</summary>
    public sealed class BuildingStore
    {
        public readonly int Count;
        public readonly bool[] Powered;

        /// <summary>Soma das contribuições dos moradores, por fator, em [id * FactorCount + fator].</summary>
        public readonly int[] Contribution;

        public BuildingStore(int count)
        {
            Count = count;
            Powered = new bool[count + 1];
            Contribution = new int[(count + 1) * CitizenStore.FactorCount];
        }
    }
}
