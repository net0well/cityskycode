namespace CitySky.Spike.Domain
{
    public sealed class CityState
    {
        public readonly CitizenStore Citizens;
        public readonly BuildingStore Buildings;
        public readonly TripStore Trips;

        /// <summary>Alíquota residencial em décimos de ponto percentual (100 = 10%).</summary>
        public int ResidentialTaxPermille;

        public CityState(int citizens, int buildings)
        {
            Citizens = new CitizenStore(citizens);
            Buildings = new BuildingStore(buildings);
            Trips = new TripStore(citizens);
            ResidentialTaxPermille = 100;
        }
    }
}
