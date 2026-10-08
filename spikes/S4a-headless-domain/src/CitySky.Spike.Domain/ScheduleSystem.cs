namespace CitySky.Spike.Domain
{
    /// <summary>
    /// Agenda (ScheduleSystem): cada cidadão decide a próxima atividade 1x a cada 15 min TC
    /// (balde de 120 ticks). Viagens em andamento são tratadas pelo <see cref="TripSystem"/>.
    /// </summary>
    public static class ScheduleSystem
    {
        private const int WakeMinute = 6 * 60;
        private const int SleepMinute = 22 * 60;
        private const int DepartureSlackMinutes = 10;
        private const int UnemployedShopFromMinute = 15 * 60;
        private const int UnemployedShopToMinute = 19 * 60;
        private const short ShopThreshold = 5000;

        public static void Execute(CityState state, long tick)
        {
            CitizenStore c = state.Citizens;
            int minute = SimTime.MinuteOfDay(tick);
            int day = SimTime.Day(tick);
            int first = (int)(tick % SimTime.ScheduleBucket) + 1;

            for (int id = first; id <= c.Count; id += SimTime.ScheduleBucket)
            {
                switch ((Activity)c.Activity[id])
                {
                    case Activity.Sleeping:
                        if (minute >= WakeMinute && minute < SleepMinute)
                        {
                            c.Activity[id] = (byte)Activity.AtHome;
                        }
                        break;

                    case Activity.AtHome:
                        DecideAtHome(state, id, minute, day, tick);
                        break;

                    case Activity.Working:
                        if (minute >= c.ShiftEndMinute[id])
                        {
                            if (c.Needs[id * CitizenStore.FactorCount + CitizenStore.Consumption] < ShopThreshold)
                            {
                                StartTrip(state, id, c.ShopTicks[id], tick, Activity.TravelingToShop);
                            }
                            else
                            {
                                StartTrip(state, id, c.CommuteTicks[id], tick, Activity.TravelingHome);
                            }
                        }
                        break;

                    case Activity.Shopping:
                        if (tick >= c.ActivityEndTick[id])
                        {
                            StartTrip(state, id, c.ShopTicks[id], tick, Activity.TravelingHome);
                        }
                        break;
                }
            }
        }

        private static void DecideAtHome(CityState state, int id, int minute, int day, long tick)
        {
            CitizenStore c = state.Citizens;

            if (c.Workplace[id] != 0)
            {
                if (c.LastWorkDay[id] != day)
                {
                    int commuteMinutes = c.CommuteTicks[id] / SimTime.TicksPerMinute;
                    int departMinute = c.ShiftStartMinute[id] - commuteMinutes - DepartureSlackMinutes;
                    if (minute >= departMinute && minute < c.ShiftEndMinute[id])
                    {
                        c.LastWorkDay[id] = day;
                        StartTrip(state, id, c.CommuteTicks[id], tick, Activity.TravelingToWork);
                        return;
                    }
                }
            }
            else if (minute >= UnemployedShopFromMinute && minute < UnemployedShopToMinute
                && c.Needs[id * CitizenStore.FactorCount + CitizenStore.Consumption] < ShopThreshold)
            {
                StartTrip(state, id, c.ShopTicks[id], tick, Activity.TravelingToShop);
                return;
            }

            if (minute >= SleepMinute || minute < WakeMinute)
            {
                c.Activity[id] = (byte)Activity.Sleeping;
            }
        }

        private static void StartTrip(CityState state, int id, int durationTicks, long tick, Activity travelActivity)
        {
            state.Citizens.Activity[id] = (byte)travelActivity;
            state.Citizens.TripSlot[id] = state.Trips.Add(id, tick + durationTicks, durationTicks);
        }
    }
}
