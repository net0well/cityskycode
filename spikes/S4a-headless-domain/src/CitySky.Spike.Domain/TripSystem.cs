namespace CitySky.Spike.Domain
{
    /// <summary>TripProgressSystem: avança todas as viagens ativas e processa chegadas, todo tick.</summary>
    public static class TripSystem
    {
        private const int ShoppingDurationTicks = SimTime.TicksPerHour;
        private const int PurchaseAmount = 6000;

        public static void Execute(CityState state, long tick)
        {
            TripStore trips = state.Trips;
            CitizenStore c = state.Citizens;

            // De trás para frente: o swap-remove traz para o slot atual uma viagem já processada neste tick.
            for (int slot = trips.Count - 1; slot >= 0; slot--)
            {
                if (trips.EndTickAt(slot) > tick)
                {
                    trips.Advance(slot);
                    continue;
                }

                int id = trips.CitizenAt(slot);
                Arrive(c, id, tick);
                c.TripSlot[id] = -1;

                int moved = trips.RemoveAt(slot);
                if (moved != 0)
                {
                    c.TripSlot[moved] = slot;
                }
            }
        }

        private static void Arrive(CitizenStore c, int id, long tick)
        {
            switch ((Activity)c.Activity[id])
            {
                case Activity.TravelingToWork:
                    c.Activity[id] = (byte)Activity.Working;
                    break;

                case Activity.TravelingToShop:
                    c.Activity[id] = (byte)Activity.Shopping;
                    c.ActivityEndTick[id] = tick + ShoppingDurationTicks;
                    int index = id * CitizenStore.FactorCount + CitizenStore.Consumption;
                    int stock = c.Needs[index] + PurchaseAmount;
                    c.Needs[index] = (short)(stock > 10000 ? 10000 : stock);
                    break;

                default:
                    c.Activity[id] = (byte)Activity.AtHome;
                    break;
            }
        }
    }
}
